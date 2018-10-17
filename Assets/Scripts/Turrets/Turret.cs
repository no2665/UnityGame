using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret {

    public enum Type
    {
        GROW,
        BASIC,
        NONE
    }

    public Type type;
    public int x, z;
    public GameObject turret;
    public TurretManager manager;

    private float plantTime = 0;

    private static Dictionary<Type, float> growTime = new Dictionary<Type, float>
    {
        { Type.GROW, 5 },
        { Type.BASIC, 10000 }
    };

    private static Dictionary<Type, float[]> turretOffsets = new Dictionary<Type, float[]>
    {
        { Type.GROW, new float[2]{ 0.5f, 0.5f } },
        { Type.BASIC, new float[2]{ 0.5f, 0.5f } }
    };

    public Turret(TurretManager m, GameObject g, Type t, int xPos, int zPos, Transform parent, float yPos = 0)
    {
        manager = m;
        type = t;
        x = xPos;
        z = zPos;

        float[] offsets = turretOffsets[t];
        turret = MonoBehaviour.Instantiate(g, new Vector3(x + offsets[0], yPos, z + offsets[1]), Quaternion.identity, parent);

        plantTime = Time.realtimeSinceStartup;
    }

    public void FixedUpdate()
    {
        float timeNow = Time.realtimeSinceStartup;
        if ( timeNow >= plantTime + growTime[type] )
        {
            if ( TurretManager.upgrades.ContainsKey(type))
            {
                Type[] upgrades = TurretManager.upgrades[type];
                Type upgrade = upgrades[Random.Range(0, upgrades.Length - 1)];
                if ( manager.TryUpgrade(upgrade) )
                {
                    UpgradeTo(upgrade);
                }
            }
        }
    }

    private void UpgradeTo(Type to)
    {
        float[] offsets = turretOffsets[to];
        GameObject newTurret = MonoBehaviour.Instantiate(TurretManager.turretGameObjects[to], new Vector3(x + offsets[0], turret.transform.position.y, z + offsets[1]), Quaternion.identity, turret.transform.parent);

        type = to;
        plantTime = Time.realtimeSinceStartup;

        MonoBehaviour.Destroy(turret);
        turret = newTurret;
    }
}
