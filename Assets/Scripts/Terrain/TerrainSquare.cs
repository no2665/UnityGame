using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Stores a terrain mesh, and information about it.
 */
public class TerrainSquare {

    public GameObject terrainMesh;
    public float creationTime;
    
    public TerrainSquare( GameObject t, float ct )
    {
        terrainMesh = t;
        creationTime = ct;
        GenerateName();
    }

    public void SetActive( bool active )
    {
        terrainMesh.SetActive(active);
    }

    /*
     * Move the mesh, and generate a new name
     */
    public void MoveTo( Vector3 pos )
    {
        terrainMesh.transform.position = pos;
        terrainMesh.GetComponent<GenerateTerrain>().RegenerateMesh();
        GenerateName();
    }

    public string Name {
        get {
            return terrainMesh.name;
        }
    }

    private void GenerateName()
    {
        terrainMesh.name = GenerateName( terrainMesh.transform.position.x, terrainMesh.transform.position.z );
    }

    /*
     * Terrain_x_z
     */
    public static string GenerateName( float x, float z )
    {
        return "Terrain_" + ((int) x).ToString() + "_" + ((int) z).ToString();
    }
}
