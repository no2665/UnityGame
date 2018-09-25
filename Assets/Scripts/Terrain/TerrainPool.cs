using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPool : MonoBehaviour {

    public GameObject terrain;
    //public GameObject water;
    
    private List<TerrainSquare> inactiveTerrainPool;

    private GameObject activeTerrainContainer;
    private GameObject inactiveTerrainContainer;

    // 20 * 20 + 20 + 19
    private readonly int poolSize = 439;

    void Start()
    {
        Vector3 startPos = Vector3.zero;
        inactiveTerrainPool = new List<TerrainSquare>();
        GameObject terrainContainer = new GameObject("TerrainContainer");
        activeTerrainContainer = new GameObject("Active");
        activeTerrainContainer.transform.parent = terrainContainer.transform;
        inactiveTerrainContainer = new GameObject("Inactive");
        inactiveTerrainContainer.transform.parent = terrainContainer.transform;
        inactiveTerrainContainer.transform.position = Vector3.down * TerrainHelper.Instance.heightScale * 3;
        inactiveTerrainContainer.SetActive(false);

        for (int s = 0; s < poolSize; s++)
        {
            CreateNewTerrain();
        }
    }

    public TerrainSquare RequestTerrain()
    {
        if (inactiveTerrainPool.Count == 0)
        {
            CreateNewTerrain();
        }
        TerrainSquare tile =  inactiveTerrainPool[0];
        inactiveTerrainPool.Remove(tile);
        tile.creationTime = Time.realtimeSinceStartup;
        tile.terrainSquare.transform.SetParent(activeTerrainContainer.transform);
        return tile;
    }

    public void ReturnTerrain(TerrainSquare tile)
    {
        tile.terrainSquare.transform.SetParent(inactiveTerrainContainer.transform);
        inactiveTerrainPool.Add(tile);
    }

    private void CreateNewTerrain()
    {
        float updateTime = Time.realtimeSinceStartup;
        GameObject t = (GameObject)Instantiate(terrain, inactiveTerrainContainer.transform.position, Quaternion.identity, inactiveTerrainContainer.transform);
        //GameObject w = (GameObject)Instantiate(water, t.transform.position + Vector3.forward * 5, Quaternion.identity, t.transform);
        //inactiveTerrainPool.Add(new TerrainSquare(t, w, updateTime));
        inactiveTerrainPool.Add(new TerrainSquare(t, updateTime));
    }

}
