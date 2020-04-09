using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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