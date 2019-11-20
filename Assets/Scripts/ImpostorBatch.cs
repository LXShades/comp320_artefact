using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Contains and renders a collection of impostor planes with varying UVs and a shared texture*/
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ImpostorBatch : MonoBehaviour
{
    private Mesh myMesh;
    private Material myMaterial;

    /** Maximum number of impostor planes stored in this batch */
    public int maxNumImpostors = 300;

    /** The texture to be used when rendering this impostor batch */
    public Texture texture = null;

    /** Number of impostors currently reserved for use */
    private int numReservedImpostors = 0;

    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] indexes;

    private bool doFlushMeshChanges = false;

    private void Awake()
    {
        vertices = new Vector3[maxNumImpostors * 4];
        uvs = new Vector2[maxNumImpostors * 4];
        indexes = new int[maxNumImpostors * 6];

        for (int i = 0; i < maxNumImpostors; i++)
        {
            int indexRoot = i * 6;
            int vertexRoot = i * 4;

            indexes[indexRoot] = vertexRoot;
            indexes[indexRoot + 1] = vertexRoot + 1;
            indexes[indexRoot + 2] = vertexRoot + 2;
            indexes[indexRoot + 3] = vertexRoot;
            indexes[indexRoot + 4] = vertexRoot + 2;
            indexes[indexRoot + 5] = vertexRoot + 3;
        }
    }

    private void Start()
    {
        // Generate the mesh upon creation
        RefreshMesh();
    }

    private void LateUpdate()
    {
        // Reset position (we're batched, so no movement plz)
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        // Flush changes to the mesh?
        if (doFlushMeshChanges)
        {
            RefreshMesh();
        }
    }

    private void RefreshMesh()
    {
        if (myMesh == null)
        {
            // Create the empty mesh and material
            myMesh = new Mesh();
            myMaterial = new Material(ImpMan.singleton.impostorShader);

            GetComponent<MeshFilter>().sharedMesh = myMesh;
            GetComponent<MeshRenderer>().material = myMaterial;
        }
        else
        {
            myMesh.Clear();
        }
        
        // Assign the mesh elements
        myMesh.vertices = vertices;
        myMesh.SetIndices(indexes, MeshTopology.Triangles, 0);
        myMesh.uv = uvs;
        myMesh.RecalculateBounds();

        // Refresh the material
        myMaterial.SetFloat("_Cutoff", 0.99f);
        myMaterial.mainTexture = texture;
    }

    /** Reserves an impostor plane */
    public int ReservePlane()
    {
        if (numReservedImpostors + 1 >= maxNumImpostors)
        {
            Debug.LogWarning("Warning: Impostor batch has run out of space!");
            return 0;
        }

        return numReservedImpostors++;
    }

    /** Plants a plane with the specified dimensions */
    public void SetPlane(int planeIndex, Vector3 center, Vector3 up, Vector3 right, Rect planeUvs)
    {
        // Update the vertex and UVs buffer at this plane index
        int planeRoot = planeIndex * 4;

        vertices[planeRoot]     = center + up + right;
        vertices[planeRoot + 1] = center - up + right;
        vertices[planeRoot + 2] = center - up - right;
        vertices[planeRoot + 3] = center + up - right;

        uvs[planeRoot + 2] = planeUvs.min;
        uvs[planeRoot + 1] = new Vector2(planeUvs.xMax, planeUvs.yMin);
        uvs[planeRoot + 0] = planeUvs.max;
        uvs[planeRoot + 3] = new Vector2(planeUvs.xMin, planeUvs.yMax);

        doFlushMeshChanges = true;
    }
}
