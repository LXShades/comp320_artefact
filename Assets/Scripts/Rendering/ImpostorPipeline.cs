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

		// Setup culling params using the main camera (reuse for impostors)
		float farClip = 0;
		ScriptableCullingParameters cullingParameters;

		foreach (Camera camera in cameras)
		{
			if (camera.GetComponent<ImpostorCamera>() && camera.cullingMask != 0)
			{
				farClip = Mathf.Max(camera.farClipPlane, farClip);
			}
		}

		CullResults.GetCullingParameters(Camera.main, out cullingParameters);

		if (farClip > 0)
		{
			for (int i = 0; i < 32; i++)
			{
				cullingParameters.SetLayerCullDistance(i, Mathf.Max(farClip, cullingParameters.GetLayerCullDistance(i)));
			}
		}

		CullResults cull = CullResults.Cull(ref cullingParameters, renderContext);

		// Render all cameras
		foreach (Camera camera in cameras)
		{
			Render(renderContext, camera, ref cull);
		}

		//Camera.main.farClipPlane = oldFarClip;
	}

	private void Render(ScriptableRenderContext context, Camera camera, ref CullResults cull)
	{
		CommandBuffer commands = new CommandBuffer()
		{
			name = "Camera Rendering"
		};

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

		// this draws the depth shader thing as it's on the unlit pass
		{
			var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"));
			var filterSettings = new FilterRenderersSettings(true) { renderQueueRange = RenderQueueRange.opaque };

			filterSettings.layerMask = camera.cullingMask;
			filterSettings.renderingLayerMask = (uint)camera.cullingMask;

			drawSettings.sorting.flags = SortFlags.CommonOpaque;

			context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);
		}

		{
			// Render main objects
			DrawRendererSettings drawSettings = new DrawRendererSettings(camera, new ShaderPassName("ForwardBase"));
			FilterRenderersSettings filterSettings = new FilterRenderersSettings(true);

			// Setup settings
			drawSettings.SetShaderPassName(1, new ShaderPassName("ForwardBase"));
			//drawSettings.SetShaderPassName(2, new ShaderPassName("PrepassBase"));
			//drawSettings.SetShaderPassName(3, new ShaderPassName("Always"));
			//drawSettings.SetShaderPassName(4, new ShaderPassName("Vertex"));
			//drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLMRGBM"));
			//drawSettings.SetShaderPassName(6, new ShaderPassName("VertexLM"));
			drawSettings.rendererConfiguration = RendererConfiguration.PerObjectLightProbe | RendererConfiguration.PerObjectLightmaps;

			//SetupLights(ref cull, ref context);
			// Draw opaques
			drawSettings.sorting.flags = SortFlags.CommonOpaque;
			filterSettings.renderQueueRange = RenderQueueRange.opaque;
			context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);



			/*DrawRendererSettings fuck = new DrawRendererSettings(camera, new ShaderPassName("ForwardAdd"));
			fuck.SetShaderPassName(1, new ShaderPassName("ForwardAdd"));
			context.DrawRenderers(cull.visibleRenderers, ref fuck, filterSettings);*/

			// Draw the skybox (doe after opaques to reduce overdraw, before transparents for no weirdness
			if (camera.clearFlags.HasFlag(CameraClearFlags.Skybox))
			{
				context.DrawSkybox(camera);
			}

			// Draw transparents
			drawSettings.sorting.flags = SortFlags.CommonTransparent;
			filterSettings.renderQueueRange = RenderQueueRange.transparent;
			context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

		}

		{
			// Draw shadows??
			DrawRendererSettings drawSettings = new DrawRendererSettings(camera, new ShaderPassName("ShadowCaster"));
			DrawShadowsSettings shadowSettings = new DrawShadowsSettings(cull, 0);
			FilterRenderersSettings filterSettings = new FilterRenderersSettings(true);

			filterSettings.renderQueueRange = RenderQueueRange.opaque;
			//context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

			//context.DrawShadows(ref shadowSettings);
		}

		{
			// Draw UI?
			DrawRendererSettings drawSettings = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"));
			FilterRenderersSettings filterSettings = new FilterRenderersSettings(true);

			filterSettings.renderQueueRange = RenderQueueRange.transparent;
			drawSettings.sorting.flags = SortFlags.CommonTransparent;
			drawSettings.SetShaderPassName(1, new ShaderPassName("Default"));

			context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);
		}

		// Done!
		context.Submit();
	}

	void SetupLights(ref CullResults cull, ref ScriptableRenderContext context)
	{
		CommandBuffer cmdLighting = new CommandBuffer() { name = "Lighting Stuff" };
		int mainLightIndex = -1;
		Light mainLight = null;

		Vector4[] lightPositions = new Vector4[8];
		Vector4[] lightColors = new Vector4[8];
		Vector4[] lightAttn = new Vector4[8];
		Vector4[] lightSpotDir = new Vector4[8];

		//Initialise values
		for (int i = 0; i < 8; i++)
		{
			lightPositions[i] = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
			lightColors[i] = Color.black;
			lightAttn[i] = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
			lightSpotDir[i] = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
		}

		for (int i = 0; i < cull.visibleLights.Count; i++)
		{
			VisibleLight light = cull.visibleLights[i];

			if (mainLightIndex == -1) //Directional light
			{
				if (light.lightType == LightType.Directional)
				{
					Vector4 dir = light.localToWorld.GetColumn(2);
					lightPositions[0] = new Vector4(-dir.x, -dir.y, -dir.z, 0);
					lightColors[0] = light.light.color;

					float lightRangeSqr = light.range * light.range;
					float fadeStartDistanceSqr = 0.8f * 0.8f * lightRangeSqr;
					float fadeRangeSqr = (fadeStartDistanceSqr - lightRangeSqr);
					float oneOverFadeRangeSqr = 1.0f / fadeRangeSqr;
					float lightRangeSqrOverFadeRangeSqr = -lightRangeSqr / fadeRangeSqr;
					float quadAtten = 25.0f / lightRangeSqr;
					lightAttn[0] = new Vector4(quadAtten, oneOverFadeRangeSqr, lightRangeSqrOverFadeRangeSqr, 1.0f);

					cmdLighting.SetGlobalVector("_LightColor0", lightColors[0]);
					cmdLighting.SetGlobalVector("_WorldSpaceLightPos0", lightPositions[0]);

					mainLight = light.light;
					mainLightIndex = i;
				}
			}
			else
			{
				continue;//so far just do only 1 directional light
			}
		}

		cmdLighting.SetGlobalVectorArray("unity_LightPosition", lightPositions);
		cmdLighting.SetGlobalVectorArray("unity_LightColor", lightColors);
		cmdLighting.SetGlobalVectorArray("unity_LightAtten", lightAttn);
		cmdLighting.SetGlobalVectorArray("unity_SpotDirection", lightSpotDir);

		context.ExecuteCommandBuffer(cmdLighting);
		cmdLighting.Release();
	}
}
