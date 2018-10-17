using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurretAttack : MonoBehaviour {

    private float abAttackRadius;
    private float abDamage;
    private float abReloadTime;

    private GameObject enemyBeingAttacked;
    private float lastAttackTime = 0;

    public void PassStats(float attackRadius, float damage, float reloadTime)
    {
        abAttackRadius = attackRadius;
        abDamage = damage;
        abReloadTime = reloadTime; 
    }
	
	void FixedUpdate () {
        if ( enemyBeingAttacked != null && enemyBeingAttacked.activeSelf && CheckDistance(transform.position, enemyBeingAttacked.transform.position) )
        {
            Attack(enemyBeingAttacked.GetComponent<Health>());
        } else
        {
            enemyBeingAttacked = null;
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            for (int i = 0; i < enemies.Length; i++)
            {
                if ( CheckDistance(transform.position, enemies[i].transform.position) )
                {
                    enemyBeingAttacked = enemies[i];
                    Attack( enemyBeingAttacked.GetComponent<Health>() );
                    break;
                }
            }
        }
	}

    public bool CheckDistance(Vector3 from, Vector3 to)
    {
        return Vector3.Distance(from, to) <= abAttackRadius;
    }

    public void Attack(Health health)
    {
        float timeNow = Time.realtimeSinceStartup;
        if ( timeNow >= lastAttackTime + abReloadTime )
        {
            lastAttackTime = timeNow;
            health.TakeHealth(abDamage);
        }
    }
}
