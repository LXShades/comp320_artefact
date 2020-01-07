using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class Impostify : MonoBehaviour
{
    [Header("Impostor settings")]
    [Tooltip("Whether to include child meshes")]
    public bool includeChildMeshes = true;

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
            if (value != _isImpostorVisible)
            {
                foreach (Renderer renderer in myRenderers)
                {
                    renderer.enabled = !value;
                }
                _isImpostorVisible = value;
            }
        }
        get
        {
            return _isImpostorVisible;
        }
    }
    private bool _isImpostorVisible = false;

    /// <summary>
    /// Current surface being used for the impostor
    ///</summary>
    ImpostorSurface impostorSurface;
    
    /** Expected position of the impostor */
    Vector3 impostorPosition;

    /** Physical radius of the object. This translates to the height of the impostor (rename?) */
    float impostorWidth;
    float impostorHeight;

    /** Corners of the bounding box surrounding the mesh, scaled */
    Vector3 maxBounds;
    Vector3 minBounds;

    /** The current projection matrix used by the impostor camera. Only applies when useBestFit is true. */
    Matrix4x4 impostorProjectionMatrix;

    /** The renderer associated with this object */
    Renderer[] myRenderers = new Renderer[0];
    Mesh[] myMeshes = new Mesh[0];

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
        impSurface.batch.SetPlane(impSurface.batchPlaneIndex, impostorPosition, mainCamera.transform.up * impostorHeight, mainCamera.transform.right * impostorWidth, impSurface.uvDimensions);
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

        float boxX = (minBounds.x - maxBounds.x)*0.5f, boxY = (minBounds.y - maxBounds.y)*0.5f, boxZ = (minBounds.z - maxBounds.z)*0.5f;
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
        impostorPosition = boundsCentre;
        impostorWidth = screenSpaceWidth * frustumWidthAtImpostorDepth;
        impostorHeight = screenSpaceHeight * frustumHeightAtImpostorDepth;

        Vector2 screenSpacePosition = new Vector2(
            Vector3.Dot(camera.transform.right, impostorPosition - camera.transform.position) / frustumWidthAtImpostorDepth * 2,
            Vector3.Dot(camera.transform.up, impostorPosition - camera.transform.position) / frustumHeightAtImpostorDepth * 2);

        impostorProjectionMatrix = Matrix4x4.Scale(new Vector3(0.5f / screenSpaceWidth, 0.5f / screenSpaceHeight, 1))
                                    * Matrix4x4.Translate(new Vector3(-screenSpacePosition.x, -screenSpacePosition.y, 0))
                                    * camera.projectionMatrix;

        camera.projectionMatrix = impostorProjectionMatrix;
    }

    void RenderImpostor()
    {
        Camera camera = ImpMan.singleton.impostorCamera;
        int[] oldLayers = new int[myMeshes.Length];
        bool[] oldVisibility = new bool[myMeshes.Length];
        const int impostorLayer = 30;

        // Clear the background pixels
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = fillImpostorBackground ? new Color(0, 0, 1) : new Color(0, 0, 0, 0);

        // Setup to render only this object
        for (int i = 0; i < myMeshes.Length; i++)
        {
            oldLayers[i] = myRenderers[i].gameObject.layer;
            oldVisibility[i] = myRenderers[i].enabled;

            myRenderers[i].gameObject.layer = impostorLayer;
            myRenderers[i].enabled = true;
        }
        
        // Render to the impostor
        camera.cullingMask = 1 << impostorLayer;
        camera.Render();

        for (int i = 0; i < myMeshes.Length; i++)
        {
            myRenderers[i].gameObject.layer = oldLayers[i];
            myRenderers[i].enabled = oldVisibility[i];
        }
    }

    void RefreshRendererInfo()
    {
        if (myRenderers.Length == 0)
        {
            // Collect meshes to render
            List<Mesh> meshList = new List<Mesh>();
            List<Renderer> rendererList = new List<Renderer>();
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child.GetComponentInParent<LODGroup>() != null)
                {
                    // don't add children of LOD groups, as they probably comprise multiple LOD copies
                    continue;
                }

                Mesh childMesh = child.GetComponent<MeshFilter>()?.sharedMesh;
                Renderer childRenderer = child.GetComponent<MeshRenderer>();

                // try to get the highest LOD if myRenderer fails
                if (childMesh == null)
                {
                    LODGroup lods = child.GetComponent<LODGroup>();

                    if (lods.lodCount > 0)
                    {
                        childMesh = lods.GetLODs()[0].renderers[0].GetComponent<MeshFilter>()?.sharedMesh;
                        childRenderer = lods.GetLODs()[0].renderers[0];
                    }
                }
                
                if (childMesh && childRenderer)
                {
                    meshList.Add(childMesh);
                    rendererList.Add(childRenderer);
                }
            }

            myMeshes = meshList.ToArray();
            myRenderers = rendererList.ToArray();
        }

        boundsCentre = transform.position;
        boundsRadius = 0;
        
        if (myMeshes.Length > 0)
        {
            float totalMaxDistance = 0;
            // Take the centre of the bounding box for all meshes combined
            maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (Renderer renderer in myRenderers)
            {
                minBounds = Vector3.Min(minBounds, renderer.bounds.min);
                maxBounds = Vector3.Max(maxBounds, renderer.bounds.max);
            }

            // Extend the size of the radius based on the distance of the vertices from the center of each model
            Vector3 centre = (minBounds + maxBounds) / 2;

            for (int i = 0; i < myMeshes.Length; i++)
            {
                Mesh mesh = myMeshes[i];
                Vector3[] vertices = mesh.vertices;
                float totalScale = 1;
                Transform currentTransform = myRenderers[i].transform;
                float maxDistance = 0;

                while (currentTransform)
                {
                    totalScale *= Mathf.Max(currentTransform.localScale.x, currentTransform.localScale.x, currentTransform.localScale.z);
                    currentTransform = currentTransform.parent;
                }

                for (int v = 0; v < vertices.Length; v++)
                {
                    maxDistance = Mathf.Max(maxDistance, Vector3.Distance(vertices[v], centre));
                }

                totalMaxDistance = Mathf.Max(totalMaxDistance, maxDistance * totalScale);
            }

            boundsCentre = centre;// transform.TransformPoint(centre);
            boundsRadius = totalMaxDistance * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // show renderer boundaries in the editor
        if (myRenderers.Length == 0)
        {
            RefreshRendererInfo();

            if (myRenderers.Length == 0)
            {
                return;
            }
        }

        Gizmos.color = new Color(0, 1, 0, 0.25f);
        Gizmos.DrawSphere(boundsCentre, boundsRadius);
        Gizmos.color = new Color(1, 1, 0, 1);
        Gizmos.DrawWireSphere(boundsCentre, boundsRadius);

        Gizmos.color = new Color(1, 1, 0, 1);
        Gizmos.DrawWireCube(boundsCentre, maxBounds - minBounds);
    }
}
