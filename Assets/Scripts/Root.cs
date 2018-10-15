using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root {

    public int ID;
    public int x;
    public int z;

    private GameObject root;
    private List<Root> neighbours;

    private static int lastID = -1;
    private static List<int> ids = new List<int>();

    public Root(GameObject r, int xPos, int zPos)
    {
        root = r;
        x = xPos;
        z = zPos;

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
        MonoBehaviour.Destroy(root);
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
