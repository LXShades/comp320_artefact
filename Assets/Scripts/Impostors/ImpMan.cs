#define USE_MASKS_FOR_HIDING
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** ImpMan stands for Impostor Manager. He's the dude that manages Impostifiers in the scene and controls when they update.
 * 
 * ImpMan is a singleton, if you're interested ladies ;) */
public class ImpMan : MonoBehaviour
{
    /// <summary>
    /// Gets or creates the singleton ImpMan
    /// </summary>
    public static ImpMan singleton
    {
        get
        {
            if (!_singleton)
            {
                // Find or create the singleton
                _singleton = GameObject.FindObjectOfType<ImpMan>();

                if (!_singleton)
                {
                    _singleton = new GameObject("ImpMan", typeof(ImpMan)).GetComponent<ImpMan>();
                }
            }
            
            return _singleton;
        }
    }
    private static ImpMan _singleton;

    [Header("Resources (runtime, don't change!)")]
    /// <summary>
    /// List of all impostables in the scene
    /// </summary>
    public List<Impostify> impostables = new List<Impostify>();

    /// <summary>
    /// List of all impostor texture resources currently being used
    /// </summary>
    public List<RenderTexture> impostorTextures = new List<RenderTexture>();

    /// <summary>
    /// List of all known impostor surfaces that have been reserved
    /// </summary>
    public List<ImpostorSurface> impostorSurfaces = new List<ImpostorSurface>();

    /// <summary>
    /// List of all known impostor batches that have been created/reserved
    /// </summary>
    public List<ImpostorBatch> impostorBatches = new List<ImpostorBatch>();

    public float debugImpostorDepth = 5.0f;

    
    [Header("Impostor configuration")]
    [Tooltip("Whether impostors should be used at all")]
    public bool enableImpostors = true;

    [Tooltip("Whether to keep the impostor camera activated")]
    public bool activateImpostorCamera = false;

    [Tooltip("Freezes active impostor systems")]
    public bool freezeImpostors = false;

    [Tooltip("Initial configurations for impostor layers")]
    public ImpostorLayer[] impostorLayers = new ImpostorLayer[0];

    [Header("Impostor textures")]
    [Tooltip("The shader to render impostors with")]
    public Shader impostorShader;

    [Tooltip("The width, in pixels, of the internal impostor textures")]
    public int impostorTextureWidth = 1024;
    [Tooltip("The height, in pixels, of the internal impostor textures")]
    public int impostorTextureHeight = 1024;

    [Tooltip("Number of divisions splitting the impostor texture between impostor surfaces. A value of e.g. 2 means there is a 2x2 split")]
    public int impostorTextureDivisions = 1;

    [Header("Impostor rendering settings")]
    [Tooltip("Render layer to draw impostors on")]
    public int impostorRenderLayer = 31;
    public int numRenderLayerDivisions = 2;
    
    [Header("Miscellaneous")]
    /// <summary>
    /// The camera that snapshots the impostors. We only need one such camera in the scene
    /// </summary>
    public ImpostorCamera impostorCamera;

    /// <summary>
    /// A pre-calculated list of UVs for an impostor texture evenly divided into (impostorTextureDivisions*impostorTextureDivisions) surfaces
    /// </summary>
    Rect[] impostorTextureDivisionUvs = new Rect[0];

    private void Awake()
    {
        // Create a list of all impostifiable objects
        impostables.AddRange(FindObjectsOfType<Impostify>());
        impostables.RemoveAll(impostify => impostify.enabled == false);

        // Pregenerate the predefined division slot dimension and positions
        Vector2 uvSize = new Vector2(1.0f / (float)impostorTextureDivisions, 1.0f / (float)impostorTextureDivisions);
        impostorTextureDivisionUvs = new Rect[impostorTextureDivisions * impostorTextureDivisions];
        for (int x = 0; x < impostorTextureDivisions; x++)
        {
            for (int y = 0; y < impostorTextureDivisions; y++)
            {
                int index = y * impostorTextureDivisions + x;
                impostorTextureDivisionUvs[index].x = x * uvSize.x;
                impostorTextureDivisionUvs[index].y = y * uvSize.y;
                impostorTextureDivisionUvs[index].size = uvSize;
            }
        }

        // Create the impostor camera
        impostorCamera = CreateImpostorCamera();
    }

