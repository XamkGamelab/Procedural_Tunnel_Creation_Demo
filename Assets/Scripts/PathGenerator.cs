
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static PathGenerator;

public class PathGenerator : MonoBehaviour
{
    [Header("Map generation settings")]

    public Vector2 mapSize;

    [Tooltip("Density of node grid")]
    public float nodeDensity = 1.5f;

    [Tooltip("Treshold that perlin noise has to be over that node spawns")]
    [Range(0f, 1f)]
    public float nodeTreshold = 0.1f;

    [Tooltip("Angles lower than this are removed")]
    public float minAngle = 0.1f;

    [Tooltip("How many connections are map tries to generate for each node")]
    public int nodeConnections;
    [Tooltip("Multiplies node density with this and ignores shorter than that lines")]
    public float shortestDistanceModifier = 2f;

    [Header("Doesnt work as expected")]
    [Tooltip("Tries to limit connections for node, doenst work as such for some reason")]
    public int maxConnections = 4;


   
    public static PathGenerator Instance { get; private set; }

    public List<PathNode> pathNodes = new List<PathNode>();

    public List<NodeConnection> connections = new List<NodeConnection>();














    [Header("Game generations")]
    [Tooltip("Min distance to heart, if too large might not find spawn points")]
    public float distanceToHeart = 100f;

    public GameObject playerObject;

    public GameObject heartObject;



    private Vector3 heartLocation = Vector3.zero;

    private Vector3 playerLocation = Vector3.zero;

   

    private Color[] colors;
    public class PathNode
    {
        public List<NodeConnection> connections = new List<NodeConnection>();

        public Vector3 position;

     

        public PathNode( Vector3 pos)
        {
            
            position = pos;
        }
    }
    
    public class NodeConnection
    {
       public PathNode node1;
        public PathNode node2;

        public Mesh connectionMesh;

        public int segmentId = -1;
        public NodeConnection(PathNode n1, PathNode n2)
        {

            node1 = n1;
            node2 = n2;
        }

