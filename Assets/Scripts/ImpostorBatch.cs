﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Contains and renders a collection of impostor planes with varying UVs and a shared texture*/
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class ImpostorBatch : MonoBehaviour
{
    /// <summary>
    /// Mesh containing the impostor plane(s)
    /// </summary>
    private Mesh myMesh;

    /// <summary>
    /// Material being used by the impostor plane(s)
    /// </summary>
    private Material myMaterial;

    [Tooltip("Maximum number of impostor planes stored in this batch")]
    public int maxNumImpostors = 32;

    [Tooltip("The texture to be used when rendering this impostor batch")]
    public Texture texture = null;

    /// <summary>
    /// Number of impostors currently reserved for use in this batch
    /// </summary>
    private int numReservedImpostors = 0;

    // Mesh element cache (as accessing mesh.vertices, etc takes time)
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] indexes;

    /// <summary>
    /// Whether to refresh 
    /// </summary>
    private bool isMeshInvalidated = false;

    private void Awake()
    {
        // Setup the impostor mesh caches
        vertices = new Vector3[maxNumImpostors * 4];
        uvs = new Vector2[maxNumImpostors * 4];
        indexes = new int[maxNumImpostors * 6];

        // Pre-calculate the index buffer (this never needs to change)
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

        // Flush changes to the mesh
        if (isMeshInvalidated)
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
        
        // Copy the cached mesh elements
        myMesh.vertices = vertices;
        myMesh.SetIndices(indexes, MeshTopology.Triangles, 0);
        myMesh.uv = uvs;
        myMesh.RecalculateBounds();

        // Refresh the material
        myMaterial.SetFloat("_Cutoff", 0.99f);
        myMaterial.mainTexture = texture;

        isMeshInvalidated = false;
    }

    /// <summary>
    /// Reserves an impostor plane
    /// </summary>
    /// <returns>The index of the reserved plane</returns>
    public int ReservePlane()
    {
        if (numReservedImpostors + 1 >= maxNumImpostors)
        {
            Debug.LogWarning("Warning: Impostor batch has run out of space!");
            return 0;
        }

        return numReservedImpostors++;
    }

    /// <summary>
    /// Positions the plane with the given index and specified dimensions
    /// </summary>
    /// <param name="planeIndex">The index of the plane to place</param>
    /// <param name="center">The centre of the plane</param>
    /// <param name="up">The plane's up vector. This should be half the total height of the impostor</param>
    /// <param name="right">The plane's right vector. This should be half the total width of the impostor</param>
    /// <param name="planeUvs">The UVs covered by the impostor plane</param>
    public void SetPlane(int planeIndex, Vector3 center, Vector3 up, Vector3 right, Rect planeUvs)
    {
        // Update the vertex and UVs buffer at this plane index
        int planeRoot = planeIndex * 4;

        vertices[planeRoot]     = center + up + right;
        vertices[planeRoot + 1] = center - up + right;
        vertices[planeRoot + 2] = center - up - right;
        vertices[planeRoot + 3] = center + up - right;

        uvs[planeRoot + 0] = planeUvs.max;
        uvs[planeRoot + 1] = new Vector2(planeUvs.xMax, planeUvs.yMin);
        uvs[planeRoot + 2] = planeUvs.min;
        uvs[planeRoot + 3] = new Vector2(planeUvs.xMin, planeUvs.yMax);

        // Refresh the mesh later
        isMeshInvalidated = true;
    }
}