    private void Start()
    {
        foreach (ImpostorLayer layer in impostorLayers)
        {
            layer.surface = ReserveImpostorSurface(1024, 1024);
        }
    }

    private void Update()
    {
        // Enable/disable impostors in general
        if (Input.GetKeyDown(KeyCode.Space))
        {
            enableImpostors = !enableImpostors;

            if (!enableImpostors)
            {
                impostorCamera.camera.enabled = false;

                // allow the main camera to render everything again
                Camera.main.cullingMask = ~0;
            }

            foreach (ImpostorBatch batch in impostorBatches)
            {
                batch.GetComponent<MeshRenderer>().enabled = enableImpostors;
            }
        }

        if (enableImpostors)
        {
            bool hasUpdated = false;

            if (!freezeImpostors)
            {
                // update impostor layers
                for (int i = 0; i < impostorLayers.Length; i++)
                {
                    hasUpdated |= RefreshImpostorLayer(impostorLayers[i]);
                }
            }

            if (activateImpostorCamera)
            {
                impostorCamera.camera.enabled = true; // we wanted to disable it when hasUpdated is false, but that caused stutters by gfx.WaitForPresent...

                if (!hasUpdated)
                {
                    impostorCamera.camera.clearFlags = CameraClearFlags.Nothing;
                    impostorCamera.camera.cullingMask = 0; // don't render anything right now
                }
            }
            else
            {
                impostorCamera.camera.enabled = false;
            }
        }

        // Take screenshots
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ScreenCapture.CaptureScreenshot($"Screenshot {System.DateTime.Now.ToLongTimeString().Replace(":", "-")}.png");
        }
    }

    bool didTheThing = false;
    int progressiveRenderIndex = 0;

    // remove later!!!
    float impostorWidth = 0;
    float impostorHeight = 0;
    Vector3 impostorPosition;

    bool RefreshImpostorLayer(ImpostorLayer layer)
    {
        if ((int)(Time.time * layer.updateRate) == (int)((Time.time - Time.deltaTime) * layer.updateRate))
        {
            return false; // we're not refreshing yet
        }

        // collect all impostors ahead of the camera at the distance into th elayer
        Vector3 cameraForward = Camera.main.transform.forward;
        float cameraForwardBase = Vector3.Dot(cameraForward, Camera.main.transform.position);
        List<Impostify> impostablesToRender = new List<Impostify>();
        int numRenderers = 0;

        Benchmark benchCollect = Benchmark.Start();

        if (!didTheThing)
        {
            foreach (Impostify impostable in impostables)
            {
                // Render this impostor
                impostablesToRender.Add(impostable);

                foreach (Renderer renderer in impostable.renderers)
                {
                    //float depth = Vector3.Dot(renderer.bounds.center, cameraForward) - cameraForwardBase - renderer.bounds.size.magnitude;

                    //if (depth > layer.minRadius && depth < layer.maxRadius)
                    {
                        renderer.gameObject.layer = impostorRenderLayer + (numRenderers % numRenderLayerDivisions);

                        numRenderers++;
                    }
                }
            }
        }
        didTheThing = true; // hack...
        numRenderers++; // more hack...

        // Don't bother if there's nothing to draw
        if (numRenderers == 0) return false;

        benchCollect.Stop();

        // Render the objects in this impostor layer
        // Setup the culling masks
        int impostorLayerCullingMask = 1 << impostorRenderLayer;
        for (int i = 0; i < numRenderLayerDivisions; i++)
        {
            impostorLayerCullingMask |= 1 << (impostorRenderLayer + i);
        }

        // main camera renders none of the impostable stuff, impostor camera renders the current progressive mask
        Camera.main.cullingMask = ~impostorLayerCullingMask;
        impostorCamera.camera.cullingMask = 1 << (impostorRenderLayer + progressiveRenderIndex);

        if (progressiveRenderIndex == 0)
        {
            // if we're rendering the first part of a progressive frame, swap the buffers so we see the previous image
            layer.surface.batch.texture = layer.surface.backBuffer;
            layer.surface.SwapBuffers();

            // and place the impostor!
            layer.surface.batch.SetPlane(layer.surface.batchPlaneIndex, impostorPosition,
                impostorCamera.transform.up * impostorHeight, impostorCamera.transform.right * impostorWidth, layer.surface.uvDimensions);

            // frame the next impostor layer with the current camera position
            impostorCamera.FrameLayer(debugImpostorDepth, Camera.main, out impostorWidth, out impostorHeight, out impostorPosition);
        }

        if (activateImpostorCamera)
        {
            // camera will render naturally
            impostorCamera.SetTargetSurface(layer.surface, layer.debugFillBackground ? new Color(1, 0, 0, 1) : new Color(1, 0, 0, 0));
        }
        else
        {
            // manually render the camera
            impostorCamera.RenderToSurface(layer.surface, layer.debugFillBackground ? new Color(1, 0, 0, 1) : new Color(1, 0, 0, 0));
        }

        if (progressiveRenderIndex > 0)
        {
            // only clear the image if we're building atop an existing frame
            impostorCamera.camera.clearFlags = CameraClearFlags.Nothing;
        }

        // increase the progressive render index for this layer (todo: separate per layer)
        progressiveRenderIndex = (progressiveRenderIndex + 1) % numRenderLayerDivisions;

        // Store the list of objects so we can re-enable them if they leave the radius during the next update
        layer.activeImpostors = impostablesToRender;

        //Debug.Log($"Collect: {benchCollect.ms} Render: {benchFrame.ms} EnableDisable: {benchEnableDisable.ms}");

        return true;
    }

    /// <summary>
    /// Reserves and returns a new impostor surface.
    /// </summary>
    /// <param name="width">Ignored [todo]. The desired width of the impostor</param>
    /// <param name="height">Ignored [todo]. The desired height of the impostor</param>
    /// <returns>A new ImpostorSurface</returns>
    private ImpostorSurface ReserveImpostorSurface(int width, int height)
    {
        int textureIndex = impostorSurfaces.Count / impostorTextureDivisions;
        int uvIndex = impostorSurfaces.Count % impostorTextureDivisions;

        // if we need more new textures, add them here
        if (textureIndex * 2 >= impostorTextures.Count)
        {
            RenderTexture frontBuf = new RenderTexture(impostorTextureWidth, impostorTextureHeight, 16);
            RenderTexture backBuf = new RenderTexture(impostorTextureWidth, impostorTextureHeight, 16);
            ImpostorBatch batch = new GameObject("_ImpostorBatch_", typeof(ImpostorBatch)).GetComponent<ImpostorBatch>();

            backBuf.format = RenderTextureFormat.ARGBHalf;
            frontBuf.format = RenderTextureFormat.ARGBHalf;

            impostorTextures.Add(frontBuf);
            impostorTextures.Add(backBuf);
            impostorBatches.Add(batch);

            batch.texture = backBuf;
        }

        // Create the surface
        ImpostorSurface fragment = new ImpostorSurface
        {
            backBuffer = impostorTextures[textureIndex * 2 + 1],
            frontBuffer = impostorTextures[textureIndex * 2],
            uvDimensions = impostorTextureDivisionUvs[uvIndex],
            batch = impostorBatches[textureIndex],
            batchPlaneIndex = impostorBatches[textureIndex].ReservePlane()
        };

        impostorSurfaces.Add(fragment);
        return fragment;
    }

    /// <summary>
    /// Clears all impostor surfaces. This does not free the resources they used.
    /// </summary>
    private void ClearImpostorSurfaces()
    {
        impostorSurfaces.Clear();
    }
    
    /// <summary>
    /// Sets up the impostor layers to match the configuration given in the list
    /// </summary>
    public void SetConfiguration(ImpostorConfiguration configuration)
    {
        // Restore impostors contianed in previous layers
        foreach (ImpostorLayer layer in impostorLayers)
        {
            foreach (Impostify impostable in layer.activeImpostors)
            {
                foreach (Renderer renderer in impostable.renderers)
                {
                    renderer.gameObject.layer = 0;
                    renderer.enabled = true;
                }
            }
        }

        // Remove previous layers
        ClearImpostorSurfaces();

        // Copy new ones over
        impostorLayers = new ImpostorLayer[configuration.layers.Length];

        for (int i = 0; i < configuration.layers.Length; i++)
        {
            impostorLayers[i] = configuration.layers[i].Clone();

            impostorLayers[i].surface = ReserveImpostorSurface(0, 0);
        }
    }

    /// <summary>
    /// Creates the impostor camera
    /// </summary>
    private ImpostorCamera CreateImpostorCamera()
    {
        GameObject cameraObject = new GameObject("_ImpostorCamera_", typeof(ImpostorCamera));

        // Disable it for now (so it isn't used as an actual camera)
        return cameraObject.GetComponent<ImpostorCamera>();
    }
}