        public bool Same(NodeConnection other)
        {
            if(other.node1.Equals(node1) && other.node2.Equals(node2)){
                return true;
            }
            else if(other.node2.Equals(node1) && other.node1.Equals(node2))
            {
                return true;
            }


            return false;
        }
    }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
         
        

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

#if !UNITY_EDITOR
        GenerateMap();
        AddHeartAndPlayer();
#endif
    }


    private void OnDrawGizmos()
    {
        if(colors == null)
        {
            colors = new Color[256];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            }
        }
        
        if(heartLocation != Vector3.zero)
        {
           
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(heartLocation, Vector3.one * 5);

        }
        if (playerLocation != Vector3.zero)
        {
          
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(playerLocation, Vector3.one * 5);

        }
        if (playerLocation != Vector3.zero && heartLocation != Vector3.zero)
        {

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(heartLocation, playerLocation);
        }

            foreach (var node in pathNodes)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(node.position, 0.1f);
           
        }
        foreach (var c in connections)
        {
            if(c.segmentId >= 0)
            {
                Gizmos.color = colors[c.segmentId];
            }
            
            Gizmos.DrawLine(c.node1.position, c.node2.position );

        }
    }

    public void GenerateMap()
    {
        float randomSeed = Random.Range(0, 1000000);
        pathNodes.Clear();
        connections.Clear();
        if (nodeDensity > 0)
        {

            //create nodes
            for (float x = 0; mapSize.x > x; x += nodeDensity)
            {
                for (float z = 0; mapSize.y > z; z += nodeDensity)
                {
                    float sampleValue = Mathf.PerlinNoise(x + randomSeed, z + randomSeed);

                    if (sampleValue > nodeTreshold)
                    {
                        pathNodes.Add(new PathNode(new Vector3(x, 0, z)));

                    }


                }
            }

            foreach (PathNode node in pathNodes)
            {
                //find closest ones


               
                List<(PathNode, float)> closeNodes = new List<(PathNode, float)>();

                foreach (PathNode checkedNode in pathNodes)
                {

                    if (checkedNode != node)
                    {
                        if(node.connections.Count >= maxConnections)
                        {
                            break;
                        }
                        //if no close nodes
                        if (closeNodes.Count < nodeConnections)
                        {
                            closeNodes.Add((checkedNode, Vector3.Distance(node.position, checkedNode.position)));



                        }
                        else
                        {

                            //check for each close one
                            for (int i = 0; i < closeNodes.Count; i++)
                            {
                                float dist = Vector3.Distance(node.position, checkedNode.position);
                                if(dist < nodeDensity * shortestDistanceModifier)
                                {
                                    
                                }
                                else if (dist < closeNodes[i].Item2)
                                {
                                    //find longest distance
                                    int furthestIndex = 0;
                                    for (int j = 0; j < closeNodes.Count; j++)
                                    {
                                        if (closeNodes[j].Item2 > closeNodes[furthestIndex].Item2)
                                        {
                                            furthestIndex = j;

                                        }

                                    }
                                    closeNodes[furthestIndex] = (checkedNode, dist);
                                    break;
                                }
                            }
                        }

                    }

                }
                //create connections and asign them for max connection limit
                foreach (var n in closeNodes)
                {
                    var newCon = new NodeConnection(n.Item1, node);
                    if (connections.Any(x => x.Same(newCon))){

                    }
                    else
                    {
                        NodeConnection connection = new NodeConnection(n.Item1, node);
                        connections.Add(connection);
                        node.connections.Add(connection);
                        n.Item1.connections.Add(connection);
                    }
                  
                }


            }
          
            List<int> removeIndexes = new List<int>();


            
            //check angles between
            foreach (var n in pathNodes)
            {
                //for each connecition in n
                for (int i = 0; i < n.connections.Count; i++)
                {
                    var connection = n.connections[i];
                    
                    //find angle between connections 
                    foreach(var connectionComparison in n.connections)
                    {
                        if (connection.Same(connectionComparison))
                        {

                        }
                        else if (removeIndexes.Contains(connections.IndexOf(connectionComparison)) || removeIndexes.Contains(connections.IndexOf(connection)))
                        {


                        }
                        else
                        {
                            
                            Vector3 A = Vector3.zero;
                            Vector3 B = Vector3.zero;


                            if (connection.node1 == n)
                            {
                                A = connection.node2.position - connection.node1.position ;
                            }
                            else
                            {
                                A =  connection.node1.position - connection.node2.position;
                            }

                            if (connectionComparison.node1 == n)
                            {
                                B = connectionComparison.node2.position - connectionComparison.node1.position;
                            }
                            else
                            {
                               B = connectionComparison.node1.position - connectionComparison.node2.position;
                            }

                           
                            if(Vector3.Angle(A,B) < minAngle)
                            {
                             
                                removeIndexes.Add(connections.IndexOf(connection));
                                

                            }

                        }

                    }


                }

            }



            //remove crossing ones
            for (int i = 0; i < connections.Count; i++)
            {
                for (int j = 0; j < connections.Count; j++)
                {



                    if (i == j)
                    {



                    }
                    else
                    {
                        var connection = connections[i];
                        var checkedConnection = connections[j];
                        if (VectorUtilities.CheckInterSectionOnXZ(connection.node1.position
                            , connection.node2.position
                            , checkedConnection.node1.position
                            , checkedConnection.node2.position))
                        {
                            removeIndexes.Add(i);
                        }


                    }


                }




            }

            foreach (int r in removeIndexes)
            {

                connections[r] = null;

            }
            connections.RemoveAll(x => x == null);

            //clear nodeConnections for reasgiment
            foreach (var n in pathNodes)
            {
                n.connections.Clear();

            }





            foreach (NodeConnection con in connections)
            {
                foreach (PathNode node in pathNodes)
                {
                    if (con.node1.Equals(node) || con.node2.Equals(node))
                    {
                        node.connections.Add(con);
                    }


                }
            }
        }
        else
        {
            Debug.LogWarning("Node density should be over 0");
        }
    }


   private void AddHeartAndPlayer()
    {
      
        int islandIndex = 0;
        //THIS COULD BE OPTIMZED A LOT

        //tag connected with id 
        foreach (var connection in connections)
        {
            if (connection.segmentId == -1)
            {
                connection.segmentId = islandIndex;
                FindConnected(connection.node1, islandIndex);
                FindConnected(connection.node2, islandIndex);
                islandIndex++;
            }
        }
        //add island to lists
        
        List<List<NodeConnection>> connectionIslands = new List<List<NodeConnection>>(new List<NodeConnection>[islandIndex]);
        foreach (var connection in connections)
        {
            if (connection.segmentId == -1)
            {
                Debug.LogWarning(connection + "id -1");
            }
                if (connectionIslands[connection.segmentId] == null)
            {
                connectionIslands[connection.segmentId] = new List<NodeConnection>();
            }
            connectionIslands[connection.segmentId].Add(connection);
        }

        //find largest island
        List<NodeConnection> largestIsland = null;
        foreach (var island in connectionIslands)
        {
            if(largestIsland == null)
            {
                largestIsland = island;

            }
            else
            {

                if (island.Count > largestIsland.Count) largestIsland = island;

            }
        }

        //add heart
        var hConnectin = largestIsland[Random.Range(0, largestIsland.Count)];
        if(hConnectin.node1.connections.Count > hConnectin.node2.connections.Count)
        {

            heartLocation = hConnectin.node1.position;

        }
        else
        {
            heartLocation = hConnectin.node2.position;
        }
       
        heartObject.transform.position = heartLocation;


        //add player
        int maxIterations = 1000;
        int i = 0;
        while(i != maxIterations)
        {

            var randomConnnectio = largestIsland[Random.Range(0, largestIsland.Count)];

            //NOT LIKE THIS
            if(Vector3.Distance(randomConnnectio.node1.position, heartLocation ) > distanceToHeart)
            {
                if (randomConnnectio.node1.connections.Count > 1)
                {
                    playerLocation = randomConnnectio.node1.position;
                    break;
                }
               

            }
            else if(Vector3.Distance(randomConnnectio.node2.position, heartLocation) > distanceToHeart)
            {
                if (randomConnnectio.node2.connections.Count > 1)
                {
                    playerLocation = randomConnnectio.node2.position;
                    break;
                }
            }


            i++;
        }
        if(i == maxIterations)
        {
            Debug.LogWarning("Did not found farenough spawn");
            playerLocation = largestIsland[Random.Range(0, largestIsland.Count)].node1.position;
        }

        playerObject.transform.position = playerLocation;


        

    }
    /// <summary>
    /// Finds all connected to node
    /// </summary>
    /// <param name="node"></param>
    /// <param name="islandIndex"></param>
    private void FindConnected(PathNode node, int islandIndex)
    {


        foreach (var connection in node.connections)
        {
            if (connection.segmentId == -1)
            {
                connection.segmentId = islandIndex;
                FindConnected(connection.node1, islandIndex);
                FindConnected(connection.node2, islandIndex);

            }

        }


    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        GenerateMap();
        AddHeartAndPlayer();
    }
#endif
}
