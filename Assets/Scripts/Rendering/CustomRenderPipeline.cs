using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

/// <summary>
/// Renders things based on impostor layers??
/// </summary>
[CreateAssetMenu(menuName = "Rendering/Impostor Pipeline")]
public class CustomRenderPipeline : RenderPipelineAsset
{
    protected override IRenderPipeline InternalCreatePipeline()
    {
        return new ImpostorPipeline();
    }
}