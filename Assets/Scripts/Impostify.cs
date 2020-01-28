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
                foreach (Renderer renderer in renderers)
                {
                    renderer.gameObject.layer = 0;
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
    /// </summary>
    public ImpostorSurface impostorSurface;
    
    /** Expected position of the impostor */
    Vector3 impostorPosition;

    /** Physical dimensions of the impostor object on its local axes */
    float impostorWidth;
    float impostorHeight;

    /** Corners of the bounding box surrounding the mesh, scaled */
    Vector3 maxBounds;
    Vector3 minBounds;

    /** The current projection matrix used by the impostor camera. Only applies when useBestFit is true. */
    Matrix4x4 impostorProjectionMatrix;

    /** The renderer associated with this object */
    [System.NonSerialized] public Renderer[] renderers = new Renderer[0];
    [System.NonSerialized] public Mesh[] meshes = new Mesh[0];

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

    /// <summary>
    /// Renders this impostor onto the current assigned impostorSurface
    /// </summary>
    void RenderImpostor()
    {
        ImpostorCamera impCam = ImpMan.singleton.impostorCamera;
        int[] oldLayers = new int[meshes.Length];
        bool[] oldVisibility = new bool[meshes.Length];
        int impostorLayer = ImpMan.singleton.impostorRenderLayer;

        // Clear the background pixels
        impCam.FrameArea(minBounds, maxBounds, Camera.main, out impostorWidth, out impostorHeight);

        // Setup to render only this object
        for (int i = 0; i < meshes.Length; i++)
        {
            oldLayers[i] = renderers[i].gameObject.layer;
            oldVisibility[i] = renderers[i].enabled;

            renderers[i].gameObject.layer = impostorLayer;
            renderers[i].enabled = true;
        }

        // Render to the impostor
        impCam.RenderToSurface(impostorSurface, fillImpostorBackground ? new Color(0, 0, 1) : new Color(0, 0, 0, 0));

        for (int i = 0; i < meshes.Length; i++)
        {
            renderers[i].gameObject.layer = oldLayers[i];
            renderers[i].enabled = oldVisibility[i];
        }

        // Update the impostor plane
        if (impostorSurface != null)
        {
            impostorSurface.batch.SetPlane(impostorSurface.batchPlaneIndex, impostorPosition, mainCamera.transform.up * impostorHeight, mainCamera.transform.right * impostorWidth, impostorSurface.uvDimensions);
        }
    }

    /// <summary>
    /// Regenerates the mesh list and boundaries for this object
    /// Note that this function may be called in-editor
    /// </summary>
    void RefreshRendererInfo()
    {
        if (renderers.Length == 0)
        {
            // Collect meshes to render
            List<Mesh> meshList = new List<Mesh>();
            List<Renderer> rendererList = new List<Renderer>();
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                // don't add children of LOD groups, as they probably comprise multiple LOD copies
                bool isInLodGroup = false;
                for (Transform parent = child.parent; parent != null; parent = parent.parent)
                {
                    if (parent.GetComponent<LODGroup>())
                    {
                        isInLodGroup = true;
                        break;
                    }
                }

                if (isInLodGroup)
                {
                    continue;
                }

                MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                Mesh childMesh = null;
                Renderer childRenderer = null;

                // try and get the mesh filter
                if (meshFilter)
                {
                    childMesh = meshFilter.sharedMesh;
                    childRenderer = child.GetComponent<MeshRenderer>();
                }
                
                // Add to the mesh/renderer list
                if (childMesh && childRenderer)
                {
                    meshList.Add(childMesh);
                    rendererList.Add(childRenderer);
                }

                // Add LOD groups if there are any
                LODGroup lodComponent = child.GetComponent<LODGroup>();

                if (lodComponent && lodComponent.lodCount > 0)
                {
                    Debug.Log($"Found a LOD. Diable? {Application.isPlaying}");
                    LOD[] lods = lodComponent.GetLODs();
                    
                    foreach (MeshRenderer renderer in lods[0].renderers)
                    {
                        meshFilter = renderer.GetComponent<MeshFilter>();

                        if (meshFilter)
                        {
                            meshList.Add(meshFilter.sharedMesh);
                            rendererList.Add(renderer);
                        }
                    }

                    // If in-game, disable other LODs in the group
#if UNITY_EDITOR
                    if (Application.isPlaying)
                    {
#endif
                        for (int i = 1; i < lods.Length; i++)
                        {
                            foreach (Renderer renderer in lods[i].renderers)
                            {
                                renderer.enabled = false;
                            }
                        }

                        lods[0].fadeTransitionWidth = 1;
                        lods[0].screenRelativeTransitionHeight = 0;
                        lodComponent.SetLODs(new LOD[1] { lods[0] });
#if UNITY_EDITOR
                    }
#endif
                }
            }

            meshes = meshList.ToArray();
            renderers = rendererList.ToArray();
        }

        boundsCentre = transform.position;
        boundsRadius = 0;
        
        if (meshes.Length > 0)
        {
            float totalMaxDistance = 0;
            // Take the centre of the bounding box for all meshes combined
            maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (Renderer renderer in renderers)
            {
                minBounds = Vector3.Min(minBounds, renderer.bounds.min);
                maxBounds = Vector3.Max(maxBounds, renderer.bounds.max);
            }

            // Extend the size of the radius based on the distance of the vertices from the center of each model
            Vector3 centre = (minBounds + maxBounds) / 2;

            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh mesh = meshes[i];
                Vector3[] vertices = mesh.vertices;
                float totalScale = 1;
                Transform currentTransform = renderers[i].transform;
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
        if (renderers.Length == 0)
        {
            RefreshRendererInfo();

            if (renderers.Length == 0)
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
