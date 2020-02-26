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
    public List<RenderTexture> impostorDepthTextures = new List<RenderTexture>();

    /// <summary>
    /// List of all known impostor surfaces that have been reserved
    /// </summary>
    public List<ImpostorSurface> impostorSurfaces = new List<ImpostorSurface>();

    /// <summary>
    /// List of all known impostor batches that have been created/reserved
    /// </summary>
    public List<ImpostorBatch> impostorBatches = new List<ImpostorBatch>();
    
    [Header("Impostor configuration")]
    [Tooltip("Whether impostors should be used at all")]
    public bool enableImpostors = true;

    [Tooltip("Whether to keep the impostor camera activated")]
    public bool activateImpostorCamera = false;

    [Tooltip("Renders to the main camera as well")]
    public bool enableMainCameraRendering = false;

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
    public int impostorBatchLayer = 30;
    public int numProgressiveRenderGroups = 2;

    /// <summary>
    /// A pre-calculated list of UVs for an impostor texture evenly divided into (impostorTextureDivisions*impostorTextureDivisions) surfaces
    /// </summary>
    Rect[] impostorTextureDivisionUvs = new Rect[0];

    /// <summary>
    /// Original far clip plane of the main camera
    /// </summary>
    float oldFarClipPlane;

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
    }

    private void Start()
    {
        foreach (ImpostorLayer layer in impostorLayers)
        {
            layer.surface = ReserveImpostorSurface(1024, 1024);
        }

        oldFarClipPlane = Camera.main.farClipPlane;
    }

    private void Update()
    {
        // Enable/disable impostors in general
        if (Input.GetKeyDown(KeyCode.I))
        {
            enableImpostors = !enableImpostors;

            if (!enableImpostors)
            {
                foreach (ImpostorLayer layer in impostorLayers)
                {
                    layer.impostorCamera.camera.enabled = false;
                }

                // allow the main camera to render everything again
                Camera.main.cullingMask = ~0;
                Camera.main.farClipPlane = oldFarClipPlane;
                Camera.main.layerCullDistances = new float[32];
            }

            foreach (ImpostorBatch batch in impostorBatches)
            {
                batch.GetComponent<MeshRenderer>().enabled = enableImpostors;
            }
        }

        if (enableImpostors)
        {
            // Refresh impostor layers
            foreach (ImpostorLayer layer in impostorLayers)
            {
                bool hasLayerUpdated = false;

                if (!freezeImpostors)
                {
                    if ((int)(Time.time * layer.updateRate) != (int)((Time.time - Time.deltaTime) * layer.updateRate))
                    {
                        RefreshImpostorLayer(layer);
                        hasLayerUpdated = true;
                    }
                }

                if (activateImpostorCamera)
                {
                    layer.impostorCamera.camera.enabled = true; // we wanted to disable it when hasUpdated is false, but that caused stutters by gfx.WaitForPresent...

                    if (!hasLayerUpdated)
                    {
                        // Don't render anything on this camera right now
                        layer.impostorCamera.camera.clearFlags = CameraClearFlags.Nothing;
                        layer.impostorCamera.camera.cullingMask = 0; 
                    }
                }
                else
                {
                    layer.impostorCamera.camera.enabled = false;
                }
            }

            // Set the main camera to render none of the bits between 1<<impostorRenderLayer and 1<<numProgressiveRenderGroups
            Camera.main.cullingMask = (int)~(~(~0u >> numProgressiveRenderGroups) >> (31 - impostorRenderLayer));

            //.....or just make it render things closer than the closest layer
            if (enableMainCameraRendering)
            {
                float closestRadius = Camera.main.farClipPlane;
                foreach (ImpostorLayer layer in impostorLayers)
                {
                    if (layer.minRadius < closestRadius)
                    {
                        closestRadius = layer.minRadius;
                    }
                }

                /// maybe use layer cull distances........
                float[] layerCullDistances = new float[32];

                for (int i = 0; i < numProgressiveRenderGroups; i++)
                {
                    layerCullDistances[impostorRenderLayer - i] = closestRadius + 1;
                }

                Camera.main.layerCullDistances = layerCullDistances;
                Camera.main.cullingMask = ~0;
            }
            else
            {
                Camera.main.layerCullDistances = new float[32];
            }
        }
    }

    bool didTheThing = false;

    void RefreshImpostorLayer(ImpostorLayer layer)
    {
        List<Impostify> impostablesToRender = new List<Impostify>();
        int numRenderers = 0;

        Benchmark benchCollect = Benchmark.Start();

        if (!didTheThing)
        {
            // Regroup impostors into appropriate layers
            foreach (Impostify impostable in impostables)
            {
                // Render this impostor
                impostablesToRender.Add(impostable);

                foreach (Renderer renderer in impostable.renderers)
                {
                    renderer.gameObject.layer = impostorRenderLayer - (numRenderers % numProgressiveRenderGroups);

                    numRenderers++;
                }
            }
        }
        didTheThing = true; // hack...
        numRenderers++; // more hack...

        // Don't bother if there's nothing to draw
        if (numRenderers == 0) return;

        benchCollect.Stop();

        // Render the objects in this impostor layer
        // Setup the culling masks
        layer.impostorCamera.camera.cullingMask = 1 << (impostorRenderLayer - layer.progressiveRenderGroup);

        if (layer.progressiveRenderGroup == 0)
        {
            // if we're rendering the first part of a progressive frame, swap the buffers so we see the previous image
            layer.surface.batch.texture = layer.surface.backBuffer;
            layer.surface.batch.depthTexture = layer.surface.backBufferDepth;
            layer.surface.batch.nearPlane = layer.minRadius;
            layer.surface.batch.farPlane = layer.maxRadius;
            layer.surface.SwapBuffers();

            // and place the previous impostor!
            layer.surface.batch.SetPlane(layer.surface.batchPlaneIndex, layer.nextImpostorPosition,
                layer.impostorCamera.transform.up * layer.nextImpostorSize.height, layer.impostorCamera.transform.right * layer.nextImpostorSize.width, layer.surface.uvDimensions);

            // now, frame the next impostor layer with the current camera position
            float nextWidth, nextHeight;
            layer.impostorCamera.FrameLayer(layer.renderDistance > 0 ? layer.renderDistance : layer.minRadius, Camera.main, out nextWidth, out nextHeight, out layer.nextImpostorPosition);

            layer.nextImpostorSize.width = nextWidth;
            layer.nextImpostorSize.height = nextHeight;

            // split scene into layered sections for render
            layer.impostorCamera.camera.nearClipPlane = layer.minRadius;
            layer.impostorCamera.camera.farClipPlane = layer.maxRadius;
        }

        if (activateImpostorCamera)
        {
            // camera will render naturally
            layer.impostorCamera.SetTargetSurface(layer.surface, layer.debugFillBackground ? new Color(1, 0, 0, 1) : new Color(1, 0, 0, 0));
        }
        else
        {
            // manually render the camera
            layer.impostorCamera.RenderToSurface(layer.surface, layer.debugFillBackground ? new Color(1, 0, 0, 1) : new Color(1, 0, 0, 0));
        }

        if (layer.progressiveRenderGroup > 0)
        {
            // only clear the image if we're building atop an existing frame
            layer.impostorCamera.camera.clearFlags = CameraClearFlags.Nothing;
        }

        // increase the progressive render index for this layer (todo: separate per layer)
        layer.progressiveRenderGroup = (layer.progressiveRenderGroup + 1) % numProgressiveRenderGroups;

        // Store the list of objects so we can re-enable them if they leave the radius during the next update
        layer.activeImpostors = impostablesToRender;

        //Debug.Log($"Collect: {benchCollect.ms} Render: {benchFrame.ms} EnableDisable: {benchEnableDisable.ms}");
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
            RenderTexture frontBufDepth = new RenderTexture(impostorTextureWidth, impostorTextureHeight, 16, RenderTextureFormat.Depth);
            RenderTexture backBuf = new RenderTexture(impostorTextureWidth, impostorTextureHeight, 16);
            RenderTexture backBufDepth = new RenderTexture(impostorTextureWidth, impostorTextureHeight, 16, RenderTextureFormat.Depth);
            ImpostorBatch batch = new GameObject("_ImpostorBatch_", typeof(ImpostorBatch)).GetComponent<ImpostorBatch>();

            backBuf.format = RenderTextureFormat.ARGBHalf;
            frontBuf.format = RenderTextureFormat.ARGBHalf;

            impostorTextures.Add(frontBuf);
            impostorTextures.Add(backBuf);
            impostorDepthTextures.Add(frontBufDepth);
            impostorDepthTextures.Add(backBufDepth);
            impostorBatches.Add(batch);

            batch.texture = backBuf;
            batch.gameObject.layer = impostorBatchLayer;
        }

        // Create the surface
        ImpostorSurface fragment = new ImpostorSurface
        {
            backBuffer = impostorTextures[textureIndex * 2 + 1],
            frontBuffer = impostorTextures[textureIndex * 2],
            backBufferDepth = impostorDepthTextures[textureIndex * 2 + 1],
            frontBufferDepth = impostorDepthTextures[textureIndex * 2],
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

        // Copy new ones over
        impostorLayers = new ImpostorLayer[configuration.layers.Length];

        for (int i = 0; i < configuration.layers.Length; i++)
        {
            impostorLayers[i] = configuration.layers[i].Clone();
        }

        InitImpostorLayers();
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

    /// <summary>
    /// Initialises all applicable impostor layers
    /// </summary>
    private void InitImpostorLayers()
    {
        // Remove previous impostor surfaces
        ClearImpostorSurfaces();

        foreach (ImpostorLayer layer in impostorLayers)
        {
            // Create the impostor camera
            layer.impostorCamera = CreateImpostorCamera();

            // Reserve the impostor surface
            layer.surface = ReserveImpostorSurface(0, 0);
        }
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

[System.Serializable]
/// <summary>
/// A portion of an impostor texture and associated impostor batch/index that can be reserved for an impostor
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
    public RenderTexture frontBufferDepth;

    /// <summary>
    /// The back buffer texture, used for renders
    /// </summary>
    public RenderTexture backBuffer;
    public RenderTexture backBufferDepth;

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

        swap = backBufferDepth;
        backBufferDepth = frontBufferDepth;
        frontBufferDepth = swap;
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
    /// The distance to render this impostor at. If 0, minRadius is used
    /// </summary>
    public float renderDistance;

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
    /// The impostor camera used for rendering this layer
    /// </summary>
    public ImpostorCamera impostorCamera;

    /// <summary>
    /// Current mask index for progressively-rendered objects
    /// </summary>
    public int progressiveRenderGroup;

    /// <summary>
    /// The position of the impostor currently being rendered/displayed
    /// </summary>
    public Vector3 nextImpostorPosition;

    /// <summary>
    /// The size of the impostor currently being rendered/displayed
    /// </summary>
    public Rect nextImpostorSize;

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