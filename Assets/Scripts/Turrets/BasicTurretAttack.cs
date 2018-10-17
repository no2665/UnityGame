using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTurretAttack : TurretAttack {

    public float attackRadius = 5;
    public float damage = 5;
    public float reloadTime = 5;

    private void Start()
    {
        PassStats(attackRadius, damage, reloadTime);
    }

}
