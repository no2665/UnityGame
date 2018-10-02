using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CreateTerrainMesh : MonoBehaviour {

    private const int numVerticesX = TerrainHelper.numVerticesX;
    private const int numVerticesZ = TerrainHelper.numVerticesZ;
    private const int edgeLength = TerrainHelper.edgeLength;

    // Generate the mesh with a well-known vertex order
    void Start()
    {
        Mesh mesh = new Mesh
        {
            vertices = GenerateVertices(),
            triangles = GenerateTriangles(),
            uv = GenerateUV()
        };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    /*
     * Generate face triangles
     */
    private int[] GenerateTriangles()
    {
        //generate two triangles per vertex except the last column and last row
        int[] triangles = new int[(numVerticesX - 1) * (numVerticesZ - 1) * 6];
        for (int y = 0; y < numVerticesZ - 1; y++)
        {
            for (int x = 0; x < numVerticesX - 1; x++)
            {
                triangles[(y * (numVerticesX - 1) + x) * 6 + 5] = y * numVerticesX + x;
                triangles[(y * (numVerticesX - 1) + x) * 6 + 4] = y * numVerticesX + x + 1;
                triangles[(y * (numVerticesX - 1) + x) * 6 + 3] = y * numVerticesX + x + 1 + numVerticesX;
                triangles[(y * (numVerticesX - 1) + x) * 6 + 2] = y * numVerticesX + x;
                triangles[(y * (numVerticesX - 1) + x) * 6 + 1] = y * numVerticesX + x + 1 + numVerticesX;
                triangles[(y * (numVerticesX - 1) + x) * 6 + 0] = y * numVerticesX + x + numVerticesX;
            }
        }
        return triangles;
    }

    /*
     * Generate vertex coordinates
     */
    Vector3[] GenerateVertices()
    {
        Vector3[] vertices = new Vector3[numVerticesX * numVerticesZ];
        for (int y = 0; y < numVerticesZ; y++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                // Set y = 0, this is be changed later
                vertices[y * numVerticesX + x] = new Vector3(x * edgeLength, 0, y * edgeLength);
            }
        }
        return vertices;
    }

    /*
     * Generate texture coordinates
     */
    private Vector2[] GenerateUV()
    {
        Vector2[] uvs = new Vector2[numVerticesX * numVerticesZ];
        float widthF = (float)numVerticesX - 1;
        float heightF = (float)numVerticesZ - 1;
        for (int y = 0; y < numVerticesZ; y++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                // x, y have to be between 0 and 1
                uvs[y * numVerticesX + x] = new Vector2((float)(x) / widthF, (float)(y) / heightF);
            }
        }
        return uvs;
    }
}
