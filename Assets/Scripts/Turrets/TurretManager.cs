using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretManager : MonoBehaviour {

    public enum TurretType
    {
        GROW,
        BASIC,
        NONE
    }
    public GameObject sprout;

    private Hashtable turrets = new Hashtable();

    public void HandleClick(float x, float y, float z)
    {
        if (GetTurret(x, z) == TurretType.NONE) 
        {
            AddTurret(x, y, z);
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
