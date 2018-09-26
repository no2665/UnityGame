using UnityEngine;

using LibNoise;
using LibNoise.Generator;

[ExecuteInEditMode]
public class TerrainHelper : MonoBehaviour {

    public const int numVerticesX = 3;
    public const int numVerticesZ = 3;
    public const int edgeLength = 5;

    public float heightScale = 5.0f;
    public float detailScale = 5.0f;

    public float steps = 4.0f;

    public float fidgetScale = 5.0f;
    public float fidgetRange = 1.0f;

    public float waterLevel = 3;

    private static TerrainHelper instance = null;

    private Perlin fidgetPerlin = new Perlin(0.04, 1.0, 0.25, 3, System.DateTime.Now.Millisecond, QualityMode.High);
    private Perlin heightPerlin = new Perlin(0.08, 3.0, 0.75, 2, System.DateTime.Now.Millisecond, QualityMode.High);

    private readonly float perlinOffsetX1 = 45000.001f;
    private readonly float perlinOffsetY1 = 25000.001f;
    private readonly float perlinOffsetZ1 = 30000.001f;

    // Create a singleton
    public static TerrainHelper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TerrainHelper>();
                if (instance == null)
                {
                    GameObject go = new GameObject("TerrainHelper");
                    instance = go.AddComponent<TerrainHelper>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (!Application.isEditor)
            {
                DontDestroyOnLoad(this.gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Gets the value of y at x, z coordinates. Y is returned in discrete steps. Used for getting the y value of the vertices in the terrain mesh
    public float GetYAt(float x, float z)
    {
        // Get a value from the perlin generator, and map it to 0 to 1
        float perlin1 = (float)heightPerlin.GetValue((x + perlinOffsetY1) / detailScale, 0, (z + perlinOffsetY1) / detailScale);
        perlin1 = MathMap(perlin1, -1, 1, 0, 1);
        // Rewrite it to be from -heightScale to 0
        float y = (perlin1 * heightScale) - heightScale;
        // Used to finding which discrete value y should be set to
        float smallestDiff = float.MaxValue;
        float steppedY = y;
        // Go through each discrete value, to find which is closest to y, and set y to that value
        for (float i = -heightScale; i <= 0; i += heightScale / (steps - 1))
        {
            float diff = Mathf.Abs(y - i);
            if (diff < smallestDiff)
            {
                steppedY = i;
                smallestDiff = diff;
            }
        }
        return steppedY;
    }

    // Gets the value of y at x, z coordinates. Y is returned as a continuous value, so slopes are accounted for. Useful for placing objects on the terrain
    public float GetRealYAt(float x, float z)
    {
        // We're going to find the four vertices that surround our point

        // Find closest x coordinate.
        float roundDownX = (Mathf.RoundToInt(x) / edgeLength) * edgeLength;
        float roundUpX = roundDownX + (edgeLength * Mathf.Sign(roundDownX));
        float closestX = roundDownX;
        if (roundUpX - x < x - roundDownX)
        {
            closestX = roundUpX;
        }
        // Find closest z coordinate.
        float roundDownZ = (Mathf.RoundToInt(z) / edgeLength) * edgeLength;
        float roundUpZ = roundDownZ + (edgeLength * Mathf.Sign(roundDownZ));
        float closestZ = roundDownZ;
        if (roundUpZ - z < z - roundDownZ)
        {
            closestZ = roundUpZ;
        }

        // Get x, z of closest vertex
        float fidgetClosestX = closestX + GetXFidgetAt(closestX, closestZ);
        float fidgetClosestZ = closestZ + GetZFidgetAt(closestX, closestZ);

        // Find x, z of the vertex along the x axis that is part of the triangle we are in.
        float directionX = -1;
        if (x > fidgetClosestX)
        {
            directionX = 1;
        }
        float nextX = closestX + (edgeLength * directionX);
        float fidgetNextXX = nextX + GetXFidgetAt(nextX, closestZ);
        float fidgetNextXZ = closestZ + GetZFidgetAt(nextX, closestZ);

        // Find x, z of the vertex along the z axis
        float directionZ = -1;
        if (z > fidgetClosestZ)
        {
            directionZ = 1;
        }
        float nextZ = closestZ + (edgeLength * directionZ);
        float fidgetNextZZ = nextZ + GetZFidgetAt(closestX, nextZ);
        float fidgetNextZX = closestX + GetXFidgetAt(closestX, nextZ);

        // Find the x, z of vertex opposite our closest vertex, given the other two vertices above
        float furthestX = nextX;
        float furthestZ = nextZ;
        float fidgetFurthestX = furthestX + GetXFidgetAt(furthestX, furthestZ);
        float fidgetFurthestZ = furthestZ + GetZFidgetAt(furthestX, furthestZ);

        // Find the heights of all four vertices
        float nearVertexHeight = GetYAt(closestX, closestZ);
        float nextXVertexHeight = GetYAt(nextX, closestZ);
        float nextZVertexHeight = GetYAt(closestX, nextZ);
        float furthestVertexHeight = GetYAt(furthestX, furthestZ);

        // Return early if there is no slope
        if (nearVertexHeight == nextXVertexHeight && nextXVertexHeight == nextZVertexHeight && nextZVertexHeight == furthestVertexHeight)
        {
            return nearVertexHeight;
        }

        // Decide which vertices make up the triangle we are in.
        Vector3[] vertices = new Vector3[3];
        // Closest vertex is always included
        vertices[0] = new Vector3(fidgetClosestX, nearVertexHeight, fidgetClosestZ);
        // if there are two triangles to pick from
        // -----
        // |  /|
        // | / |
        // |/  |
        // X----
        if (directionX == directionZ)
        {
            // Furthest is always included in this case
            vertices[1] = new Vector3(fidgetFurthestX, furthestVertexHeight, fidgetFurthestZ);

            Vector2 middleLine = (new Vector2(fidgetFurthestX, fidgetFurthestZ)) - (new Vector2(fidgetClosestX, fidgetClosestZ));
            Vector2 closestToPoint = (new Vector2(x, z)) - (new Vector2(fidgetClosestX, fidgetClosestZ));
            Vector2 testXLine = (new Vector2(fidgetNextXX, fidgetNextXZ)) - (new Vector2(fidgetClosestX, fidgetClosestZ));
            if ( Mathf.Abs(Vector2.Angle(closestToPoint, testXLine)) < Mathf.Abs(Vector2.Angle(middleLine, testXLine)) )
            {
                vertices[2] = new Vector3(fidgetNextXX, nextXVertexHeight, fidgetNextXZ);
            } else
            {
                vertices[2] = new Vector3(fidgetNextZX, nextZVertexHeight, fidgetNextZZ);
            }


        } else // We can only pick one triangle so disregard furthest
        {
            // X----
            // |  /|
            // | / |
            // |/  |
            // -----
            vertices[1] = new Vector3(fidgetNextXX, nextXVertexHeight, fidgetNextXZ);
            vertices[2] = new Vector3(fidgetNextZX, nextZVertexHeight, fidgetNextZZ);
        }

        // Use barycentric coordinates
        float det = (vertices[1].z - vertices[2].z) * (vertices[0].x - vertices[2].x) + (vertices[2].x - vertices[1].x) * (vertices[0].z - vertices[2].z);
        float l1 = ((vertices[1].z - vertices[2].z) * (x - vertices[2].x) + (vertices[2].x - vertices[1].x) * (z - vertices[2].z)) / det;
        float l2 = ((vertices[2].z - vertices[0].z) * (x - vertices[2].x) + (vertices[0].x - vertices[2].x) * (z - vertices[2].z)) / det;
        float l3 = 1.0f - l1 - l2;

        return l1 * vertices[0].y + l2 * vertices[1].y + l3 * vertices[2].y;
    }

    // Gets a fidget amount of which to move the x coordinate of the terrain mesh
    public float GetXFidgetAt(float x, float z)
    {
        float perlin1 = (float)fidgetPerlin.GetValue((x + perlinOffsetX1) / fidgetScale, 0, (z + perlinOffsetX1) / fidgetScale);
        perlin1 = MathMap(perlin1, -1, 1, 0, 1);
        return (perlin1 * (fidgetRange * 2)) - fidgetRange;
    }

    // Gets a fidget amount of which to move the z coordinate of the terrain mesh
    public float GetZFidgetAt(float x, float z)
    {
        float perlin1 = (float)fidgetPerlin.GetValue((x + perlinOffsetZ1) / fidgetScale, 0, (z + perlinOffsetZ1) / fidgetScale);
        perlin1 = MathMap(perlin1, -1, 1, 0, 1);
        return (perlin1 * (fidgetRange * 2)) - fidgetRange;
    }

    private float MathMap(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }
}
