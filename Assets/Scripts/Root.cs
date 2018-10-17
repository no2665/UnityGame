using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Creates a root, and keeps a record of it's neighbours so a graph of roots can be made.
 * Doesn't inherit from MonoBehaviour because it isn't attached to a GameObject,
 * and we are not allowed to 'new' MonoBehaviours.
 */
public class Root {
    
    public int x, z;
    // Every root should have a unique ID
    public int ID;

    private static int lastID = -1;
    private static List<int> ids = new List<int>();

    private GameObject root;
    private List<Root> neighbours;

    // Offsets to centre the root model
    private readonly float offsetX = 0.5f;
    private readonly float offsetZ = 0.5f;

    // Basic constructor to create a temporary root
    public Root( int xPos, int zPos )
    {
        x = xPos;
        z = zPos;

        neighbours = new List<Root>();
        ID = SetID();
    }

    // Longer constructor for a permanent root
    public Root( GameObject r, int xPos, int zPos, Transform parent, float yPos = 0 )
    {
        x = xPos;
        z = zPos;

        // Create the root here
        root = MonoBehaviour.Instantiate( r, new Vector3( x + offsetX, yPos, z + offsetZ ), Quaternion.AngleAxis( 90, Vector3.right ), parent );

        neighbours = new List<Root>();
        ID = SetID();
    }

    // Delete the root. Detach from the graph
    public void Delete()
    {
        foreach ( Root r in neighbours )
        {
            r.GetNeighbours().Remove(this);
        }
        neighbours = null;
        ids.Remove(ID);
        ID = -1;
        if ( root != null )
        {
            MonoBehaviour.Destroy(root);
        }
    }
    
    public List<Root> GetNeighbours()
    {
        return neighbours;
    }

    public int GetX()
    {
        return x;
    }

    public int GetZ()
    {
        return z;
    }

    public void AddNeighbour( Root root )
    {
        neighbours.Add(root);
    }

    // Stub method. May be needed later for pooling.
    public bool IsEnabled()
    {
        return true;
    }

    public static int SetID()
    {
        int i = ++lastID;
        ids.Add(i);
        return i;
    }

    public static List<int> GetIDs()
    {
        return ids;
    }
    
}
