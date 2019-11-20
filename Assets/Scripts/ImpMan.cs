﻿using System.Collections;
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

    public List<RenderTexture> impostorTextures = new List<RenderTexture>();

    public List<ImpostorSurface> impostorSurfaces = new List<ImpostorSurface>();

    public List<Impostify> impostables = new List<Impostify>();

    [Header("Impostor textures")]
    public int impostorTextureWidth = 1024;
    public int impostorTextureHeight = 1024;

    public int framesPerImpostorUpdate = 1;

    private int frame = 0;

    private int lastUpdatedImpostor = 0;

    private void Awake()
    {
        /** Add a first impostor texture */
        impostorTextures.Add(new RenderTexture(impostorTextureWidth, impostorTextureHeight, 16));

        /** Create a list of all impostifiable objects */
        impostables.AddRange(FindObjectsOfType<Impostify>());
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
            lastUpdatedImpostor++;

            if (lastUpdatedImpostor >= impostorSurfaces.Count)
            {
                lastUpdatedImpostor = 0;
            }

            impostorSurfaces[lastUpdatedImpostor].owner.RegenerateImpostor(impostorSurfaces[lastUpdatedImpostor]);
        }

        frame++;
    }

    private ImpostorSurface ReserveImpostorSurface(int width, int height)
    {
        int textureIndex = impostorSurfaces.Count / 4;
        int uvIndex = impostorSurfaces.Count % 4;

        while (textureIndex >= impostorTextures.Count)
        {
            impostorTextures.Add(new RenderTexture(impostorTextureWidth, impostorTextureHeight, 16));
        }

        RenderTexture texture = impostorTextures[textureIndex];
        Vector2[] uvSequence = new Vector2[] { new Vector2(0, 0), new Vector2(0.5f, 0), new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f) };

        ImpostorSurface fragment = new ImpostorSurface
        {
            texture = texture,
            uvDimensions = new Rect(uvSequence[uvIndex], new Vector2(0.5f, 0.5f)),
        };

        impostorSurfaces.Add(fragment);
        return fragment;
    }

    private void ClearImpostorTextures()
    {
        impostorSurfaces.Clear();
        /*foreach (ImpTextureFragment fragment in impostorTextureFragments)
        {
            fragment.
        }*/
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

    /** The owner of this texture fragment */
    public Impostify owner;
};

public class ImpPlane
{
    /** UV coordinates across the plane */
    public Rect uvDimensions;


}