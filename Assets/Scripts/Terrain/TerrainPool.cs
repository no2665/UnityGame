using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPool : MonoBehaviour {

    public GameObject terrain;
    
    private List<TerrainSquare> inactiveTerrainPool;

    private GameObject activeTerrainContainer;
    private GameObject inactiveTerrainContainer;

    // Terrian width * terrain depth + an extra column + an extra row ( -1 for the extra column )
    // 20 * 20 + 20 + 19
    private readonly int poolSize = 439;

    void Start()
    {
        Vector3 startPos = Vector3.zero;
        inactiveTerrainPool = new List<TerrainSquare>();
        // Set up the containers for the terrain gameobjects
        GameObject terrainContainer = new GameObject("TerrainContainer");
        activeTerrainContainer = new GameObject("Active");
        activeTerrainContainer.transform.parent = terrainContainer.transform;
        inactiveTerrainContainer = new GameObject("Inactive");
        inactiveTerrainContainer.transform.parent = terrainContainer.transform;
        inactiveTerrainContainer.transform.position = Vector3.down * TerrainHelper.Instance.heightScale * 3;
        inactiveTerrainContainer.SetActive(false);
        // Set up the pool
        for (int s = 0; s < poolSize; s++)
        {
            CreateNewTerrain();
        }
    }

    /*
     * Get a terrain piece
     */
    public TerrainSquare RequestTerrain()
    {
        if (inactiveTerrainPool.Count == 0)
        {
            CreateNewTerrain();
        }
        TerrainSquare tile =  inactiveTerrainPool[0];
        inactiveTerrainPool.Remove(tile);
        tile.creationTime = Time.realtimeSinceStartup;
        tile.terrainMesh.transform.SetParent(activeTerrainContainer.transform);
        return tile;
    }

    /*
     * Give a terrain piece back
     */
    public void ReturnTerrain(TerrainSquare tile)
    {
        tile.terrainMesh.transform.SetParent(inactiveTerrainContainer.transform);
        inactiveTerrainPool.Add(tile);
    }

    /*
     *  Instantiate the terrain pieces here
     */
    private void CreateNewTerrain()
    {
        float updateTime = Time.realtimeSinceStartup;
        GameObject t = (GameObject)Instantiate(terrain, inactiveTerrainContainer.transform.position, Quaternion.identity, inactiveTerrainContainer.transform);
        inactiveTerrainPool.Add(new TerrainSquare(t, updateTime));
    }

}
