using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class ImpostorPipeline : RenderPipeline
{

	public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
	{
		base.Render(renderContext, cameras);
		
		foreach (Camera camera in cameras)
		{
			Render(renderContext, camera);
		}
	}

	private void Render(ScriptableRenderContext context, Camera camera)
	{
		CommandBuffer commands = new CommandBuffer()
		{
			name = "Camera Rendering"
		};

		// Setup culling parameters, etc
		ScriptableCullingParameters cullingParameters;

		CullResults.GetCullingParameters(camera, out cullingParameters);

		//if (camera.name == "_ImpostorCamera_")
		{
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

			planes[4].distance /= 200.0f;
			planes[5].distance /= 500.0f;

			for (int i = 0; i < 6; i++)
			{
				cullingParameters.SetCullingPlane(i, planes[i]);
				Debug.Log($"Plane{i}: {cullingParameters.GetCullingPlane(i).normal}, {cullingParameters.GetCullingPlane(i).distance}");
			}
		}

		CullResults cull = CullResults.Cull(ref cullingParameters, context);

#if UNITY_EDITOR
		if (camera.cameraType == CameraType.SceneView)
		{
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
#endif

		// Setup matrices etc
		context.SetupCameraProperties(camera);

		// Clear the back buffer
		if (camera.clearFlags != CameraClearFlags.Nothing)
		{
			commands.ClearRenderTarget(camera.clearFlags != CameraClearFlags.Nothing, camera.clearFlags == CameraClearFlags.Color || camera.clearFlags == CameraClearFlags.SolidColor, camera.backgroundColor, 1.0f);
			context.ExecuteCommandBuffer(commands);
			commands.Clear();
		}
		commands.EndSample("Camera Rendering");

		commands.Release();

		// Render objects
		DrawRendererSettings drawSettings = new DrawRendererSettings(camera, new ShaderPassName("ForwardBase"));
		FilterRenderersSettings filterSettings = new FilterRenderersSettings(true);
		
		// Setup settings
		drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
		drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
		drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
		drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
		drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
		drawSettings.SetShaderPassName(6, new ShaderPassName("SRPDefaultUnlit"));

		// Draw opaques
		drawSettings.sorting.flags = SortFlags.CommonOpaque;
		filterSettings.renderQueueRange = RenderQueueRange.opaque;
		context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

		// Draw the skybox (doe after opaques to reduce overdraw, before transparents for no weirdness
		if (camera.clearFlags.HasFlag(CameraClearFlags.Skybox))
		{
			context.DrawSkybox(camera);
		}

		// Draw transparents
		drawSettings.sorting.flags = SortFlags.CommonTransparent;
		filterSettings.renderQueueRange = RenderQueueRange.transparent;
		context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

		// Done!
		context.Submit();
	}
}
