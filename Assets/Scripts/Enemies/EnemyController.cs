using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Controls an enemy. Sets the nav agents destination. Makes the enemy attack things.
 * Plays the enemy animations.
 */
public class EnemyController : MonoBehaviour {

    public float attackRadius = 10;
    public float reloadTime = 5;
    public float damage = 1;

    private NavMeshAgent agent;

    private float lastAttack = 0;

    private GameObject playerBase;
    private Health baseHealth;
    private Vector3 baseLocation;

    private Health health;
    private RectTransform healthBarTransform;
    private GameObject mainCamera;
    private float deathTime = 0;
    private Vector3 deathLocation = Vector3.zero;
    private readonly float deathAnimationSecs = 2;
    private bool isDying = false;
    private bool isDead = false;

    // Called before anything else
    void Awake () {
        // Just set up variables
        health = GetComponent<Health>();
        healthBarTransform = GetComponentInChildren<RectTransform>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        playerBase = GameObject.FindGameObjectWithTag("Base");

        baseHealth = playerBase.GetComponent<Health>();
        baseLocation = playerBase.transform.position;

        agent = GetComponent<NavMeshAgent>();
    }

    // Called after awake, and whenever the enemy is resurrected.
    private void OnEnable()
    {
        isDying = false;
        isDead = false;

        SetAgentDestination();
    }

    /*
     * Find the destination for the nav agent, and get it going
     */
    private void SetAgentDestination()
    {
        // The base is a circle area. Find the point on the edge of the area closest to our enemy.
        float baseRadius = playerBase.GetComponent<RootManager>().baseRadius;
        Vector3 noYEnemyPosition = new Vector3( transform.position.x, 0, transform.position.z );
        Vector3 noYBasePosition = new Vector3( baseLocation.x, 0, baseLocation.z );
        Vector3 baseToEnemy = noYEnemyPosition - noYBasePosition;

        Vector3 destination = baseLocation + ( baseToEnemy.normalized * baseRadius );
        
        agent.enabled = true;
        agent.SetDestination(destination);
    }

    private void FixedUpdate()
    {
        if ( isDying || health.currentHealth <= 0 )
        { // start the death animation
            if ( isDying )
            {
                // Lerp our position down, so we sink into the ground
                // TODO: create a real animation
                float t = ( Time.realtimeSinceStartup - deathTime ) / deathAnimationSecs;
                transform.position = Vector3.Lerp( deathLocation, deathLocation + (Vector3.down * 2), t );
                if ( t > deathAnimationSecs )
                {
                    isDead = true; // Can be resurrected
                }
            }
            else
            { // first time health has dropped to zero.
                isDying = true;
                deathTime = Time.realtimeSinceStartup;
                deathLocation = transform.position;
                // disabled the agent so it doesn't reset our movements.
                agent.enabled = false;
            }
        }
        else
        { // healthy enemy
            if ( Vector3.Distance( transform.position, baseLocation ) <= attackRadius )
            { // attack the base if we can.
                // TODO: create an attack animation
                float timeNow = Time.realtimeSinceStartup;
                if ( timeNow > lastAttack + reloadTime )
                {
                    lastAttack = timeNow;
                    baseHealth.TakeHealth(damage);
                }
            }
        }

        // rotate the healthbar so it's facing the camera.
        // TODO: make this look better.
        Vector3 v = mainCamera.transform.position - healthBarTransform.transform.position;
        v.y = 0.0f;
        healthBarTransform.transform.LookAt( mainCamera.transform.position - v );
    }

    /*
     * Whether or not the enemy has died.
     */
    public bool IsDead()
    {
        return isDead;
    }
}
