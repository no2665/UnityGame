using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Creates and updates all of the turrets
 */
public class TurretManager : MonoBehaviour
{
    public GameObject player;
    public GameObject sprout;
    public GameObject basicTurret;

    public Slider energySlider;
    public float energyPerTick = 2;
    public float secondsBetweenTicks = 1;

    // Energy costs for each turret
    public static Dictionary< Turret.Type, float > turretCosts = new Dictionary< Turret.Type, float >()
    {
        { Turret.Type.GROW, 10 },
        { Turret.Type.BASIC, 15 },
        { Turret.Type.NONE, 0 },
    };
    // The upgrade flow chart. { Type1 -> { Type2, Type4 } }, { Type2 -> { Type3 } }
    public static Dictionary< Turret.Type, Turret.Type[] > upgrades = new Dictionary< Turret.Type, Turret.Type[] >
    {
        { Turret.Type.GROW, new Turret.Type[1]{ Turret.Type.BASIC } }
    };
    // Holds the prefab gameobjects above
    public static Dictionary<  Turret.Type, GameObject > turretGameObjects;

    private Hashtable turrets = new Hashtable();
    private float energy;
    private float lastEnergyAdded;

    private GameObject turretContainer;
    
    //private MovePotato potatoController;

    public void Start()
    {
        energy = energySlider.value;
        lastEnergyAdded = Time.realtimeSinceStartup;

        turretContainer = new GameObject("Turrets");
        turretContainer.transform.parent = transform;

        //potatoController = player.GetComponent<MovePotato>();

        // Load the prefabs now they've got values
        turretGameObjects = new Dictionary< Turret.Type, GameObject >
        {
            { Turret.Type.GROW, sprout },
            { Turret.Type.BASIC, basicTurret }
        };
    }

    public void FixedUpdate()
    {
        float timeNow = Time.realtimeSinceStartup;
        if ( timeNow - lastEnergyAdded >= secondsBetweenTicks )
        { // See if we can add energy
            energy += energyPerTick;
            lastEnergyAdded += secondsBetweenTicks;
        }
        if ( energy > energySlider.maxValue )
        { // Stop it going too high
            energy = energySlider.maxValue;
        }
        energySlider.value = energy;

        // Update the turrets
        foreach ( Turret t in turrets.Values )
        {
            t.FixedUpdate();
        }
    }

    public void HandleClick( Vector3 pos )
    {
        HandleClick( pos.x, pos.y, pos.z );
    }

    public void HandleClick( float x, float y, float z )
    {
        if ( GetTurret( x, z ) == Turret.Type.NONE ) 
        { // Place a turret if there isn't one here already
            float cost = turretCosts[Turret.Type.GROW];
            if ( energy >= cost )
            { // only if we have the energy
                energy -= cost;
                AddTurret( x, y, z );
            }
        }
    }

    /*
     * Creates a new turret at x, y, z
     */
    public void AddTurret( float x, float y, float z )
    {
        string name = GetTurretName( x, z );
        if ( ! turrets.ContainsKey(name) )
        {
            turrets[name] = new Turret( this, sprout, Turret.Type.GROW, Mathf.FloorToInt(x), Mathf.FloorToInt(z), turretContainer.transform, y );
        }
    }

    /*
     * See if there is a turret at x, z
     */
    public Turret.Type GetTurret( float x, float z )
    {
        string name = GetTurretName( x, z );
        if ( turrets.ContainsKey(name) )
        {
            return ((Turret) turrets[name]).type;
        }
        return Turret.Type.NONE;
    }

    /*
     * Returns true if we have enough energy to upgrade.
     * Reduces the energy, so the upgrade can be performed
     */
    public bool TryUpgrade( Turret.Type to )
    {
        if ( turretCosts.ContainsKey(to) )
        {
            float cost = turretCosts[to];
            if ( energy >= cost )
            {
                energy -= cost;
                return true;
            }
        }
        return false;
    }

    // For easily storing the turrets in the hashtable
    private string GetTurretName( float x, float z )
    {
        return "Turret_" + ((int) x).ToString() + "_" + ((int) z).ToString();
    }
}
