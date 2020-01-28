using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ImpostorCamera : MonoBehaviour
{
    public new Camera camera;

    private void Awake()
    {
        camera = GetComponent<Camera>();

        // don't use the camera as an output camera
        camera.enabled = false;
    }

    /// <summary>
    /// Positions and frames the camera to capture an impostor covering the area between minBounds and maxBounds.
    /// </summary>
    /// <param name="minBounds"></param>
    /// <param name="maxBounds"></param>
    /// <param name="mainCamera"></param>
    /// <param name="impostorWidth"></param>
    /// <param name="impostorHeight"></param>
    public void FrameArea(Vector3 minBounds, Vector3 maxBounds, Camera mainCamera, out float impostorWidth, out float impostorHeight)
    {
        // Copy and setup camera projection settings
        camera.aspect = mainCamera.aspect;
        camera.fieldOfView = mainCamera.fieldOfView;
        camera.transform.position = mainCamera.transform.position;
        camera.transform.rotation = mainCamera.transform.rotation;

        Vector3 impostorPosition = (minBounds + maxBounds) * 0.5f;
        float boxX = (minBounds.x - maxBounds.x) * 0.5f, boxY = (minBounds.y - maxBounds.y) * 0.5f, boxZ = (minBounds.z - maxBounds.z) * 0.5f;
        float impostorDepth = Vector3.Dot(impostorPosition - camera.transform.position, camera.transform.forward);
        float frustumWidthAtImpostorDepth = (impostorDepth * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f)) * 2f * camera.aspect;
        float frustumHeightAtImpostorDepth = (impostorDepth * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f)) * 2f;
        float screenSpaceWidth = (Mathf.Abs(camera.transform.right.x * boxX) +
                                 Mathf.Abs(camera.transform.right.y * boxY) +
                                 Mathf.Abs(camera.transform.right.z * boxZ)) / frustumWidthAtImpostorDepth;
        float screenSpaceHeight = (Mathf.Abs(camera.transform.up.x * boxX) +
                                 Mathf.Abs(camera.transform.up.y * boxY) +
                                 Mathf.Abs(camera.transform.up.z * boxZ)) / frustumHeightAtImpostorDepth;

        // Figure out how big the impostor will be and its plane scale, etc, and render to match that area of the screen
        impostorWidth = screenSpaceWidth * frustumWidthAtImpostorDepth;
        impostorHeight = screenSpaceHeight * frustumHeightAtImpostorDepth;

        Vector2 screenSpacePosition = new Vector2(
            Vector3.Dot(camera.transform.right, impostorPosition - camera.transform.position) / frustumWidthAtImpostorDepth * 2,
            Vector3.Dot(camera.transform.up, impostorPosition - camera.transform.position) / frustumHeightAtImpostorDepth * 2);

        camera.ResetProjectionMatrix();
        camera.projectionMatrix = Matrix4x4.Scale(new Vector3(0.5f / screenSpaceWidth, 0.5f / screenSpaceHeight, 1))
                                    * Matrix4x4.Translate(new Vector3(-screenSpacePosition.x, -screenSpacePosition.y, 0))
                                    * camera.projectionMatrix;
    }

    /// <summary>
    /// Sets the target surface to render to
    /// </summary>
    /// <param name="surface">The target surface</param>
    /// <param name="clearColour">The background colour to clear to</param>
    public void SetTargetSurface(ImpostorSurface surface, Color clearColour)
    {
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = clearColour;
        camera.targetTexture = surface.texture;
        camera.pixelRect = new Rect(surface.pixelDimensions.x, surface.pixelDimensions.y,
                                    surface.pixelDimensions.width, surface.pixelDimensions.height);

        camera.cullingMask = 1 << ImpMan.singleton.impostorRenderLayer;
    }

    public void RenderToSurface(ImpostorSurface surface, Color clearColour)
    {
        SetTargetSurface(surface, clearColour);

        camera.Render();
    }
}
