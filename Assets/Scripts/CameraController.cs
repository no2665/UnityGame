using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject player;
	public GameObject lookAt;
	
	// Update is called once per frame
	void LateUpdate () {
		if ( Vector3.Distance( lookAt.transform.position, player.transform.position) > 5 ) {
			Vector3 diff = player.transform.position - lookAt.transform.position;
			lookAt.transform.position = player.transform.position - (diff.normalized * 5);
		}
	}
}
