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
    public List<ImpostorBatch> impostorBatches;

    
    [Header("Impostor configuration")]
    [Tooltip("Whether impostors should be used at all")]
    public bool enableImpostors = true;

    public ImpostorLayerConfiguration[] layerConfigurations;

    [Header("Impostor textures")]
    [Tooltip("The shader to render impostors with")]
    public Shader impostorShader;

    [Tooltip("The width, in pixels, of the internal impostor textures")]
    public int impostorTextureWidth = 1024;
    [Tooltip("The height, in pixels, of the internal impostor textures")]
    public int impostorTextureHeight = 1024;

    [Tooltip("Number of divisions splitting the impostor texture between impostor surfaces. A value of e.g. 2 means there is a 2x2 split")]
    public int impostorTextureDivisions = 1;

    [Header("Impostor regeneration")]
    [Tooltip("Test: Frames to pass before updating an impostor(s)")]
    public int framesPerImpostorUpdate = 1;
    
    /// <summary>
    /// The camera that snapshots the impostors. We only need one such camera in the scene
    /// </summary>
    public Camera impostorCamera;

    /// <summary>
    /// A frame counter since the ImpMan's creation
    /// </summary>
    private int frame = 0;

    /// <summary>
    /// The index of the last updated impostor
    /// </summary>
    private int lastUpdatedImpostor = 0;

    /// <summary>
    /// A pre-calculated list of UVs for an impostor texture evenly divided into (impostorTextureDivisions*impostorTextureDivisions) surfaces
    /// </summary>
    Rect[] impostorTextureDivisionUvs = new Rect[0];

    private void Awake()
    {
        // Create a list of all impostifiable objects
        impostables.AddRange(FindObjectsOfType<Impostify>());

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

    private void Update()
    {
        // This function currently contains a lot of debugging features
        bool doUpdateImpostors = ((frame % framesPerImpostorUpdate) == 0) || Input.GetKeyDown(KeyCode.Space);

        if (enableImpostors)
        {
            if (frame == 0)
            {
                // Clear all impostor textures
                ClearImpostorSurfaces();

                // Reserve and render texture fragments for each object
                foreach (Impostify impostable in impostables)
                {
                    ImpostorSurface surface = ReserveImpostorSurface(512, 512);

                    if (surface != null)
                    {
                        surface.owner = impostable;
                        impostable.RerenderImpostor(surface);
                    }
                }
            }
            else if (doUpdateImpostors && impostorSurfaces.Count > 0)
            {
                // Refresh the next impostor(s)
                for (int i = 0; i < impostorSurfaces.Count; i++)
                {
                    lastUpdatedImpostor++;

                    if (lastUpdatedImpostor >= impostorSurfaces.Count)
                    {
                        lastUpdatedImpostor = 0;
                    }

                    // Only process impostors that don't enable holdSpaceForImpostor
                    if (!impostorSurfaces[lastUpdatedImpostor].owner.holdSpaceForImpostor || Input.GetKey(KeyCode.Space))
                    {
                        impostorSurfaces[lastUpdatedImpostor].owner.RerenderImpostor(impostorSurfaces[lastUpdatedImpostor]);
                        break;
                    }
                }
            }
        }

        // Enable/disable impostors in general
        if (Input.GetKeyDown(KeyCode.Space))
        {
            enableImpostors = !enableImpostors;

            foreach (ImpostorSurface surface in impostorSurfaces)
            {
                surface.owner.isImpostorVisible = enableImpostors;
            }
            foreach (ImpostorBatch batch in impostorBatches)
            {
                batch.GetComponent<MeshRenderer>().enabled = enableImpostors;
            }
        }

        // Take screenshots
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ScreenCapture.CaptureScreenshot($"Screenshot {System.DateTime.Now.ToLongTimeString().Replace(":", "-")}.png");
        }

        frame++;
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
        while (textureIndex >= impostorTextures.Count)
        {
            impostorTextures.Add(new RenderTexture(impostorTextureWidth, impostorTextureHeight, 16));
            impostorBatches.Add(new GameObject("_ImpostorBatch_", typeof(ImpostorBatch)).GetComponent<ImpostorBatch>());

            impostorBatches[impostorBatches.Count - 1].texture = impostorTextures[impostorTextures.Count - 1];
        }

        // Create the surface
        ImpostorSurface fragment = new ImpostorSurface
        {
            texture = impostorTextures[textureIndex],
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
    /// Renders a group of impostors (WIP)
    /// </summary>
    private void RenderImpostors()
    {
        // Render a group of impostors
        
        // Set up the main camera to render

        // For each impostor, the projection should be different...
        // ...OR use multiple render layers and put the impostors on that separate layer?
        // OR assign a proxy shader to the impostors, which renders 

        // Make sure 
    }
    
    /// <summary>
    /// Creates the impostor camera
    /// </summary>
    private Camera CreateImpostorCamera()
    {
        GameObject cameraObject = new GameObject("_ImpostorCamera_", typeof(Camera));

        // Disable it for now (so it isn't used as an actual camera)
        cameraObject.GetComponent<Camera>().enabled = false;
        return cameraObject.GetComponent<Camera>();
    }
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
            if (texture)
            {
                _pixelDimensions = new RectInt(
                    Mathf.RoundToInt(value.x * texture.width),
                    Mathf.RoundToInt(value.y * texture.height),
                    Mathf.RoundToInt(value.width * texture.width),
                    Mathf.RoundToInt(value.height * texture.height)
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
    public RenderTexture texture
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
};

/// <summary>
/// Impostor layer configuration settings
/// </summary>
[System.Serializable]
public class ImpostorLayerConfiguration
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


}