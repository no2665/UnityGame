using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour
{
    public GameObject player;
    public GameObject sprout;
    public GameObject basicTurret;

    public Slider energySlider;
    public float energyPerTick = 2;
    public float secondsBetweenTicks = 1;

    public static Dictionary<Turret.Type, float> turretCosts = new Dictionary<Turret.Type, float>()
    {
        { Turret.Type.GROW, 10 },
        { Turret.Type.BASIC, 15 },
        { Turret.Type.NONE, 0 },
    };
    public static Dictionary<Turret.Type, Turret.Type[]> upgrades = new Dictionary<Turret.Type, Turret.Type[]>
    {
        { Turret.Type.GROW, new Turret.Type[1]{ Turret.Type.BASIC } }
    };
    public static Dictionary<Turret.Type, GameObject> turretGameObjects;

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

        turretGameObjects = new Dictionary<Turret.Type, GameObject>
        {
            { Turret.Type.GROW, sprout },
            { Turret.Type.BASIC, basicTurret }
        };
    }

    public void FixedUpdate()
    {
        float timeNow = Time.realtimeSinceStartup;
        if ( timeNow - lastEnergyAdded >= secondsBetweenTicks )
        {
            energy += energyPerTick;
            lastEnergyAdded += secondsBetweenTicks;
        }
        if ( energy > energySlider.maxValue )
        {
            energy = energySlider.maxValue;
        }
        energySlider.value = energy;

        // Update the turrets
        foreach ( DictionaryEntry e in turrets )
        {
            foreach ( DictionaryEntry t in (Hashtable) e.Value )
            {
                ((Turret)t.Value).FixedUpdate();
            }
        }
    }

    public void HandleClick(Vector3 pos)
    {
        HandleClick(pos.x, pos.y, pos.z);
    }

    public void HandleClick(float x, float y, float z)
    {
        if (GetTurret(x, z) == Turret.Type.NONE) 
        {
            float cost = turretCosts[Turret.Type.GROW];
            if ( energy >= cost )
            {
                energy -= cost;
                AddTurret(x, y, z);
            }
        }
    }

    public void AddTurret(float x, float y, float z)
    {
        if (!turrets.ContainsKey(x) )
        {
            turrets[x] = new Hashtable();
        }
        ((Hashtable)turrets[x])[z] = new Turret(this, sprout, Turret.Type.GROW, Mathf.FloorToInt(x), Mathf.FloorToInt(z), turretContainer.transform, y);
    }

    public Turret.Type GetTurret(float x, float z)
    {
        if ( turrets.ContainsKey(x) )
        {
            Hashtable xTurrets = (Hashtable) turrets[x];
            if ( xTurrets.ContainsKey(z) )
            {
                return ((Turret) xTurrets[z]).type;
            }
        }
        return Turret.Type.NONE;
    }

    public bool TryUpgrade(Turret.Type to)
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
}
