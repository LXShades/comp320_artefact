using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A container of ImpostorLayer setups for the ImpMan to use
/// </summary>
[System.Serializable]
public class ImpostorConfiguration
{
    /// <summary>
    /// Optional name for this configuration
    /// </summary>
    public string name;

    /// <summary>
    /// List of layers to impostify
    /// </summary>
    public ImpostorLayer[] layers;

    /// <summary>
    /// Whether the main camera should render some of the foreground in addition to the layers
    /// </summary>
    public bool enableMainCameraRendering = true;

    /// <summary>
    /// If non-zero, caps the FPS. If 0, FPS is unlimited
    /// </summary>
    public int fpsCap = 0;
}