using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    private int maxPositionTries = 20;

    private float terrainWidth, terrainDepth;
    private float enemyHeight;
    
	void Start () {
        inactiveEnemies = new List<GameObject>();
        activeEnemies = new List<GameObject>();
        enemyContainer = new GameObject("Enemies");
        enemyContainer.SetActive(false);

        timeTillNextSpawn = Random.Range(minSecondsBetweenSpawns, maxSecondsBetweenSpawns);

        GenerateInfiniteTerrain terrain = GetComponent<GenerateInfiniteTerrain>();
        terrainWidth = terrain.halfTerrainWidth * 2 * GenerateInfiniteTerrain.tileSize;
        terrainDepth = terrain.halfTerrainDepth * 2 * GenerateInfiniteTerrain.tileSize;

        for ( int i = 0; i < maxEnemies; i++ )
        {
            GameObject e = Instantiate(enemy, Vector3.zero, Quaternion.identity, enemyContainer.transform);
            e.SetActive(false);
            inactiveEnemies.Add(e);
        }
        enemyContainer.SetActive(true);

        enemyHeight = enemy.GetComponent<MeshFilter>().sharedMesh.bounds.size.y;
    }
	
	void FixedUpdate () {
        if ( inactiveEnemies.Count > 0 )
        {
            float timeNow = Time.realtimeSinceStartup;
            if (timeNow > lastSpawn + timeTillNextSpawn)
            {
                lastSpawn = timeNow;
                timeTillNextSpawn = Random.Range(minSecondsBetweenSpawns, maxSecondsBetweenSpawns);
                GameObject e = inactiveEnemies[0];

                float playerX = player.transform.position.x;
                float playerZ = player.transform.position.z;
                
                for ( int i = 0; i < maxPositionTries; i++ )
                {
                    Vector3 randomPosition = new Vector3(Random.Range(playerX - (terrainWidth / 2), playerX + (terrainWidth / 2)), 0, Random.Range(playerZ - (terrainDepth / 2), playerZ + (terrainDepth / 2)));
                    randomPosition.y = TerrainHelper.Instance.GetRealYAt(randomPosition.x, randomPosition.z);

                    NavMeshHit navMeshPosition;

                    if ( NavMesh.SamplePosition(randomPosition, out navMeshPosition, enemyHeight * 2, NavMesh.AllAreas) )
                    {
                        randomPosition = navMeshPosition.position;
                        e.transform.position = randomPosition;

                        e.SetActive(true);
                        inactiveEnemies.Remove(e);
                        activeEnemies.Add(e);

                        break;
                    }
                }

            }
        }

        if ( activeEnemies.Count > 0 )
        {
            List<GameObject> toRemove = new List<GameObject>();
            foreach ( GameObject enemy in activeEnemies )
            {
                if ( enemy.GetComponent<EnemyController>().IsDead() )
                {
                    enemy.SetActive(false);
                    inactiveEnemies.Add(enemy);
                    toRemove.Add(enemy);
                }
            }

            foreach ( GameObject e in toRemove )
            {
                activeEnemies.Remove(e);
            }
        }
    }
}