/// <summary>
/// A container of ImpostorLayer setups for the ImpMan to use
/// </summary>
[System.Serializable]
public class ImpostorConfiguration
{
    /// <summary>
    /// List of layers to impostify
    /// </summary>
    public ImpostorLayer[] layers;
}

/// <summary>
/// A fragment of an impostor texture that can be reserved for an object(s)
/// </summary>
public class ImpostorSurface
{
    /// <summary>
    /// Position and size of the surface on the texture, in UV coordinates
    /// </summary>
    public Rect uvDimensions
    {
        get
        {
            return _uvDimensions;
        }
        set
        {
            _uvDimensions = value;

            // Refresh the surface pixel dimensions
            if (frontBuffer)
            {
                _pixelDimensions = new RectInt(
                    Mathf.RoundToInt(value.x * frontBuffer.width),
                    Mathf.RoundToInt(value.y * frontBuffer.height),
                    Mathf.RoundToInt(value.width * frontBuffer.width),
                    Mathf.RoundToInt(value.height * frontBuffer.height)
                );
            }
        }
    }
    private Rect _uvDimensions;

    /// <summary>
    /// Position and size of the surface on the texture, in pixels
    /// </summary>
    public RectInt pixelDimensions
    {
        get
        {
            return _pixelDimensions;
        }
    }
    private RectInt _pixelDimensions;

