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
    public void FrameArea(Vector3 minBounds, Vector3 maxBounds, Vector3 impostorCentre, Camera mainCamera, out float impostorWidth, out float impostorHeight, out Vector3 impostorCentreOut)
    {
        // Copy and setup camera projection settings
        camera.aspect = mainCamera.aspect;
        camera.fieldOfView = mainCamera.fieldOfView;
        camera.transform.position = mainCamera.transform.position;
        camera.transform.rotation = mainCamera.transform.rotation;

        // aight guess we do this the hard way
        Matrix4x4 worldToCamera = mainCamera.worldToCameraMatrix;
        Matrix4x4 cameraToWorld = mainCamera.cameraToWorldMatrix;

        Vector3 localSpaceMinCorner = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 localSpaceMaxCorner = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Vector3[] corners = new Vector3[8] {
            new Vector3(minBounds.x, minBounds.y, minBounds.z),
            new Vector3(minBounds.x, minBounds.y, maxBounds.z),
            new Vector3(minBounds.x, maxBounds.y, minBounds.z),
            new Vector3(minBounds.x, maxBounds.y, maxBounds.z),
            new Vector3(maxBounds.x, minBounds.y, minBounds.z),
            new Vector3(maxBounds.x, minBounds.y, maxBounds.z),
            new Vector3(maxBounds.x, maxBounds.y, minBounds.z),
            new Vector3(maxBounds.x, maxBounds.y, maxBounds.z)
        };

        foreach (Vector3 corner in corners)
        {
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(corner);

            localSpaceMinCorner = Vector3.Min(localSpaceMinCorner, screenPoint);
            localSpaceMaxCorner = Vector3.Max(localSpaceMaxCorner, screenPoint);
        }

        float desiredImpostorDepth = Vector3.Dot(impostorCentre - mainCamera.transform.position, mainCamera.transform.forward);
        float screenSpaceWidth = localSpaceMaxCorner.x - localSpaceMinCorner.x;
        float screenSpaceHeight = localSpaceMaxCorner.y - localSpaceMinCorner.y;
        Vector3 screenSpaceCentre = (localSpaceMaxCorner + localSpaceMinCorner) * 0.5f;
        Vector3 worldMinEdge = mainCamera.ScreenToWorldPoint(new Vector3(localSpaceMinCorner.x, localSpaceMinCorner.y, desiredImpostorDepth));
        Vector3 worldMaxEdge = mainCamera.ScreenToWorldPoint(new Vector3(localSpaceMaxCorner.x, localSpaceMaxCorner.y, desiredImpostorDepth));
        Vector3 worldCentre = (worldMinEdge + worldMaxEdge) * 0.5f;

        // Calculate the size of the impostor and its centre
        impostorWidth = Mathf.Abs(Vector3.Dot(worldMaxEdge - worldMinEdge, mainCamera.transform.right)) * 0.5f;
        impostorHeight = Mathf.Abs(Vector3.Dot(worldMaxEdge - worldMinEdge, mainCamera.transform.up)) * 0.5f;
        impostorCentreOut = worldCentre;

        screenSpaceWidth /= camera.pixelWidth / 2 * camera.aspect;
        screenSpaceHeight /= camera.pixelHeight / 2;
        screenSpaceCentre.x /= camera.pixelWidth;
        screenSpaceCentre.y /= camera.pixelHeight;

        // Scales up the portion of the screen (occupied by the impostor) to fill the whole render surface
        camera.ResetProjectionMatrix();
        camera.projectionMatrix = Matrix4x4.Scale(new Vector3(0.5f / screenSpaceWidth, 0.5f / screenSpaceHeight, 1))
                                    * Matrix4x4.Translate(new Vector3(-screenSpaceCentre.x, -screenSpaceCentre.y, 0))
                                    * camera.projectionMatrix;

//        mainCamera.projectionMatrix = camera.projectionMatrix;

        // Draws the min and max edges of the impostor
        DebugDraw.Point(worldMinEdge, Color.red);
        DebugDraw.Point(worldMaxEdge, Color.red);

        // draw bounding box
        DebugDraw.Box(minBounds, maxBounds, Color.cyan);

        // draw impostor position A
        DebugDraw.Square(worldCentre, mainCamera.transform.right * impostorWidth, mainCamera.transform.up * impostorHeight, Color.green);
        DebugDraw.Point(worldCentre, Color.cyan, 1);

        // draw user's intended impostor centre
        DebugDraw.Point(impostorCentre, Color.green, 2.0f);

        // draw camera frustum
        float d = 100.0f;
        float frustumWidthAtD = (d * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f)) * 2f * camera.aspect;
        float frustumHeightAtD = (d * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f)) * 2f;

        DebugDraw.Line(camera.transform.position, camera.transform.position + camera.transform.forward * d + camera.transform.right * (frustumWidthAtD / 2) + camera.transform.up * (frustumHeightAtD / 2), Color.red);
        DebugDraw.Line(camera.transform.position, camera.transform.position + camera.transform.forward * d + camera.transform.right * (frustumWidthAtD / 2) - camera.transform.up * (frustumHeightAtD / 2), Color.red);
        DebugDraw.Line(camera.transform.position, camera.transform.position + camera.transform.forward * d - camera.transform.right * (frustumWidthAtD / 2) + camera.transform.up * (frustumHeightAtD / 2), Color.red);
        DebugDraw.Line(camera.transform.position, camera.transform.position + camera.transform.forward * d - camera.transform.right * (frustumWidthAtD / 2) - camera.transform.up * (frustumHeightAtD / 2), Color.red);
    }

    /// <summary>
    /// Positions and frames the camera to capture an impostor covering the area between minBounds and maxBounds.
    /// </summary>
    /// <param name="minBounds"></param>
    /// <param name="maxBounds"></param>
    /// <param name="mainCamera"></param>
    /// <param name="impostorWidth"></param>
    /// <param name="impostorHeight"></param>
    public void FrameLayer(float windowDistance, Camera mainCamera, out float impostorWidth, out float impostorHeight, out Vector3 impostorCentre)
    {
        // Copy and setup camera projection settings
        camera.aspect = mainCamera.aspect;
        camera.fieldOfView = mainCamera.fieldOfView;
        camera.transform.position = mainCamera.transform.position;
        camera.transform.rotation = mainCamera.transform.rotation;

        impostorWidth = windowDistance * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f) * camera.aspect;
        impostorHeight = windowDistance * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
        impostorCentre = mainCamera.transform.position + mainCamera.transform.forward * windowDistance;

        camera.ResetProjectionMatrix();
        //camera.projectionMatrix = Matrix4x4.Scale(new Vector3(1/windowDistance, 1/windowDistance, 1)) * camera.projectionMatrix;

        // draw impostor position A
        DebugDraw.Square(impostorCentre, mainCamera.transform.right * impostorWidth, mainCamera.transform.up * impostorHeight, Color.green);
        DebugDraw.Point(impostorCentre, Color.cyan, 1);

        // draw user's intended impostor centre
        DebugDraw.Point(impostorCentre, Color.green, 2.0f);

        // draw camera frustum
        float d = 100.0f;
        float frustumWidthAtD = (d * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f)) * 2f * camera.aspect;
        float frustumHeightAtD = (d * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f)) * 2f;

        DebugDraw.Line(camera.transform.position, camera.transform.position + camera.transform.forward * d + camera.transform.right * (frustumWidthAtD / 2) + camera.transform.up * (frustumHeightAtD / 2), Color.red);
        DebugDraw.Line(camera.transform.position, camera.transform.position + camera.transform.forward * d + camera.transform.right * (frustumWidthAtD / 2) - camera.transform.up * (frustumHeightAtD / 2), Color.red);
        DebugDraw.Line(camera.transform.position, camera.transform.position + camera.transform.forward * d - camera.transform.right * (frustumWidthAtD / 2) + camera.transform.up * (frustumHeightAtD / 2), Color.red);
        DebugDraw.Line(camera.transform.position, camera.transform.position + camera.transform.forward * d - camera.transform.right * (frustumWidthAtD / 2) - camera.transform.up * (frustumHeightAtD / 2), Color.red);
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
