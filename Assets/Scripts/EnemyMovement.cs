using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour {

    public GameObject playerBase;

    private NavMeshAgent agent;

	void Start () {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(playerBase.transform.position);
	}
	
}
