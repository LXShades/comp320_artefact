using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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