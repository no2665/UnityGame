using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Updates terrain mesh with the perlin noise values, to achieve an interesting (not flat) terrain
 */
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

    /*
     * To be used when moving a terrain tile. It calls an internal regenerate function which updates the 
     * mesh with new height values
     */
    public void RegenerateMesh()
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        RegenerateMesh(mesh);
        if (gameObject.GetComponent<MeshCollider>() != null) {
            (gameObject.GetComponent<MeshCollider>()).sharedMesh = mesh;
        }
    }

    /*
     * Move each vertex to the height from the perlin noise generator.
     * Move each vertex along the x and z axis by some small amount to make the mesh 
     * non-square
     */
    private void RegenerateMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        // Mesh isn't valid and/or ready yet
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

    /*
     * Pack some extra data into the uv4 matrix for the shader to use
     * Here we look at each vertex's neighbours to see the vertex is part of a slope,
     * so the shader can then use a different colour
     */
    private Vector2[] GenerateVertexNeighbourData(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] neighbours = new Vector2[vertices.Length];

        /*
         * x-x-x
         * |/|/|
         * x-x-x
         * |/|/|
         * x-x-x
         * Have to look at each vertex (x) and determine which vertices it shares any triangle with.
         * Below (a) will be the current vertex, and (b) will be the potential neighbour.
         */

        for (int z = 0; z < numVerticesZ; z++)
        {
            for (int x = 0; x < numVerticesX; x++)
            {
                List<float> heights = new List<float>();
                int p = z * numVerticesX + x;
                Vector3 v = vertices[p];
                float y = v.y;
                /*
                 * x-x-x
                 * |/|/|
                 * x-a-b
                 * |/|/|
                 * x-x-x
                 */
                int nextZ = (z + 1) * numVerticesX;
                if ( p + 1 < nextZ )
                {
                    heights.Add(vertices[p + 1].y);
                }
                /*
                 * x-x-x
                 * |/|/|
                 * b-a-x
                 * |/|/|
                 * x-x-x
                 */
                int thisZ = z * numVerticesX;
                if ( p - 1 >= thisZ)
                {
                    heights.Add(vertices[p - 1].y);
                }
                /*
                 * x-b-x
                 * |/|/|
                 * x-a-x
                 * |/|/|
                 * x-x-x
                 */
                if ( p + numVerticesX < numVerticesX * numVerticesZ )
                {
                    heights.Add(vertices[p + numVerticesX].y);
                }
                /*
                 * x-x-x
                 * |/|/|
                 * x-a-x
                 * |/|/|
                 * x-b-x
                 */
                if ( p - numVerticesX >= 0 )
                {
                    heights.Add(vertices[p - numVerticesX].y);
                }
                /*
                 * x-x-b
                 * |/|/|
                 * x-a-x
                 * |/|/|
                 * x-x-x
                 */
                int nextZLastX = ((z + 1) * numVerticesX) + numVerticesX - 1;
                if ( p + 1 + numVerticesX <= nextZLastX && nextZLastX < numVerticesX * numVerticesZ )
                {
                    heights.Add(vertices[p + 1 + numVerticesX].y);
                }
                /*
                 * x-x-x
                 * |/|/|
                 * x-a-x
                 * |/|/|
                 * b-x-x
                 */
                int prevZ = (z - 1) * numVerticesX;
                if ( p - 1 - numVerticesX >= prevZ && prevZ >= 0 )
                {
                    heights.Add(vertices[p - 1 - numVerticesX].y);
                }

                // Now we know the neighbours heights, check it against ours
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
