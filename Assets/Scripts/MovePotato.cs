using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class MovePotato : MonoBehaviour {
    
    public float idleSeconds = 30;
    public float turnSpeed = 275;
    public float speed = 100;
    public float stopSeconds = 2;
    public float standUpRotateAngle = 20;
    public float jumpForce = 10;

    private Rigidbody potato;
    private Animator potatoAC;
    private float lastInputTime = 0;

    private bool isRolling = false;
    private float lastV = 0;
    private float lastRollTime;
    private Quaternion lastRollQuat;
    private Vector3 lastRollForward;
    private bool standingUp = false;
    private Vector3 upOrDown = Vector3.zero;
    private readonly float standUpAcceptableError = 0.0001f;

    private float floorFallError = 0;

    private bool jumping = false;

    // Use this for initialization
    void Start () {
        // Put the potato on the ground.
        transform.position = new Vector3( 0, TerrainHelper.Instance.GetRealYAt( 0, 0 ) + 0.01f, 0 );
        // Initialise variables
        potato = GetComponent<Rigidbody>();
        potatoAC = GetComponent<Animator>();
        lastInputTime = Time.realtimeSinceStartup;
        // Mesh is on it's side
        floorFallError = GetComponentInChildren<MeshCollider>().sharedMesh.bounds.size.x / 4;
    }
	
	// Update is called once per frame
	void Update () {

        float timeNow = Time.realtimeSinceStartup;

        // Get input
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float extraSpeed = 1f + CrossPlatformInputManager.GetAxis("Fire3");
        float jump = CrossPlatformInputManager.GetAxis("Jump");

        // if we have input
        if ( v != 0 || h != 0 || jump != 0 )
        {
            // save the time of last input
            lastInputTime = timeNow;
        }

        // if we're moving forwards or backwards
        if ( v != 0 )
        {
            // Save the direction
            lastV = v > 0 ? 1 : -1;

            // Roll the potato
            potato.AddTorque( transform.right * lastV * speed * extraSpeed, ForceMode.Force );

            // Start rolling
            isRolling = true;

            // Save these so we can straighten up later
            lastRollTime = timeNow;
            lastRollQuat = new Quaternion( transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w );
            lastRollForward = ( new Vector3( transform.forward.x, 0, transform.forward.z ) ).normalized;

            // Animate
            potatoAC.SetBool("Is moving", true);

        }
        else if ( isRolling ) // No input but we're not straight yet
        {
            // Get the rotation, from 0 to 180, to determine if we are straight
            float rotX = Mathf.Abs(transform.rotation.eulerAngles.x);
            float modRotX = rotX % 180;

            // if we're as good as standing up
            if ( modRotX <= standUpAcceptableError )
            {
                // Stop rolling, reset variables
                isRolling = false;
                standingUp = false;
                upOrDown = Vector3.zero;
                // Remove any extra forces
                potato.velocity = Vector3.zero;
                potato.angularVelocity = Vector3.zero;

                // Stop animation
                potatoAC.SetBool( "Is moving", false );

            }
            else if ( ( modRotX >= 180 - standUpRotateAngle ) || ( modRotX <= standUpRotateAngle ) || standingUp ) // We are close to being straight, make some small adjustments.
            {
                // Set to true so we can keep making the adjustments
                standingUp = true;

                float timeDiff = timeNow - lastRollTime;
                if ( timeDiff > stopSeconds ) // If we're gone past the desired stand up time, set the rotation explicity
                {
                    // Watch out for the order of the rotations here.
                    transform.rotation = Quaternion.identity * Quaternion.LookRotation( lastRollForward, upOrDown );

                }
                else // Lerp the rotation to make a smooth adjustment
                {
                    // First time making adjustments
                    if ( upOrDown.y == 0 )
                    {
                        if ( transform.up.y > 0 ) // If we're going to stand up the correct way
                        {
                            upOrDown = Vector3.up;
                        }
                        else // If we're going to be upside down
                        {
                            upOrDown = Vector3.down;
                        }
                    }
                    // Make the adjustment
                    transform.rotation = Quaternion.Slerp( lastRollQuat, Quaternion.identity * Quaternion.LookRotation( lastRollForward, upOrDown ), timeDiff / stopSeconds );
                }

            }
            else // We are not close to standing up straight, keep on rolling.
            {
                // Roll the potato
                potato.AddTorque( transform.right * lastV * speed );

                // Save these for later
                lastRollTime = timeNow;
                lastRollQuat = new Quaternion( transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w );
                lastRollForward = ( new Vector3( transform.forward.x, 0, transform.forward.z ) ).normalized;
                
            }
        }

        // if we've got some sideways input, and we're not currently rolling
        if ( v == 0 && h != 0 && ! isRolling )
        {
            // Inverse the turn we're upside down
            float direction = transform.up.y > 0 ? 1 : -1;
            // Turn the potato, so we can roll in a different direction
            transform.Rotate( 0, Time.deltaTime * h * turnSpeed * direction, 0 );
        }

        // jump
        if ( jump > 0 && ! jumping )
        {
            jumping = true;
            potato.AddForce( Vector3.up * jumpForce );
        }
        else
        {
            // Not jumping, add a bit of downwards pressure, so it's easier to climb slopes.
            potato.AddForce( Vector3.down * 10 );
        }

        // Should we show the idle animation? 
        if ( timeNow - lastInputTime > idleSeconds )
        {
            potatoAC.SetBool( "Is idle", true );
        }
        else
        {
            potatoAC.SetBool( "Is idle", false );
        }

    }

    void FixedUpdate()
    {
        // TODO: pos.y can be higher when the potato is upside down.
        // Stop the potato from falling through the floor.
        Vector3 pos = transform.position;
        float yAtPos = TerrainHelper.Instance.GetRealYAt( pos.x, pos.z );
        
        if ( pos.y < yAtPos - floorFallError ) 
        {
            transform.position = new Vector3( pos.x, yAtPos + 0.01f, pos.z );
            jumping = false;
        }
        else if ( pos.y < yAtPos + 0.05f )
        {
            jumping = false;
        }
    }

    public bool IsRolling()
    {
        return isRolling;
    }

    public void LookAt()
    {

    }

}
