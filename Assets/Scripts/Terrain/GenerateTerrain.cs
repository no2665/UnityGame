using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateTerrain : MonoBehaviour {

    private const int numVerticesX = TerrainHelper.numVerticesX;
    private const int numVerticesZ = TerrainHelper.numVerticesZ;
    private const int edgeLength = TerrainHelper.edgeLength;

    // Use this for initialization
    void Start() {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        RegenerateMesh(mesh);
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void RegenerateMesh()
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        RegenerateMesh(mesh);
        if (gameObject.GetComponent<MeshCollider>() != null) {
            (gameObject.GetComponent<MeshCollider>()).sharedMesh = mesh;
        }
    }

    private void RegenerateMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        if (vertices.Length == 0) return;
        for (int y = 0; y < numVerticesZ; y++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                Vector3 vertex = vertices[y * numVerticesX + x];
                float offsetX = (x * edgeLength) + gameObject.transform.position.x;
                float offsetZ = (y * edgeLength) + gameObject.transform.position.z;

                vertex.y = TerrainHelper.Instance.GetYAt(offsetX, offsetZ);

                vertex.x = (x * edgeLength) + TerrainHelper.Instance.GetXFidgetAt(offsetX, offsetZ);
                vertex.z = (y * edgeLength) + TerrainHelper.Instance.GetZFidgetAt(offsetX, offsetZ);
                vertices[y * numVerticesX + x] = vertex;
            }
        }
        mesh.vertices = vertices;
        mesh.uv4 = GenerateVertexNeighbourData(mesh);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private Vector2[] GenerateVertexNeighbourData(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] neighbours = new Vector2[vertices.Length];

        for (int z = 0; z < numVerticesZ; z++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                List<float> heights = new List<float>();
                int p = z * numVerticesX + x;
                Vector3 v = vertices[p];
                float y = v.y;
                int nextZ = (z + 1) * numVerticesX;
                if ( p + 1 < nextZ )
                {
                    heights.Add(vertices[p + 1].y);
                }
                int thisZ = z * numVerticesX;
                if ( p - 1 >= thisZ)
                {
                    heights.Add(vertices[p - 1].y);
                }
                if ( p + numVerticesX < numVerticesX * numVerticesZ )
                {
                    heights.Add(vertices[p + numVerticesX].y);
                }
                if ( p - numVerticesX >= 0 )
                {
                    heights.Add(vertices[p - numVerticesX].y);
                }
                int nextZLastX = ((z + 1) * numVerticesX) + numVerticesX - 1;
                if ( p + 1 + numVerticesX <= nextZLastX && nextZLastX < numVerticesX * numVerticesZ )
                {
                    heights.Add(vertices[p + 1 + numVerticesX].y);
                }
                int prevZ = (z - 1) * numVerticesX;
                if ( p - 1 - numVerticesX >= prevZ && prevZ >= 0 )
                {
                    heights.Add(vertices[p - 1 - numVerticesX].y);
                }

                Vector2 slopeFound = Vector2.zero;
                foreach (float h in heights)
                {
                    if ( h != y )
                    {
                        slopeFound.x = 10;
                        break;
                    }
                }

                neighbours[z * numVerticesX + x] = slopeFound;
            }
        }

        return neighbours;
    }
}
