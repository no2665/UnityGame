using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Spawns and resurrects enemies.
 */
public class EnemyManager : MonoBehaviour {

    public GameObject enemy;
    public int maxEnemies = 10;
    public GameObject player;
    public float maxSecondsBetweenSpawns = 10;
    public float minSecondsBetweenSpawns = 2;

    private List<GameObject> inactiveEnemies;
    private List<GameObject> activeEnemies;
    private GameObject enemyContainer;

    private float lastSpawn = 0;
    private float timeTillNextSpawn = 0;
    private readonly int maxPositionTries = 20;

    private float terrainWidth, terrainDepth;
    private float enemyHeight;
    
	void Start () {
        // Set up variables
        inactiveEnemies = new List<GameObject>();
        activeEnemies = new List<GameObject>();

        timeTillNextSpawn = Random.Range( minSecondsBetweenSpawns, maxSecondsBetweenSpawns );

        GenerateInfiniteTerrain terrain = GetComponent<GenerateInfiniteTerrain>();
        terrainWidth = terrain.halfTerrainWidth * 2 * GenerateInfiniteTerrain.tileSize;
        terrainDepth = terrain.halfTerrainDepth * 2 * GenerateInfiniteTerrain.tileSize;

        enemyHeight = enemy.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;

        // The container for the enemies in the editor.
        enemyContainer = new GameObject("Enemies");
        // De-activate it so the enemies aren't active when they are created.
        enemyContainer.SetActive(false);

        // Create a pool of enemies, but keep them inactive for now
        for ( int i = 0; i < maxEnemies; i++ )
        {
            GameObject e = Instantiate( enemy, Vector3.zero, Quaternion.identity, enemyContainer.transform );
            e.SetActive(false);
            inactiveEnemies.Add(e);
        }
        // Safe to re-activate this now all the enemies aren't active.
        enemyContainer.SetActive(true);
    }
	
	void FixedUpdate () {
        if ( inactiveEnemies.Count > 0 )
        { // see if we should spawn an enemy
            float timeNow = Time.realtimeSinceStartup;
            if ( timeNow > lastSpawn + timeTillNextSpawn )
            {
                lastSpawn = timeNow;
                timeTillNextSpawn = Random.Range( minSecondsBetweenSpawns, maxSecondsBetweenSpawns );
                // Our enemy
                GameObject e = inactiveEnemies[0];

                // Now we need to find a position for the enemy to spawn.

                float playerX = player.transform.position.x;
                float playerZ = player.transform.position.z;

                // We are only going to try so many random positions before we give up.
                // Though unlikely, we don't want to be getting unlucky forever.
                for ( int i = 0; i < maxPositionTries; i++ )
                {
                    // Our random position, have to set y to the terrain height
                    Vector3 randomPosition = new Vector3( Random.Range( playerX - (terrainWidth / 2), playerX + (terrainWidth / 2) ), 0, Random.Range( playerZ - (terrainDepth / 2), playerZ + (terrainDepth / 2) ) );
                    randomPosition.y = TerrainHelper.Instance.GetRealYAt(randomPosition.x, randomPosition.z);

                    NavMeshHit navMeshPosition;
                    // This will give us a valid position on the nav mesh, if it can
                    if ( NavMesh.SamplePosition( randomPosition, out navMeshPosition, enemyHeight * 2, NavMesh.AllAreas ) )
                    {
                        // valid position found
                        randomPosition = navMeshPosition.position;
                        e.transform.position = randomPosition;

                        // activate the enemy.
                        e.SetActive(true);
                        inactiveEnemies.Remove(e);
                        activeEnemies.Add(e);

                        break;
                    }
                }

            }
        }

        if ( activeEnemies.Count > 0 )
        { // See if we need to do any clean up
            // Bad idea to remove from a list in a loop, so store them for now
            List<GameObject> toRemove = new List<GameObject>();
            foreach ( GameObject enemy in activeEnemies )
            {
                if ( enemy.GetComponent<EnemyController>().IsDead() )
                { // De-activate dead enemies
                    enemy.SetActive(false);
                    inactiveEnemies.Add(enemy);
                    toRemove.Add(enemy);
                }
            }

            // Remove here
            foreach ( GameObject e in toRemove )
            {
                activeEnemies.Remove(e);
            }
        }
    }
}
