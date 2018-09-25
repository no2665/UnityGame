using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateInfiniteTerrain : MonoBehaviour {
    
    public GameObject player;
    public GameObject water;

    int terrainSize = 10;
    int halfTilesX = 10;
    int halfTilesZ = 10;

    Vector3 startPos;

    Hashtable tiles = new Hashtable();

    private TerrainPool pool;

    // Use this for initialization
    void Start ()
    {
        pool = gameObject.GetComponent<TerrainPool>();
        startPos = Vector3.zero;

        float updateTime = Time.realtimeSinceStartup;

        for (int x = -halfTilesX; x < halfTilesX; x++)
        {
            for (int z = -halfTilesZ; z < halfTilesZ; z++)
            {
                Vector3 pos = new Vector3(x * terrainSize + startPos.x, 0, z * terrainSize + startPos.z);
                TerrainSquare tile = pool.RequestTerrain();
                tile.MoveTo(pos);
                tiles.Add(tile.Name, tile);
            }
        }
		
	}

	// Update is called once per frame
	void Update ()
    {
        int xMove = (int)(player.transform.position.x - startPos.x);
        int zMove = (int)(player.transform.position.z - startPos.z);

        if ( Mathf.Abs(xMove) >= terrainSize || Mathf.Abs(zMove) >= terrainSize )
        {
            float updateTime = Time.realtimeSinceStartup;

            int playerX = (int)(Mathf.Floor(player.transform.position.x / terrainSize) * terrainSize);
            int playerZ = (int)(Mathf.Floor(player.transform.position.z / terrainSize) * terrainSize);

            water.transform.position = new Vector3(playerX, -TerrainHelper.Instance.heightScale + TerrainHelper.Instance.waterLevel, playerZ);

            for ( int x = -halfTilesX; x < halfTilesX; x++ )
            {
                for ( int z = -halfTilesZ; z < halfTilesZ; z++)
                {
                    Vector3 pos = new Vector3(x * terrainSize + playerX, 0, z * terrainSize + playerZ);
                    string tilename = TerrainSquare.GenerateName(pos.x, pos.z);

                    if ( ! tiles.ContainsKey( tilename ) )
                    {
                        TerrainSquare tile = pool.RequestTerrain();
                        tile.MoveTo(pos);
                        tiles.Add(tile.Name, tile);
                    } else
                    {
                        (tiles[tilename] as TerrainSquare).creationTime = updateTime;
                    }
                }
            }

            Hashtable newTerrain = new Hashtable();
            foreach ( TerrainSquare t in tiles.Values )
            {
                if (t.creationTime < updateTime )
                {
                    pool.ReturnTerrain(t);
                } else
                {
                    newTerrain.Add(t.Name, t);
                }
            }
            tiles = newTerrain;

            startPos = player.transform.position;
        }
	}
}
