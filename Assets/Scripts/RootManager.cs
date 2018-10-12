using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootManager : MonoBehaviour {

    public GameObject root;
    public float initialRootRadius = 15;
    public int numberOfRoots = 6;

    private Vector3 startPos = Vector3.zero;
    private GameObject rootContainer;

    private float offsetX = 0.5f;
    private float offsetZ = 0.5f;

	// Use this for initialization
	void Start () {
        startPos = transform.position;
        rootContainer = new GameObject("Roots");
        rootContainer.transform.parent = transform;

        int startX = Mathf.RoundToInt(startPos.x);
        int startZ = Mathf.RoundToInt(startPos.z);

        for ( int i = 0; i < numberOfRoots; i++)
        {
            float angle = (i * (360 / numberOfRoots)) * Mathf.Deg2Rad;

            int x = startX + Mathf.RoundToInt(initialRootRadius * Mathf.Sin(angle));
            int z = startZ + Mathf.RoundToInt(initialRootRadius * Mathf.Cos(angle));

            if ( x == startX )
            {
                int sign = (int)Mathf.Sign(z - startZ);
                for (int jz = z; sign >= 0 ? jz > startZ : jz < startZ; jz -= sign)
                {
                    Vector3 position = new Vector3(x + offsetX, startPos.y, jz + offsetZ);
                    Instantiate(root, position, Quaternion.AngleAxis(90, Vector3.right), rootContainer.transform);
                }
            } else
            {
                // Use Bresenhams's algorithm
                float deltaX = x - startX;
                float deltaZ = z - startZ;
                float deltaErr = Mathf.Abs(deltaZ / deltaX);

                int sign = (int)Mathf.Sign(deltaX);

                float error = 0;
                int nz = z;

                for (int jx = x; sign >= 0 ? jx > startX : jx < startX; jx -= sign) 
                {
                    Vector3 position = new Vector3(jx + offsetX, startPos.y, nz + offsetZ);
                    Instantiate(root, position, Quaternion.AngleAxis(90, Vector3.right), rootContainer.transform);

                    error = error + deltaErr;
                    if (error >= 0.5f)
                    {
                        nz = nz - (int)Mathf.Sign(deltaZ);
                        error = error - 1.0f;
                    }
                }
            }

        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
