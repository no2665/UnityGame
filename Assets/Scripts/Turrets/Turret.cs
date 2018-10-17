using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Creates a turrets and possibly upgrades it.
 * Doesn't inherit from MonoBehaviour because it isn't attached to a GameObject,
 * and we are not allowed to 'new' MonoBehaviours.
 */
public class Turret {

    /*
     * All turret types available
     */
    public enum Type
    {
        GROW, // Just a sprout, doesn't attack
        BASIC, // Simple attack
        NONE // No turret
    }

    public Type type;
    public int x, z;
    public GameObject turret;
    public TurretManager manager;

    private float plantTime = 0;

    /*
     * Times before each turret can upgrade
     */
    private static Dictionary< Type, float > growTime = new Dictionary< Type, float >
    {
        { Type.GROW, 8 },
        { Type.BASIC, 10000 }
    };

    /*
     * Values used to centre the gameobjects. new float[2]{ x, z }
     */
    private static Dictionary< Type, float[] > turretOffsets = new Dictionary< Type, float[] >
    {
        { Type.GROW, new float[2]{ 0.5f, 0.5f } },
        { Type.BASIC, new float[2]{ 0.5f, 0.5f } }
    };

    public Turret( TurretManager m, GameObject g, Type t, int xPos, int zPos, Transform parent, float yPos = 0 )
    {
        manager = m;
        type = t;
        x = xPos;
        z = zPos;

        // Create the turret here
        float[] offsets = turretOffsets[t];
        turret = MonoBehaviour.Instantiate( g, new Vector3( x + offsets[0], yPos, z + offsets[1] ), Quaternion.identity, parent );

        plantTime = Time.realtimeSinceStartup;
    }

    /*
     * As we don't inherit from MonoBehaviour, we don't get this for free.
     * This is called from the TurretManager
     */
    public void FixedUpdate()
    {
        float timeNow = Time.realtimeSinceStartup;
        if ( timeNow >= plantTime + growTime[type] )
        { // Check if we can upgrade
            // TODO: don't upgrade automatically. Let the user decide (Create an interface)
            if ( TurretManager.upgrades.ContainsKey(type) )
            {
                Type[] upgrades = TurretManager.upgrades[type];
                Type upgrade = upgrades[ Random.Range( 0, upgrades.Length - 1 ) ];
                if ( manager.TryUpgrade(upgrade) ) // Manager spends the energy for us
                {
                    UpgradeTo(upgrade);
                }
            }
        }
    }

    /*
     * Create a new turret, and destroy the old one
     */
    private void UpgradeTo( Type to )
    {
        float[] offsets = turretOffsets[to];
        GameObject newTurret = MonoBehaviour.Instantiate( TurretManager.turretGameObjects[to], new Vector3( x + offsets[0], turret.transform.position.y, z + offsets[1] ), Quaternion.identity, turret.transform.parent );

        type = to;
        plantTime = Time.realtimeSinceStartup;

        MonoBehaviour.Destroy(turret);
        turret = newTurret;
    }
}
