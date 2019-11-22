using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** ImpMan stands for Impostor Manager. He's the dude that manages Impostifiers in the scene and controls when they update.
 * 
 * ImpMan is a singleton, if you're interested ladies ;) */
public class ImpMan : MonoBehaviour
{
    public static ImpMan singleton
    {
        get
        {
            if (!_singleton)
            {
                // find or create the singleton
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
    public List<Impostify> impostables = new List<Impostify>();
    public List<RenderTexture> impostorTextures = new List<RenderTexture>();
    public List<ImpostorSurface> impostorSurfaces = new List<ImpostorSurface>();
    public List<ImpostorBatch> impostorBatches;

    [Header("Impostor textures")]
    public Shader impostorShader;

    public int impostorTextureWidth = 1024;
    public int impostorTextureHeight = 1024;

    // Number of divisions splitting the impostor texture between impostors. A value of e.g. 2 means there is a 2x2 split
    public int impostorTextureDivisions = 1;

    [Header("Impostor regeneration")]
    public int framesPerImpostorUpdate = 1;

    private int frame = 0;

    private int lastUpdatedImpostor = 0;

    Rect[] impostorTextureDivisionUvs = new Rect[0];

    private void Awake()
    {
        /** Create a list of all impostifiable objects */
        impostables.AddRange(FindObjectsOfType<Impostify>());

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

    private void Update()
    {
        /* Here are some potential patterns for updating impostors:
         *
         * Update as many as possible at a set interval
         * Update a set maximum amount at a set interval
         * Update specific impostors depending on their changed angles and distances
         * 
         * The first is the easiest and perhaps the most useful to test.
         * 
         * With regards to shading, we could have a shader that writes the impostors to both the screen and the impostor texture at once
         */
        if (frame == 0)
        {
            // Clear all impostor textures
            ClearImpostorTextures();

            // Reserve texture fragments for each object
            foreach (Impostify impostable in impostables)
            {
                ImpostorSurface texFrag = ReserveImpostorSurface(512, 512);

                if (texFrag != null)
                {
                    texFrag.owner = impostable;
                    impostable.RegenerateImpostor(texFrag);
                }
            }
        }
        else if ((frame % framesPerImpostorUpdate) == 0 && impostorSurfaces.Count > 0)
        {
            for (int i = 0; i < impostorSurfaces.Count; i++)
            {
                lastUpdatedImpostor++;

                if (lastUpdatedImpostor >= impostorSurfaces.Count)
                {
                    lastUpdatedImpostor = 0;
                }

                // Skip impostors that are hold-spaceable
                if (!impostorSurfaces[lastUpdatedImpostor].owner.holdSpaceForImpostor || Input.GetKey(KeyCode.Space))
                {
                    impostorSurfaces[lastUpdatedImpostor].owner.RegenerateImpostor(impostorSurfaces[lastUpdatedImpostor]);
                    break;
                }
            }
        }

        frame++;
    }

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

    private void ClearImpostorTextures()
    {
        impostorSurfaces.Clear();
    }

    private void RenderImpostors()
    {
        // Render a group of impostors
        
        // Set up the main camera to render

        // For each impostor, the projection should be different...
        // ...OR use multiple render layers and put the impostors on that separate layer?
        // OR assign a proxy shader to the impostors, which renders 

        // Make sure 
    }
}

/** A fragment of an impostor texture that can be reserved for an object(s) */
public class ImpostorSurface
{
    /** Position and size of the texture fragment on the texture, in UV coordinates*/
    public Rect uvDimensions
    {
        get
        {
            return _uvDimensions;
        }
        set
        {
            _uvDimensions = value;

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

    /** Position and size of the texture fragment on the texture in pixels */
    public RectInt pixelDimensions
    {
        get
        {
            return _pixelDimensions;
        }
    }
    private RectInt _pixelDimensions;

    /** Texture that we're allowed to use */
    public RenderTexture texture
    {
        get
        {
            return _texture;
        }
        set
        {
            _texture = value;

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

    /** The impostor batch being used and the plane reserved from it */
    public ImpostorBatch batch;

    /** Index of the plane in the impostor batch */
    public int batchPlaneIndex;

    /** The owner of this surface */
    public Impostify owner;
};

public class ImpPlane
{
    /** UV coordinates across the plane */
    public Rect uvDimensions;


}