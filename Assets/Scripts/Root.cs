using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root {

    public int ID;
    public int x, z;

    private GameObject root;
    private List<Root> neighbours;

    private static int lastID = -1;
    private static List<int> ids = new List<int>();

    private float offsetX = 0.5f;
    private float offsetZ = 0.5f;

    public Root(int xPos, int zPos)
    {
        x = xPos;
        z = zPos;

        neighbours = new List<Root>();
        ID = SetID();
    }

    public Root(GameObject r, int xPos, int zPos, Transform parent, float yPos = 0)
    {
        x = xPos;
        z = zPos;

        root = MonoBehaviour.Instantiate(r, new Vector3(x + offsetX, yPos, z + offsetZ), Quaternion.AngleAxis(90, Vector3.right), parent);

        neighbours = new List<Root>();
        ID = SetID();
    }

    public void Delete()
    {
        foreach ( Root r in neighbours )
        {
            r.GetNeighbours().Remove(this);
        }
        neighbours = null;
        ids.Remove(ID);
        ID = -1;
        if (root != null)
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

    public void AddNeighbour(Root root)
    {
        neighbours.Add(root);
    }

    public bool isEnabled()
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
