using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Holds all the roots. Can detect cycle in the root graph to prevent root blocking.
 */
public class RootManager : MonoBehaviour {

    public GameObject root;
    public float initialRootRadius = 15;
    public int numberOfRoots = 6;

    public float baseRadius = 4;

    private Vector3 startPos = Vector3.zero;
    private GameObject rootContainer;

    private Hashtable roots = new Hashtable();
    
    void Start () {
        startPos = transform.position;
        rootContainer = new GameObject("Roots");
        rootContainer.transform.parent = transform;

        int startX = Mathf.RoundToInt(startPos.x);
        int startZ = Mathf.RoundToInt(startPos.z);

        // Create a root in the middle
        CreateRoot( new Vector3( startX, startPos.y, startZ ), true );

        // Create the rest of the initial roots
        // Should look like spokes of a wheel
        for ( int i = 0; i < numberOfRoots; i++ )
        {
            float angle = ( i * ( 360 / numberOfRoots ) ) * Mathf.Deg2Rad;

            // Get the x, z of where the spoke meets the edge of the circle
            int x = startX + Mathf.RoundToInt( initialRootRadius * Mathf.Sin(angle) );
            int z = startZ + Mathf.RoundToInt( initialRootRadius * Mathf.Cos(angle) );
            
            if ( x == startX )
            { // Vertical line. Can't use Bresenhams's algorithm
                int sign = (int) Mathf.Sign( z - startZ );
                // Offset by sign so to not place the centre root
                for ( int jz = startZ + sign; sign >= 0 ? jz < z : jz > z; jz += sign )
                {
                    CreateRoot( new Vector3( x, startPos.y, jz ), true );
                }
            }
            else
            {
                // Use Bresenhams's algorithm
                float deltaX = x - startX;
                float deltaZ = z - startZ;
                float deltaErr = Mathf.Abs( deltaZ / deltaX );

                int sign = (int) Mathf.Sign(deltaX);
                int signZ = (int) Mathf.Sign(deltaZ);

                float error = 0;
                int nz = startZ + signZ;

                for ( int jx = startX + sign; sign >= 0 ? jx < x : jx > x; jx += sign ) 
                {
                    CreateRoot( new Vector3( jx, startPos.y, nz ), true );

                    error = error + deltaErr;
                    if ( error >= 0.5f )
                    {
                        nz = nz + (int) Mathf.Sign(deltaZ);
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

    /*
     * Check for a root at pos
     */
    public bool CheckForRoot( Vector3 pos )
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);
        string name = Root.GetRootName( x, z );
        if ( roots.ContainsKey(name) )
        {
            return ( (Root) roots[name] ).IsEnabled();
        }
        return false;
    }

    /*
     * Check for cycles in the graph
     */
    private bool CheckForCycles()
    {
        List<int> rootIDs = Root.GetIDs();

        // Create a visited array
        // The index of this array will match up with the indexes of the rootIDs array
        List<bool> visited = new List<bool>(rootIDs.Count);
        for ( int i = 0; i < rootIDs.Count; i++ )
        {
            visited.Add(false);
        }

        // Look at each root
        foreach ( Root r in roots.Values )
        {
            // Get it's index in the visited array from it's ID
            int index = rootIDs.IndexOf(r.ID);
            if ( ! visited[index] )
            {   // Recurse if not visited
                if ( CheckVertexCycle( index, visited, -1, r ) )
                {
                    return true;
                }
            }
        }

        return false;
    }

    /*
     * Recursive method for CheckForCycles method
     */
    private bool CheckVertexCycle( int v, List<bool> visited, int parent, Root r )
    {
        List<int> rootIDs = Root.GetIDs();
        // Mark visited
        visited[v] = true;

        // Look at every root connected to this one
        foreach ( Root next in r.GetNeighbours() )
        {
            int index = rootIDs.IndexOf(next.ID);
            if ( ! visited[index] )
            {   // Recurse if not visited
                if ( CheckVertexCycle( index, visited, v, next ) )
                {
                    return true;
                }
            }
            else if ( index != parent )
            {   // If visited and it's not the parent, then we must have cycled around
                return true;
            }
        }
        
        return false;
    }

    /*
     * Checks if a root can be placed at pos.
     * The root must not block any paths stopping the enemies getting
     * to the base, so we will check for cycles
     */
    public bool CheckValidPosition( Vector3 pos )
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);
        string name = Root.GetRootName( x, z );
        // Create a dummy root at pos
        Root tempRoot = new Root( x, z );
        // Add the root to the graph
        SetRootNeighbours( tempRoot, false );
        // The root must be connected to another root
        if ( tempRoot.GetNeighbours().Count == 0 )
        { // Not valid
            tempRoot.Delete();
            return false;
        }
        
        roots[name] = tempRoot;

        // Do the check
        bool isCycle = CheckForCycles();

        // Tidy up
        tempRoot.Delete();
        roots.Remove(name);

        return !isCycle;
    }

    public void CreateRoot( Vector3 pos )
    {
        CreateRoot( pos, false );
    }

    // Creates a root at pos
    private void CreateRoot( Vector3 pos, bool oneNeighbour )
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);
        string name = Root.GetRootName( x, z );
        // Make sure there isn't an existing root
        if ( ! CheckForRoot(pos) )
        {
            Root r = new Root( root, x, z, rootContainer.transform, pos.y );
            SetRootNeighbours( r, oneNeighbour );

            roots[name] = r;
        }
    }

    /*
     * Set the neighbours for the root.
     * @param oneNeighbour: Connects the root to only one of it's neighbours. Less 
     * chance of creating cycles, when creating roots without checking first
     */
    private void SetRootNeighbours( Root r, bool oneNeighbour )
    {
        int x = r.GetX();
        int z = r.GetZ();

        // Find the roots within one unit of Root r.
        List<Root> potentialNeighbours = new List<Root>();
        for ( int ix = x - 1; ix <= x + 1; ix++ )
        {
            for ( int iz = z - 1; iz <= z + 1; iz++ )
            {
                string name = Root.GetRootName( ix, iz );
                if ( roots.ContainsKey(name) )
                {
                    potentialNeighbours.Add( (Root) roots[name] );
                }
            }
        }
        
        if ( potentialNeighbours.Count > 0)
        { // We are near another root
            if ( oneNeighbour )
            { // Just connect to one neighbour
                Root n = potentialNeighbours[0];
                r.AddNeighbour(n);
                n.AddNeighbour(r);
            }
            else
            {
                // We don't want to connect to every neighbour, as that
                // could create short cycles straight away.
                // Instead we are checking for groups of connected neighbours,
                // and picking one from each group.
                for ( int i = 0; i < potentialNeighbours.Count; i++ )
                {
                    Root n = potentialNeighbours[i];
                    // Marks this root as checked.
                    List<int> checkedIDs = new List<int>();
                    checkedIDs.Add(n.ID);
                    CheckCloseNeighbours( n, potentialNeighbours, x, z, checkedIDs );
                    // Add each other as a neighbour
                    r.AddNeighbour(n);
                    n.AddNeighbour(r);
                }
            }
        }
    }

    /*
     * Recursive function that prevents short cycles when setting neighbours
     */
    private void CheckCloseNeighbours( Root r, List<Root> neighbours, int x, int z, List<int> ignoreID )
    {
        foreach ( Root nn in r.GetNeighbours() )
        {
            // Continue if the root has already been checked, so we don't end up in an infinite loop
            if ( ignoreID.Contains(nn.ID) ) continue;
            // Mark as checked
            ignoreID.Add(nn.ID);
            // We only check roots within one unit of the root being added
            if ( nn.x >= x - 1 && nn.x <= x + 1 && nn.z >= z - 1 && nn.z <= z + 1 )
            {
                // neighbours are removed from the potentialNeighbours array, so will not be added
                // as a neighbour.
                neighbours.Remove(nn);
                CheckCloseNeighbours( nn, neighbours, x, z, ignoreID );
            }
        }
    }
}
