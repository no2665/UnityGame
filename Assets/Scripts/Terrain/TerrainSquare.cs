using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSquare {
    public GameObject terrainSquare;
    public GameObject water;
    public float creationTime;

    //public TerrainSquare(GameObject t, GameObject w, float ct)
    public TerrainSquare(GameObject t, float ct)
    {
        terrainSquare = t;
        //water = w;
        creationTime = ct;
        GenerateName();
    }

    public void SetActive(bool active)
    {
        terrainSquare.SetActive(active);
        //water.SetActive(active);
    }

    public void MoveTo(Vector3 pos)
    {
        terrainSquare.transform.position = pos;
        //water.transform.position = pos + (Vector3.down * (TerrainHelper.Instance.heightScale - TerrainHelper.Instance.waterLevel)) + Vector3.forward * 5;
        terrainSquare.GetComponent<GenerateTerrain>().RegenerateMesh();
        GenerateName();
    }

    public string Name {
        get {
            return terrainSquare.name;
        }
    }

    private void GenerateName()
    {
        terrainSquare.name = GenerateName(terrainSquare.transform.position.x, terrainSquare.transform.position.z);
    }

    public static string GenerateName(float x, float z)
    {
        return "Terrain_" + ((int)x).ToString() + "_" + ((int)z).ToString();
    }
}
