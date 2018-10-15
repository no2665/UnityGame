using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LandscapeManager : MonoBehaviour {

    private TurretManager turrets;
    private RootManager roots;
    private NavMeshSurface navSurface;

    private bool shouldRebuildSurface = false;

    // Use this for initialization
    void Start () {
        turrets = GetComponent<TurretManager>();
        roots = GetComponentInChildren<RootManager>();
        navSurface = GetComponent<NavMeshSurface>();
    }

    public void FixedUpdate()
    {
        if ( shouldRebuildSurface )
        {
            shouldRebuildSurface = false;
            navSurface.UpdateNavMesh(navSurface.navMeshData);
        }
    }

    public void HandleClick(Vector3 pos)
    {
        pos.x = Mathf.Floor(pos.x);
        pos.z = Mathf.Floor(pos.z);
        // Check for root
        if ( roots.CheckForRoot(pos) )
        { // has root, place turret
            turrets.HandleClick(pos);
        } else
        {
            if ( roots.CheckValidPosition(pos) )
            {
                roots.CreateRoot(pos);
                shouldRebuildSurface = true;
            }
        }
    }
}
