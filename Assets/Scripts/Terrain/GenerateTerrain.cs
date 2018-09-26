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
        mesh.uv4 = GenerateVertexNeighbours(mesh);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private Vector2[] GenerateVertexNeighbours(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] neighbours = new Vector2[vertices.Length];
        for (int y = 0; y < numVerticesZ; y++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                Vector3 neighbour1, neighbour2;

                if (y == numVerticesZ - 1 && x == numVerticesX - 1)
                {
                    neighbour1 = vertices[y * numVerticesX + x - numVerticesX - 1];
                    neighbour2 = vertices[y * numVerticesX + x - 1];
                }
                else if (y == numVerticesZ - 1)
                {
                    neighbour1 = vertices[y * numVerticesX + x + 1];
                    neighbour2 = vertices[y * numVerticesX + x - numVerticesX];
                }
                else if (x == numVerticesX - 1)
                {
                    neighbour1 = vertices[y * numVerticesX + x - 1];
                    neighbour2 = vertices[y * numVerticesX + x + numVerticesX];
                }
                else if (x == 0 && y > 0)
                {
                    neighbour1 = vertices[y * numVerticesX + x + 1];
                    neighbour2 = vertices[y * numVerticesX + x - numVerticesX];
                }
                else if (x == numVerticesX - 1 - 1 && y == 0)
                {
                    neighbour1 = vertices[y * numVerticesX + x + numVerticesX];
                    neighbour2 = vertices[y * numVerticesX + x + 1 + numVerticesX];
                }
                else if (x == 1 && y == 1)
                {
                    neighbour1 = vertices[y * numVerticesX + x - 1];
                    neighbour2 = vertices[y * numVerticesX + x + numVerticesX];
                }
                else
                {
                    neighbour1 = vertices[y * numVerticesX + x + 1 + numVerticesX];
                    neighbour2 = vertices[y * numVerticesX + x + 1];
                }

                neighbours[y * numVerticesX + x] = new Vector2(neighbour1.y, neighbour2.y);
            }
        }
        Debug.Log(neighbours[0]);
        return neighbours;
    }
}
