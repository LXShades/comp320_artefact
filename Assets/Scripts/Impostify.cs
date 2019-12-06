using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class Impostify : MonoBehaviour
{
    [Header("Testing")]
    [Tooltip("Whether to preview the impostor projection matrix on the camera")]
    public bool previewProjectionMatrix = false;
    [Tooltip("Whether to fill the background of the impostor with a solid colour (to confirm that it's actually there!)")]
    public bool fillImpostorBackground = false;
    [Tooltip("If enabled, holding space will display the impostor")]
    public bool holdSpaceForImpostor = false;
    
    /// <summary>
    /// Whether the impostor is currently visible instead of the true object
    /// </summary>
    [HideInInspector] public bool isImpostorVisible
    {
        set
        {
            if (myRenderer)
            {
                myRenderer.enabled = !value;
            }
        }
        get
        {
            if (myRenderer)
            {
                return !myRenderer.enabled;
            }
            return false;
        }
    }

    /// <summary>
    /// Current surface being used for the impostor
    ///</summary>
    ImpostorSurface impostorSurface;
    
    /** Expected position of the impostor */
    Vector3 impostorPosition;

    /** Physical radius of the object. This translates to the height of the impostor (rename?) */
    float impostorRadius;

    /** The current projection matrix used by the impostor camera. Only applies when useBestFit is true. */
    Matrix4x4 impostorProjectionMatrix;

    /** The renderer associated with this object */
    Renderer myRenderer;
    Mesh myMesh;

    Camera mainCamera;

    /** World-space bounds of this object */
    private Vector3 boundsCentre;
    private float boundsRadius;

    private void Awake()
    {
        RefreshRendererInfo();
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void RerenderImpostor(ImpostorSurface impSurface = null)
    {
        if (impSurface == null)
        {
            return; // we need a texture!
        }

        impostorSurface = impSurface;

        // Render the impostor
        PrepareImpostorCamera();
        RenderImpostor();

        // Update the impostor plane
        impSurface.batch.SetPlane(impSurface.batchPlaneIndex, impostorPosition, mainCamera.transform.up * impostorRadius, mainCamera.transform.right * (impostorRadius * mainCamera.aspect), impSurface.uvDimensions);
    }

    void PrepareImpostorCamera()
    {
        // Setup the impostor camera
        Camera camera = ImpMan.singleton.impostorCamera;

        camera.aspect = mainCamera.aspect;
        camera.fieldOfView = mainCamera.fieldOfView;
        camera.transform.position = mainCamera.transform.position;
        camera.transform.rotation = mainCamera.transform.rotation;
        camera.targetTexture = impostorSurface.texture;
        camera.pixelRect = new Rect(impostorSurface.pixelDimensions.x, impostorSurface.pixelDimensions.y,
                                    impostorSurface.pixelDimensions.width, impostorSurface.pixelDimensions.height);
        camera.ResetProjectionMatrix();

        // Figure out how big the impostor will be and its plane scale, etc, and render to match that area of the screen
        impostorPosition = boundsCentre;
        impostorRadius = boundsRadius;

        float impostorDepth = Vector3.Dot(impostorPosition - camera.transform.position, camera.transform.forward);
        float frustumWidthAtImpostorDepth = (impostorDepth * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f)) * 2f * camera.aspect;
        float frustumHeightAtImpostorDepth = (impostorDepth * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f)) * 2f;
        float impostorScreenSpaceRadius = impostorRadius / frustumHeightAtImpostorDepth;
        Vector2 screenSpacePosition = new Vector2();

        screenSpacePosition = new Vector2(
            Vector3.Dot(camera.transform.right, impostorPosition - camera.transform.position) / frustumWidthAtImpostorDepth * 2,
            Vector3.Dot(camera.transform.up, impostorPosition - camera.transform.position) / frustumHeightAtImpostorDepth * 2);

        impostorProjectionMatrix = Matrix4x4.Scale(new Vector3(0.5f / (impostorRadius / frustumHeightAtImpostorDepth), 0.5f / (impostorRadius / frustumHeightAtImpostorDepth), 1))
                                    * Matrix4x4.Translate(new Vector3(-screenSpacePosition.x, -screenSpacePosition.y, 0))
                                    * camera.projectionMatrix;

        camera.projectionMatrix = impostorProjectionMatrix;
    }

    void RenderImpostor()
    {
        Camera camera = ImpMan.singleton.impostorCamera;
        int oldLayer = myRenderer.gameObject.layer;
        bool oldVisible = myRenderer.enabled;
        const int impostorLayer = 30;

        // Clear the background pixels
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = fillImpostorBackground ? new Color(0, 0, 1) : new Color(0, 0, 0, 0);

        // Setup to render only this object
        camera.cullingMask = 1<<impostorLayer;
        myRenderer.gameObject.layer = impostorLayer;
        myRenderer.enabled = true;

        // Render to the impostor
        camera.Render();

        camera.targetTexture = null;
        camera.enabled = false;

        // Done!
        myRenderer.gameObject.layer = oldLayer;
        myRenderer.enabled = oldVisible;
    }

    void RefreshRendererInfo()
    {
        if (myRenderer == null)
        {
            myRenderer = GetComponent<Renderer>();

            // try to get the highest LOD if myRenderer fails
            if (myRenderer == null)
            {
                LODGroup lods = GetComponent<LODGroup>();

                myRenderer = lods.GetLODs()[0].renderers[0];
            }

            myMesh = myRenderer.GetComponent<MeshFilter>()?.sharedMesh;
        }
        
        if (myRenderer)
        {
            Vector3[] vertices = myMesh.vertices;
            Vector3 centre = (myMesh.bounds.max + myMesh.bounds.min) / 2;
            float maxDistance = 0;

            for (int i = 0; i < vertices.Length; i++)
            {
                float distance = Vector3.Distance(vertices[i], centre);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }

            boundsCentre = transform.TransformPoint(centre);
            boundsRadius = maxDistance * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnDrawGizmos()
    {
        // show renderer boundaries in the editor
        if (myRenderer == null)
        {
            RefreshRendererInfo();

            if (myRenderer == null)
            {
                return;
            }
        }

        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawSphere(boundsCentre, boundsRadius);
        Gizmos.color = new Color(1, 1, 0, 1);
        Gizmos.DrawWireSphere(boundsCentre, boundsRadius);
    }
}
