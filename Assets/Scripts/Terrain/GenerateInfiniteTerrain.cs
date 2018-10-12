using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Threading;

public class GenerateInfiniteTerrain : MonoBehaviour {
    
    public GameObject player;
    public GameObject water;
    public int halfTerrainWidth = 10;
    public int halfTerrainDepth = 10;

    public const int tileSize = TerrainHelper.overallLength;

    private Vector3 startPos;

    private Hashtable tiles = new Hashtable();

    private TerrainPool pool;

    private NavMeshSurface navSurface;
    private bool shouldRebuildSurface = false;
    private bool firstBuild = true;
    
    void Start ()
    {
        pool = GetComponent<TerrainPool>();
        navSurface = GetComponent<NavMeshSurface>();
        startPos = Vector3.zero;

        // Create the initial terrain
        float updateTime = Time.realtimeSinceStartup;
        for (int x = -halfTerrainWidth; x < halfTerrainWidth; x++)
        {
            for (int z = -halfTerrainDepth; z < halfTerrainDepth; z++)
            {
                Vector3 pos = new Vector3(x * tileSize + startPos.x, 0, z * tileSize + startPos.z);
                TerrainSquare tile = pool.RequestTerrain();
                tile.MoveTo(pos);
                tiles.Add(tile.Name, tile);
            }
        }
        shouldRebuildSurface = true;
    }

    private void Update()
    {
        if ( shouldRebuildSurface )
        {
            shouldRebuildSurface = false;
            if ( firstBuild )
            {
                firstBuild = false;
                navSurface.BuildNavMesh();
            } else
            {
                navSurface.UpdateNavMesh(navSurface.navMeshData);
            }
        }
    }

    void FixedUpdate ()
    {
        int xMove = (int)(player.transform.position.x - startPos.x);
        int zMove = (int)(player.transform.position.z - startPos.z);

        // if we've moved the lenght of a tile
        if ( Mathf.Abs(xMove) >= tileSize || Mathf.Abs(zMove) >= tileSize )
        {
            float updateTime = Time.realtimeSinceStartup;

            int playerX = (int)(Mathf.Floor(player.transform.position.x / tileSize) * tileSize);
            int playerZ = (int)(Mathf.Floor(player.transform.position.z / tileSize) * tileSize);

            // Move the water so it's always under the player
            water.transform.position = new Vector3(playerX, -TerrainHelper.Instance.heightScale + TerrainHelper.Instance.waterLevel, playerZ);

            // Look to move more terrain into place
            for ( int x = -halfTerrainWidth; x < halfTerrainWidth; x++ )
            {
                for ( int z = -halfTerrainDepth; z < halfTerrainDepth; z++)
                {
                    Vector3 pos = new Vector3(x * tileSize + playerX, 0, z * tileSize + playerZ);
                    string tilename = TerrainSquare.GenerateName(pos.x, pos.z);

                    // if we don't have this terrain in place yet
                    if ( ! tiles.ContainsKey( tilename ) )
                    {
                        TerrainSquare tile = pool.RequestTerrain();
                        tile.MoveTo(pos);
                        tiles.Add(tile.Name, tile);
                    } else // terrain exists
                    {
                        // Update it's time still, so it won't be marked for deletion
                        (tiles[tilename] as TerrainSquare).creationTime = updateTime;
                    }
                }
            }

            // Delete all terrain that hasn't been updated.
            Hashtable newTerrain = new Hashtable();
            foreach ( TerrainSquare t in tiles.Values )
            {
                if (t.creationTime < updateTime )
                {
                    shouldRebuildSurface = true;
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
