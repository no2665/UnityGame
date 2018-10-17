using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Methods for making a turret attack enemies.
 * This should not be attached to any turret, you should instead create a new class
 * that inherits this one. There you can pass this class the turrets stats and/or
 * override any methods. These methods implement a simple attack.
 */
public abstract class TurretAttack : MonoBehaviour {

    private float abAttackRadius;
    private float abDamage;
    private float abReloadTime;

    private GameObject enemyBeingAttacked;
    private float lastAttackTime = 0;

    public void PassStats( float attackRadius, float damage, float reloadTime )
    {
        abAttackRadius = attackRadius;
        abDamage = damage;
        abReloadTime = reloadTime; 
    }
	
	void FixedUpdate () {
        // Try and focus on one enemy at a time.
        if ( enemyBeingAttacked != null && enemyBeingAttacked.activeSelf && CheckDistance( transform.position, enemyBeingAttacked.transform.position ) )
        {
            Attack( enemyBeingAttacked.GetComponent<Health>() );
        }
        else
        {
            enemyBeingAttacked = null;
            // Get all of the enemies.
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            for ( int i = 0; i < enemies.Length; i++ )
            {
                // Attack the first enemy in range
                if ( CheckDistance( transform.position, enemies[i].transform.position ) )
                {
                    enemyBeingAttacked = enemies[i];
                    Attack( enemyBeingAttacked.GetComponent<Health>() );
                    break;
                }
            }
        }
	}

    public bool CheckDistance( Vector3 from, Vector3 to )
    {
        return Vector3.Distance( from, to ) <= abAttackRadius;
    }

    public void Attack( Health health )
    {
        // If we can attack, attack
        float timeNow = Time.realtimeSinceStartup;
        if ( timeNow >= lastAttackTime + abReloadTime )
        {
            lastAttackTime = timeNow;
            health.TakeHealth(abDamage);
        }
    }
}
