using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Oversees the TurretManager and RootManager. Passes them clicks appropriately.
 */
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
        { // Rebuild the nav mesh if the landscape has changed
            shouldRebuildSurface = false;
            navSurface.UpdateNavMesh(navSurface.navMeshData);
        }
    }

    public void HandleClick( Vector3 pos )
    {
        pos.x = Mathf.Floor(pos.x);
        pos.z = Mathf.Floor(pos.z);
        // Check for root
        // Turrets must be placed on roots
        if ( roots.CheckForRoot(pos) )
        { // has root, place turret
            turrets.HandleClick(pos);
        }
        else
        {
            // Make sure the root doesn't block the path
            if ( roots.CheckValidPosition(pos) )
            {
                roots.CreateRoot(pos);
                shouldRebuildSurface = true;
            }
        }
    }
}
