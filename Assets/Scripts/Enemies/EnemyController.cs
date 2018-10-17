using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    void Awake () {
        health = GetComponent<Health>();
        healthBarTransform = GetComponentInChildren<RectTransform>();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        playerBase = GameObject.FindGameObjectWithTag("Base");

        baseHealth = playerBase.GetComponent<Health>();
        baseLocation = playerBase.transform.position;

        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        isDying = false;
        isDead = false;

        SetAgentDestination();
    }

    private void SetAgentDestination()
    {
        float baseRadius = playerBase.GetComponent<RootManager>().baseRadius;
        Vector3 noYEnemyPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 noYBasePosition = new Vector3(baseLocation.x, 0, baseLocation.z);
        Vector3 baseToEnemy = noYEnemyPosition - noYBasePosition;

        Vector3 destination = baseLocation + (baseToEnemy.normalized * baseRadius);
        
        agent.enabled = true;
        agent.SetDestination(destination);
    }

    private void FixedUpdate()
    {
        if ( isDying || health.currentHealth <= 0 )
        {
            if ( isDying )
            {
                float t = (Time.realtimeSinceStartup - deathTime) / deathAnimationSecs;
                transform.position = Vector3.Lerp(deathLocation, deathLocation + (Vector3.down * 2), t);
                if ( t > deathAnimationSecs)
                {
                    isDead = true;
                }
            } else
            {
                isDying = true;
                deathTime = Time.realtimeSinceStartup;
                deathLocation = transform.position;
                agent.enabled = false;
            }
        } else
        {
            if (Vector3.Distance(transform.position, baseLocation) <= attackRadius)
            {
                float timeNow = Time.realtimeSinceStartup;
                if (timeNow > lastAttack + reloadTime)
                {
                    lastAttack = timeNow;
                    baseHealth.TakeHealth(damage);
                }
            }
        }

        Vector3 v = mainCamera.transform.position - healthBarTransform.transform.position;
        v.y = 0.0f;
        healthBarTransform.transform.LookAt(mainCamera.transform.position - v);
    }

    public bool IsDead()
    {
        return isDead;
    }

}
