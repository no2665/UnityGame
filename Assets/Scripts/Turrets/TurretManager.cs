using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour
{

    public enum TurretType
    {
        GROW,
        BASIC,
        NONE
    }
    public GameObject player;
    public GameObject sprout;
    public Slider energySlider;
    public float energyPerTick = 2;
    public float secondsBetweenTicks = 1;
    public Dictionary<TurretType, float> turretCosts = new Dictionary<TurretType, float>()
    {
        { TurretType.GROW, 10 },
        { TurretType.BASIC, 15 },
        { TurretType.NONE, 0 },
    };

    private Hashtable turrets = new Hashtable();
    private float energy;
    private float lastEnergyAdded;
    
    private MovePotato potatoController;

    public void Start()
    {
        energy = energySlider.value;
        lastEnergyAdded = Time.realtimeSinceStartup;

        potatoController = player.GetComponent<MovePotato>();
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
    }

    public void HandleClick(float x, float y, float z)
    {
        if (GetTurret(x, z) == TurretType.NONE) 
        {
            float cost = turretCosts[TurretType.GROW];
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
        ((Hashtable)turrets[x])[z] = TurretType.GROW;
        Instantiate(sprout, new Vector3(x, y, z), Quaternion.identity);
    }

    public TurretType GetTurret(float x, float z)
    {
        if ( turrets.ContainsKey(x) )
        {
            Hashtable xTurrets = (Hashtable) turrets[x];
            if ( xTurrets.ContainsKey(z) )
            {
                return (TurretType) xTurrets[z];
            }
        }
        return TurretType.NONE;
    }
}
