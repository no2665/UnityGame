using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootManager : MonoBehaviour {

    public GameObject root;
    public float initialRootRadius = 15;
    public int numberOfRoots = 6;

    public float baseRadius = 4;

    private Vector3 startPos = Vector3.zero;
    private GameObject rootContainer;

    private Hashtable roots = new Hashtable();

    // Use this for initialization
    void Start () {
        startPos = transform.position;
        rootContainer = new GameObject("Roots");
        rootContainer.transform.parent = transform;

        int startX = Mathf.RoundToInt(startPos.x);
        int startZ = Mathf.RoundToInt(startPos.z);

        // Create a root in the middle
        CreateRoot(new Vector3(startX, startPos.y, startZ), true);

        for ( int i = 0; i < numberOfRoots; i++)
        {
            float angle = (i * (360 / numberOfRoots)) * Mathf.Deg2Rad;

            int x = startX + Mathf.RoundToInt(initialRootRadius * Mathf.Sin(angle));
            int z = startZ + Mathf.RoundToInt(initialRootRadius * Mathf.Cos(angle)); 

            if ( x == startX )
            {
                int sign = (int)Mathf.Sign(z - startZ);
                for (int jz = startZ + sign; sign >= 0 ? jz < z : jz > z; jz += sign)
                {
                    CreateRoot(new Vector3(x, startPos.y, jz), true);
                }
            } else
            {
                // Use Bresenhams's algorithm
                float deltaX = x - startX;
                float deltaZ = z - startZ;
                float deltaErr = Mathf.Abs(deltaZ / deltaX);

                int sign = (int)Mathf.Sign(deltaX);
                int signZ = (int)Mathf.Sign(deltaZ);

                float error = 0;
                int nz = startZ + signZ;

                for (int jx = startX + sign; sign >= 0 ? jx < x : jx > x; jx += sign) 
                {
                    CreateRoot(new Vector3(jx, startPos.y, nz), true);

                    error = error + deltaErr;
                    if (error >= 0.5f)
                    {
                        nz = nz + (int)Mathf.Sign(deltaZ);
                        error = error - 1.0f;
                    }
                }
            }
        }
    }

    public void FixedUpdate()
    {
        // Debug, draw graph
        /*foreach (DictionaryEntry xRoots in roots)
        {
            foreach (DictionaryEntry de in (Hashtable)xRoots.Value)
            {
                Root r = (Root)de.Value;
                foreach ( Root n in r.GetNeighbours() )
                {
                    Debug.DrawLine(new Vector3(r.x, 2, r.z), new Vector3(n.x, 2, n.z), Color.red);
                }
            }
        }*/
    }

    public bool CheckForRoot(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);
        if ( roots.ContainsKey(x) )
        {
            Hashtable xRoots = (Hashtable)roots[x];
            if ( xRoots.ContainsKey(z) )
            {
                return ((Root) xRoots[z]).isEnabled();
            }
        }
        return false;
    }

    private bool CheckForCycles()
    {
        List<int> rootIDs = Root.GetIDs();

        List<bool> visited = new List<bool>(rootIDs.Count);
        for (int i = 0; i < rootIDs.Count; i++)
        {
            visited.Add(false);
        }

        foreach ( DictionaryEntry xRoots in roots )
        {
            foreach ( DictionaryEntry de in (Hashtable) xRoots.Value )
            {
                Root r = (Root)de.Value;
                int index = rootIDs.IndexOf(r.ID);
                if ( ! visited[ index ] )
                {
                    if ( CheckVertexCycle( index, visited, -1, r) )
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool CheckVertexCycle(int v, List<bool> visited, int parent, Root r)
    {
        List<int> rootIDs = Root.GetIDs();
        visited[v] = true;

        foreach ( Root next in r.GetNeighbours() )
        {
            int index = rootIDs.IndexOf(next.ID);
            if ( ! visited[ index ] )
            {
                if ( CheckVertexCycle(index, visited, v, next) )
                {
                    return true;
                }
            } else if ( index != parent )
            {
                return true;
            }
        }
        
        return false;
    }

    public bool CheckValidPosition(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);
        Root tempRoot = new Root(x, z);
        SetRootNeighbours(tempRoot, false);
        if ( tempRoot.GetNeighbours().Count == 0 )
        {
            tempRoot.Delete();
            return false;
        }

        if (!roots.ContainsKey(x))
        {
            roots[x] = new Hashtable();
        }
        ((Hashtable)roots[x])[z] = tempRoot;

        bool isCycle = CheckForCycles();

        tempRoot.Delete();
        ((Hashtable)roots[x]).Remove(z);

        return !isCycle;
    }

    public void CreateRoot(Vector3 pos)
    {
        CreateRoot(pos, false);
    }

    private void CreateRoot(Vector3 pos, bool oneNeighbour)
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);
        if ( ! CheckForRoot(pos) )
        {
            Root r = new Root(root, x, z, rootContainer.transform, pos.y);
            SetRootNeighbours(r, oneNeighbour);

            if (!roots.ContainsKey(x))
            {
                roots[x] = new Hashtable();
            }
            ((Hashtable)roots[x])[z] = r;
        }
    }

    private void SetRootNeighbours(Root r, bool oneNeighbour)
    {
        int x = r.GetX();
        int z = r.GetZ();

        List<Root> potentialNeighbours = new List<Root>();

        for (int ix = x - 1; ix <= x + 1; ix++ )
        {
            if (roots.ContainsKey(ix))
            {
                Hashtable xRoots = (Hashtable) roots[ix];
                for (int iz = z - 1; iz <= z + 1; iz++)
                {
                    if ( xRoots.ContainsKey(iz) )
                    {
                        potentialNeighbours.Add((Root) xRoots[iz]);
                    }
                }
            }
        }

        if ( potentialNeighbours.Count > 0)
        {
            if (oneNeighbour)
            {
                Root n = potentialNeighbours[0];
                r.AddNeighbour(n);
                n.AddNeighbour(r);
            }
            else
            {
                for (int i = 0; i < potentialNeighbours.Count; i++)
                {
                    Root n = potentialNeighbours[i];
                    List<int> checkedIDs = new List<int>();
                    checkedIDs.Add(n.ID);
                    CheckCloseNeighbours(n, potentialNeighbours, x, z, checkedIDs);
                    r.AddNeighbour(n);
                    n.AddNeighbour(r);
                }
            }
        }
    }

    private void CheckCloseNeighbours(Root r, List<Root> neighbours, int x, int z, List<int> ignoreID)
    {
        foreach (Root nn in r.GetNeighbours())
        {
            if (ignoreID.Contains(nn.ID)) continue;
            ignoreID.Add(nn.ID);
            if (nn.x >= x - 1 && nn.x <= x + 1 && nn.z >= z - 1 && nn.z <= z + 1)
            {
                neighbours.Remove(nn);
                CheckCloseNeighbours(nn, neighbours, x, z, ignoreID);
            }
        }
    }
}