    /// <summary>
    /// Texture that this surface uses
    /// </summary>
    public RenderTexture frontBuffer
    {
        get
        {
            return _texture;
        }
        set
        {
            _texture = value;

            // Refresh the surface pixel dimensions
            if (value)
            {
                _pixelDimensions = new RectInt(
                    Mathf.RoundToInt(uvDimensions.x * value.width),
                    Mathf.RoundToInt(uvDimensions.y * value.height),
                    Mathf.RoundToInt(uvDimensions.width * value.width),
                    Mathf.RoundToInt(uvDimensions.height * value.height)
                );
            }
            else
            {
                _pixelDimensions = new RectInt();
            }
        }
    }
    private RenderTexture _texture;

    /// <summary>
    /// The back buffer texture, used for renders
    /// </summary>
    public RenderTexture backBuffer;

    /// <summary>
    /// The impostor batch being used and the plane reserved from it 
    /// </summary>
    public ImpostorBatch batch;

    /// <summary>
    /// Index of the plane in the impostor batch, if applicable
    /// </summary>
    public int batchPlaneIndex;

    /// <summary>
    /// The owner of this surface
    /// </summary>
    public Impostify owner;

    /// <summary>
    /// Swaps the front and back buffers
    /// </summary>
    public void SwapBuffers()
    {
        RenderTexture swap = frontBuffer;
        frontBuffer = backBuffer;
        backBuffer = swap;
    }
};

/// <summary>
/// Impostor layer configuration settings
/// </summary>
[System.Serializable]
public class ImpostorLayer
{
    /// <summary>
    /// How many updates per second this layer should have
    /// </summary>
    public float updateRate;

    /// <summary>
    /// The minimum radius of this impostor layer. Objects crossing this boundary are probably in the previous layer
    /// </summary>
    public float minRadius;

    /// <summary>
    /// The maximum radius of this impostor layer. Objects past this boundary are probably in the next layer
    /// </summary>
    public float maxRadius;

    /// <summary>
    /// Whether to fill the impostor's background for debugging purposes
    /// </summary>
    public bool debugFillBackground;

    /// <summary>
    /// The impostor surface this layer uses
    /// </summary>
    public ImpostorSurface surface;

    /// <summary>
    /// A list of objects that are currently included in this impostor
    /// </summary>
    public List<Impostify> activeImpostors = new List<Impostify>();

    /// <summary>
    /// Copies data from a different impostor layer to this one
    /// </summary>
    public ImpostorLayer Clone()
    {
        ImpostorLayer clone = (ImpostorLayer)this.MemberwiseClone();

        clone.activeImpostors = new List<Impostify>();
        clone.surface = null;

        return clone;
    }
}