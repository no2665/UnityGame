using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RootMesh : MonoBehaviour {

    private const float height = 1;
    private const float halfWidth = 0.5f;
    private const float padding = 0.1f;
    private const float widthMinusPadding = halfWidth - padding;

    public void RegenerateMesh( Root[] neighbours )
    {
        Mesh mesh = new Mesh
        {
            vertices = GenerateVertices(neighbours),
            triangles = GenerateTriangles(neighbours),
        };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private Vector3[] GenerateVertices(Root[] neighbours)
    {
        Vector3[] vertices;
        if ( neighbours != null )
        {
            vertices = new Vector3[5 + (neighbours.Length * 3)];
        }
        else
        {
            vertices = new Vector3[5];
        }

        vertices[0] = new Vector3(0, height, 0);
        vertices[1] = new Vector3(0, 0, widthMinusPadding);
        vertices[2] = new Vector3(widthMinusPadding, 0, 0);
        vertices[3] = new Vector3(0, 0, -widthMinusPadding);
        vertices[4] = new Vector3(-widthMinusPadding, 0, 0);

        if (neighbours == null)
        {
            return vertices;
        }

        int x = Mathf.FloorToInt(transform.position.x);
        int z = Mathf.FloorToInt(transform.position.z);

        int index = 5;

        for ( int i = 0; i < neighbours.Length; i++ )
        {
            Root n = neighbours[i];
            int nx = n.GetX();
            int nz = n.GetZ();

            int signZ = (int)Mathf.Sign(nz - z);
            int signX = (int)Mathf.Sign(nx - x);

            if ( x == nx )
            {
                vertices[index] = new Vector3(0, height, signZ * halfWidth);
                vertices[index + 1] = new Vector3(signZ * widthMinusPadding, 0, signZ * halfWidth);
                vertices[index + 2] = new Vector3(-signZ * widthMinusPadding, 0, signZ * halfWidth);
            } else if ( z == nz )
            {
                vertices[index] = new Vector3(signX * halfWidth, height, 0);
                vertices[index + 1] = new Vector3(signX * halfWidth, 0, -signX * widthMinusPadding);
                vertices[index + 2] = new Vector3(signX * halfWidth, 0, signX * widthMinusPadding);
            } else
            {
                vertices[index] = new Vector3(signX * halfWidth, height, signZ * halfWidth);
                vertices[index + 1] = new Vector3((signX * halfWidth) + (signZ * (widthMinusPadding / 2)), 0, (signZ * halfWidth) - (signX * widthMinusPadding / 2));
                vertices[index + 2] = new Vector3((signX * halfWidth) - (signZ * (widthMinusPadding / 2)), 0, (signZ * halfWidth) + (signX * (widthMinusPadding / 2)));
            }

            index += 3;
        }

        return vertices;
    }

    private int[] GenerateTriangles( Root[] neighbours )
    {
        int[] triangles;
        if ( neighbours != null )
        {
            triangles = new int[(4 * 3) + (neighbours.Length * 4 * 3)];
        }
        else
        {
            triangles = new int[(4 * 3)];
        }

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        triangles[6] = 0;
        triangles[7] = 3;
        triangles[8] = 4;

        triangles[9] = 0;
        triangles[10] = 4;
        triangles[11] = 1;

        if (neighbours == null)
        {
            return triangles;
        }

        int x = Mathf.FloorToInt(transform.position.x);
        int z = Mathf.FloorToInt(transform.position.z);

        int index = 12;
        int vertIndex = 5;

        for (int i = 0; i < neighbours.Length; i++)
        {
            Root n = neighbours[i];
            int nx = n.GetX();
            int nz = n.GetZ();

            int signZ = (int)Mathf.Sign(nz - z);
            int signX = (int)Mathf.Sign(nx - x);

            int positiveSideVertIndex = -1, negativeSideVertIndex = -1;

            if ( x == nx )
            {
                if ( signZ > 0 )
                {
                    positiveSideVertIndex = 2;
                    negativeSideVertIndex = 4;
                }
                else
                {
                    positiveSideVertIndex = 4;
                    negativeSideVertIndex = 2;
                }
            }
            else if ( z == nz )
            {
                if ( signX > 0 )
                {
                    positiveSideVertIndex = 3;
                    negativeSideVertIndex = 1;
                }
                else
                {
                    positiveSideVertIndex = 1;
                    negativeSideVertIndex = 3;
                }
            }
            else
            {
                if ( signX > 0 && signZ > 0)
                {
                    positiveSideVertIndex = 2;
                    negativeSideVertIndex = 1;
                }
                else if ( signX > 0 && signZ < 0 )
                {
                    positiveSideVertIndex = 3;
                    negativeSideVertIndex = 2;
                }
                else if (signX < 0 && signZ < 0)
                {
                    positiveSideVertIndex = 4;
                    negativeSideVertIndex = 3;
                }
                else if (signX < 0 && signZ > 0)
                {
                    positiveSideVertIndex = 1;
                    negativeSideVertIndex = 4;
                }
            }

            triangles[index] = 0;
            triangles[index + 1] = vertIndex;
            triangles[index + 2] = vertIndex + 1;

            triangles[index + 3] = 0;
            triangles[index + 4] = vertIndex + 1;
            triangles[index + 5] = positiveSideVertIndex;

            triangles[index + 6] = 0;
            triangles[index + 7] = vertIndex + 2;
            triangles[index + 8] = vertIndex;

            triangles[index + 9] = 0;
            triangles[index + 10] = negativeSideVertIndex;
            triangles[index + 11] = vertIndex + 2;



            index += 12;
            vertIndex += 3;
        }

        return triangles;
    }

    
}
