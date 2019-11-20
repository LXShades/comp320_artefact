using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Impostor : MonoBehaviour
{
    Mesh myMesh;

    /** The impostor batch associated with this impostor, if applicable*/
    public ImpostorBatch parentBatch = null;
    private int batchIndex = -1;

    public Rect uvs = new Rect(0, 0, 1, 1);

    static Vector3[] planeVertices = new Vector3[4] { new Vector3(5f, 0f, 5f), new Vector3(-5f, 0f, 5f), new Vector3(-5f, 0f, -5f), new Vector3(5f, 0f, -5f)};
    static Vector3[] planeNormals = new Vector3[4] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
    static int[] planeIndexes = new int[6] { 0, 2, 1, 0, 3, 2 };

    private void Start()
    {
        // Generate the mesh upon creation
        RegenerateMesh();
    }

    private void RegenerateMesh()
    {
        if (myMesh)
        {
            Destroy(myMesh);
            myMesh = null;
        }

        // Create the empty mesh
        myMesh = new Mesh();

        // Supply the current UVs
        Vector2[] cornerUvs = new Vector2[4] { uvs.min, new Vector2(uvs.xMax, uvs.yMin), uvs.max, new Vector2(uvs.xMin, uvs.yMax) };

        // Assign the mesh elements
        myMesh.vertices = planeVertices;
        myMesh.SetIndices(planeIndexes, MeshTopology.Triangles, 0);
        myMesh.normals = planeNormals;
        myMesh.uv = new Vector2[4] { cornerUvs[0], cornerUvs[1], cornerUvs[2], cornerUvs[3] };
        myMesh.RecalculateBounds();

        // done!
        GetComponent<MeshFilter>().sharedMesh = myMesh;
    }
}
