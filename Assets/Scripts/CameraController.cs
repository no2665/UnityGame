using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Follows the player to make sure they remain in frame. The camera only moves when the
 * player gets near the cameras edge
 */
public class CameraController : MonoBehaviour {

	public GameObject player;
	public GameObject lookAt;
	
    // Called after update, lets the player move first
	void LateUpdate () {
		if ( Vector3.Distance( lookAt.transform.position, player.transform.position ) > 5 ) {
			Vector3 diff = player.transform.position - lookAt.transform.position;
			lookAt.transform.position = player.transform.position - ( diff.normalized * 5 );
		}
	}
}
