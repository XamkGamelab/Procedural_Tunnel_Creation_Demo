using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

    [Header("Mesh generation")]
    [Tooltip("Points define shape tested to work only on square, should be in clockwise order")]
    public List<Vector3> points = new List<Vector3> { Vector3.left, Vector3.up, Vector3.up + Vector3.right, Vector3.right };
    [Tooltip("Depth of corridor between walls")]
    public float nodeDepth = 1;
    //public List<Vector3> nodeModelPoints = new List<Vector3> { Vector3.left, Vector3.up, Vector3.up + Vector3.right, Vector3.right };

    public Material material;

    [HideInInspector]
    public Mesh meshGlobal;
    [HideInInspector]
    public MeshFilter filter;
    [HideInInspector]
    public PathGenerator pathGenerator;




    [HideInInspector]
    public List<MeshCollider> colliders = new List<MeshCollider>();

    //[Range(1, 100)]
    //public int segmentsCountOnDistance = 1;
    [Space(10)]
    [Header("Canvas settings for somereason here")]
    public GameObject loadCanvas;

    public float startCanvasTime = 5f;
    private IEnumerator Start()
    {
        float startTime = Time.realtimeSinceStartup;
        yield return null;
        CreateMesh();
        float waitTime = startCanvasTime - startTime;
        if(waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            yield return null;
        }
       
        loadCanvas.SetActive(false);
    }


    private void OnDrawGizmosSelected()
    {
        List<(PathGenerator.PathNode, List<Vector3>)> nodePoints = new List<(PathGenerator.PathNode, List<Vector3>)>();


        foreach (var connection in pathGenerator.connections)
        {


            for (int j = 0; j < points.Count; j++)
            {

                Vector3 dir = connection.node1.position - connection.node2.position;

                Vector3 rotatedPos = connection.node1.position - (dir * nodeDepth) + Quaternion.LookRotation(dir) * points[j];
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(rotatedPos, 0.15f);

                dir = connection.node1.position - connection.node2.position;
                Vector3 rotatedPos2 = connection.node2.position + (dir * nodeDepth) + Quaternion.LookRotation(dir) * points[j];
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(rotatedPos2, 0.1f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(rotatedPos, rotatedPos2);
                float distanceToNext = 0;


                if (j + 1 == points.Count)
                {
                    distanceToNext = (points[j] - points[0]).magnitude;
                }
                else
                {
                    distanceToNext = (points[j] - points[0]).magnitude;
                }



             
            }







        }
    }
    public void CreateMesh()
    {
        if (pathGenerator == null) pathGenerator = PathGenerator.Instance;





        GeneratePathWays();
       CombinePathEndsInNodes();


        //update all meshes for rebuild
        foreach(var c in colliders)
        {
            c.sharedMesh = c.sharedMesh;
        }



    }

    private void CombinePathEndsInNodes()
    {

        foreach (var node in pathGenerator.pathNodes)
        {

            if (node.connections.Count == 2)
            {
                MergeEnds(node);
            }
            else if ( node.connections.Count == 1)
            {
                CreateEnd(node);

            }
            else if(node.connections.Count == 0){
                //do nothing
            }
            else
            {
                //sort connections to clockwise
                List<Vector3> vecs = new List<Vector3>();
                for (int i = 0; i < node.connections.Count; i++)
                {
                    if (node.connections[i].node1.Equals(node))
                    {
                        vecs.Add(node.connections[i].node2.position);

                    }
                    else
                    {

                        vecs.Add(node.connections[i].node1.position);
                    }
                }
                vecs.Sort(new VectorUtilities.ClockWiseComparer(node.position));
                //node.connections.Sort( new ConnectionSorter(node)); WHY IT DOESNT WORK????
                List < PathGenerator.NodeConnection > sortedList =  new List<PathGenerator.NodeConnection>();
                foreach (var v in vecs)
                {
                    foreach (var c in node.connections)
                    {
                        if (c.node1.position.Equals(v) || c.node2.position.Equals(v))
                        {
                            sortedList.Add(c);
                            break;

                        }
      
                    }
                }
                node.connections = sortedList;

                Vector3 height = Vector3.up;
                List<Color> colors = new List<Color>() { Color.black, Color.blue, Color.green, Color.red, Color.yellow, Color.black, Color.blue, Color.green, Color.red, Color.yellow, Color.black, Color.blue, Color.green, Color.red, Color.yellow };
             
                for (int i = 0; i < node.connections.Count; i++)
                {
                  
                    if (node.connections[i].node2.Equals(node))
                    {
                        Debug.DrawLine(node.position, node.connections[i].node1.position + height, colors[i], 1000f);
                    }
                    else
                    {
                        Debug.DrawLine(node.position, node.connections[i].node2.position + height, colors[i], 1000f);
                    }
                   
                    height += Vector3.up;
                  
                }
                height = Vector3.down;
                for (int i = 0; i < node.connections.Count; i++)
                {
                    Debug.DrawLine(node.position, vecs[i] + height, colors[i], 1000f);
                    height += Vector3.down;
                }


                    List<Vector3> floorVertices = new List<Vector3>();
                List<Vector3> ceilingVertices = new List<Vector3>();
                for (int i = 0; i < node.connections.Count; i++)
                {
                    //for each connection combine closes edge clockwise
                    int pointsOnLayer = 4;
                    var connection = node.connections[i];
                    var otherConnection = i == node.connections.Count - 1 ? node.connections[0] : node.connections[i + 1];

                    int startIndex = 0;
                    int goDiretion = 1;

                    Vector3 conDir = connection.node2.position - connection.node1.position;

                    if (connection.node2.Equals(node))
                    {
                        conDir = connection.node1.position - connection.node2.position;

                        startIndex = connection.connectionMesh.vertices.Length - 1;
                        goDiretion = -1;
                    }



                    //if starting from node
                    int startIndexOther = pointsOnLayer - 1;
                    int goDiretionOther = -1;
                    Vector3 conDirother = otherConnection.node2.position - otherConnection.node1.position;
                    //if this node is end 
                    if (otherConnection.node2.Equals(node))
                    {
                        conDirother = otherConnection.node1.position - otherConnection.node2.position;

                        startIndexOther = otherConnection.connectionMesh.vertices.Length - pointsOnLayer;
                        goDiretionOther = 1;
                    }


                    Vector3[] vertices = connection.connectionMesh.vertices;
                    Vector3[] chosenVertices = otherConnection.connectionMesh.vertices;
                    Debug.DrawLine(vertices[startIndex], chosenVertices[startIndexOther],Color.yellow, 1000f);

                    //get points from floor to ceiling
                    for (int loop = 0; loop < pointsOnLayer / 2; loop++)
                    {

                        int otherIndex = startIndexOther + loop * goDiretionOther;
                        int index = startIndex + loop * goDiretion;
                        //Vector3 midVector = VectorUtilities.MidVector(chosenVertices[otherIndex], vertices[index]);
                        Vector3 midVector = VectorUtilities.CrossingPoint(chosenVertices[otherIndex], conDirother, vertices[index], conDir);
                        Debug.DrawLine(midVector, midVector + Vector3.up, Color.blue, 1000f);
                        vertices[index] = midVector;
                        chosenVertices[otherIndex] = midVector;
                        if (loop == 0)
                        {
                            floorVertices.Add(midVector);
                        }
                        if (loop == (pointsOnLayer / 2) - 1)
                        {

                            ceilingVertices.Add(midVector);
                        }
                    }

                    otherConnection.connectionMesh.SetVertices(chosenVertices);
                    otherConnection.connectionMesh.RecalculateNormals();
                    connection.connectionMesh.SetVertices(vertices);
                    connection.connectionMesh.RecalculateNormals();
                }

                //create floors
                if (floorVertices.Count >= 3)
                {
                    GameObject pathObject = new GameObject("Corridor");
                    Mesh mesh = new Mesh();
                    pathObject.AddComponent<MeshFilter>().mesh = mesh;
                    pathObject.AddComponent<MeshRenderer>().sharedMaterial = material;
                    var coll = pathObject.AddComponent<MeshCollider>();
                    colliders.Add(coll);

                    List<Vector2> uvs = new List<Vector2>();

                    List<Vector3> f_verts = new List<Vector3>();
                    List<Vector3> c_verts = new List<Vector3>();
                    for (int i = 0; i < floorVertices.Count; i++)
                    {


                        Vector3 lineStart = transform.position;

                        f_verts.Add(floorVertices[i]);
                       
                        c_verts.Add(ceilingVertices[i]);


                    }
                    Vector3 floorMid = VectorUtilities.MidPoint(f_verts);
                    f_verts.Sort(new VectorUtilities.ClockWiseComparer(floorMid));
                    c_verts.Sort(new VectorUtilities.ClockWiseComparer(VectorUtilities.MidPoint(c_verts)));
                    //Debug.DrawLine(VectorUtilities.MidPoint(f_verts), VectorUtilities.MidPoint(c_verts), Color.red, 1000f);
                    List<int> trigs = new List<int>();

                    //Draws lines in order
                    Debug.DrawLine(f_verts[0], f_verts[1], Color.cyan, 100000f);
                    Debug.DrawLine(f_verts[0], f_verts[f_verts.Count - 1], Color.green, 1000f);

                    //set UV
                    float scaler = 0.1f; //HACK
                    for (int i = 0; i < floorVertices.Count; i++)
                    {
                        Vector3 v = floorMid - floorVertices[i];
                        v *= scaler;
                        uvs.Add(new Vector2(v.x,v.z));

                    }
                    for (int i = 0; i < floorVertices.Count; i++)
                    {
                        Vector3 v = floorMid - floorVertices[i];
                        v *= scaler;
                        uvs.Add(new Vector2(v.x, v.z));

                    }


                    for (int i = 1; i < floorVertices.Count - 1; i++)
                    {
                       
                        trigs.Add(0);
                        trigs.Add(i);
                        trigs.Add(i + 1);

                    }
                    for (int i = 1; i < floorVertices.Count - 1; i++)
                    {

                        trigs.Add(floorVertices.Count + i);
                        trigs.Add(floorVertices.Count);
                        trigs.Add(floorVertices.Count + i + 1);


                    }
                    List<Vector3> verts = new List<Vector3>();

                    verts.AddRange(f_verts);
                    verts.AddRange(c_verts);



                    mesh.SetVertices(verts);
                    mesh.SetTriangles(trigs, 0);
                    mesh.RecalculateNormals();
                    mesh.SetUVs(0, uvs);
                    coll.sharedMesh = mesh;
                    
                   
                }
            }


            //find if this is end or start







        }

    }
    /// <summary>
    /// Fills empty node end
    /// </summary>
    /// <param name="node"></param>
    private void CreateEnd(PathGenerator.PathNode node)
    {
        var connection = node.connections[0];

        int pointsOnLayer = 4;

        Vector3[] vertices = connection.connectionMesh.vertices;
        //if starting from node
        int startIndex = 0;
        int goDiretion = 1;
        if (connection.node2.Equals(node))
        {
            startIndex = connection.connectionMesh.vertices.Length - 1;
            goDiretion = -1;


        }
        List<Vector3> verts = new List<Vector3>();

        for (int i = 0; i < pointsOnLayer; i++)
        {
            int index = startIndex + i * goDiretion;
            verts.Add(vertices[index]);

        }
        GameObject pathObject = new GameObject("End");
        Mesh mesh = new Mesh();
        pathObject.AddComponent<MeshFilter>().mesh = mesh;
        pathObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        var coll = pathObject.AddComponent<MeshCollider>();
        colliders.Add(coll);

        List<Vector2> uvs = new List<Vector2>();

      
      
       
       
        List<int> trigs = new List<int>();

        Vector3 mid = VectorUtilities.MidPoint(verts);
        verts.Sort(new VectorUtilities.ClockWiseComparer(mid));
        //set UV
        float scaler = 0.1f; //HACK
        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 v = mid - vertices[i];
            v *= scaler;
            uvs.Add(new Vector2(v.x, v.y));

        }



       
        for (int i = 1; i < verts.Count - 1; i++)
        {

           
            trigs.Add(0);
            trigs.Add(i);
            trigs.Add( i + 1);


        }
       

        mesh.SetVertices(verts);
        mesh.SetTriangles(trigs, 0);
        mesh.RecalculateNormals();
        mesh.SetUVs(0, uvs);
        coll.sharedMesh = mesh;


    }
   

    

    public class ConnectionSorter : IComparer<PathGenerator.NodeConnection>
    {
        PathGenerator.PathNode node;
        public ConnectionSorter(PathGenerator.PathNode _node)
        {
            node = _node;

        }
        public ConnectionSorter(Vector3 o)
        {
            //node = _node;

        }
        public int Compare(PathGenerator.NodeConnection x, PathGenerator.NodeConnection y)
        {
            //compare to nodes connected to this

            //Vector3 first = x.node1.Equals(node) ? x.node2.position : x.node1.position;
            //Vector3 second = y.node1.Equals(node) ? y.node2.position : y.node1.position;

            Vector3 v1 = Vector3.zero;
            if (x.node1.Equals(this.node))
            {
                v1 = x.node2.position;
            }
            else if (x.node2.Equals(this.node)){
                v1 = x.node1.position;
            }
            else
            {
                Debug.LogError("Node not found" + node);
            }
            Vector3 v2 = Vector3.zero;
            if (y.node1.Equals(this.node))
            {
                v1 = y.node2.position;
            }
            else if (y.node2.Equals(this.node)){
                v1 = y.node1.position;
            }
            else
            {
                Debug.LogError("Node not found" + node);
            }
           
            
          
            return VectorUtilities.ClockWiseComparer.IsClockWise( new Vector2(v1.x, v1.z), new Vector2(v2.x, v2.z), node.position);

        }
    }
    /// <summary>
    /// merges node with its ends Must be 2 connections
    /// </summary>
    /// <param name="node"></param>
    private void MergeEnds(PathGenerator.PathNode node)
    {

        //for each connection combine closes edge clockwise
        var connection = node.connections[0];
        var otherConnection = node.connections[1];
        //vertices start from 0

        
       
        int pointsOnLayer = 4;
       

        //if starting from node
        int startIndex = 0;
        int goDiretion = 1;
        //if this node is end 
        Vector3 conDir = connection.node2.position - connection.node1.position;
       
        if (connection.node2.Equals(node))
        {
             conDir = connection.node1.position - connection.node2.position;

            startIndex = connection.connectionMesh.vertices.Length - 1;
            goDiretion = -1;
        }
      


        //if starting from node
        int startIndexOther = pointsOnLayer - 1; 
        int goDiretionOther = -1;
        Vector3 conDirother = otherConnection.node2.position - otherConnection.node1.position;
        //if this node is end 
        if (otherConnection.node2.Equals(node))
        {
            conDirother = otherConnection.node1.position - otherConnection.node2.position;

            startIndexOther = otherConnection.connectionMesh.vertices.Length - pointsOnLayer;
            goDiretionOther = 1;
        }

     

        Vector3[] vertices = connection.connectionMesh.vertices;
        Vector3[] chosenVertices = otherConnection.connectionMesh.vertices;
        for (int i = 0; i < pointsOnLayer; i++)
        {
            int index = startIndex + i * goDiretion;
            int otherIndex = startIndexOther + i * goDiretionOther;
            
            Vector3 midVector = VectorUtilities.CrossingPoint(chosenVertices[otherIndex],conDirother, vertices[index],conDir);

            Debug.DrawLine(midVector, midVector + Vector3.up, Color.green, 1000f);

            vertices[index] = midVector;
            chosenVertices[otherIndex] = midVector;
        }
        otherConnection.connectionMesh.SetVertices(chosenVertices);
        
        otherConnection.connectionMesh.RecalculateNormals();
       // otherConnection.connectionMesh.SetUVs(0, otherConnection.connectionMesh.uv);

        connection.connectionMesh.SetVertices(vertices);
        connection.connectionMesh.RecalculateNormals();
        //connection.connectionMesh.SetUVs(0, connection.connectionMesh.uv);

    }
    private void GeneratePathWays()
    {

        float layerPerimeter = 0;
        for (int i = 0; i < points.Count; i++)
        {
            if (i + 1 == points.Count)
            {
                layerPerimeter += (points[i] - points[0]).magnitude;
            }
            else
            {
                layerPerimeter += (points[i] - points[i + 1]).magnitude;
            }



        }

        foreach (var connection in pathGenerator.connections)
        {
            GameObject pathObject = new GameObject("Pathway");
            Mesh mesh = new Mesh();
            pathObject.AddComponent<MeshFilter>().mesh = mesh;
            pathObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            var coll = pathObject.AddComponent<MeshCollider>();
            colliders.Add(coll); 
            //coll.convex = true;


            Vector3 lineStart = transform.position;
            List<Vector3> verts = new List<Vector3>();
            List<int> trigs = new List<int>();
            List<Vector3> norms = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();


            int layerStart = 0;
            //for (int segment = 0; segment < segmentsCountOnDistance; segment++)
            //{

                List<Vector3> layerPoints = new List<Vector3>();


                //Create starting end
                for (int j = 0; j < points.Count; j++)
                {
                 

                    Vector3 dir = connection.node1.position - connection.node2.position;
                    Vector3 rotatedPos = connection.node1.position - (dir * nodeDepth) + Quaternion.LookRotation(dir) * points[j];
                    layerPoints.Add(rotatedPos);


                    //UV 
                    //float t = (float)segment / (float)segmentsCountOnDistance;
                    float distanceToNext = 0;
                    if (j + 1 == points.Count)
                    {
                        distanceToNext = (points[j] - points[0]).magnitude;
                    }
                    else
                    {
                        distanceToNext = (points[j] - points[0]).magnitude;
                    }
                    float uvValue = distanceToNext / layerPerimeter;
                    //uvs.Add(new Vector2(uvValue, t));
                    uvs.Add(new Vector2(j/ (float)(points.Count-1), 0f));

                   

                }

                verts.AddRange(layerPoints);
                layerPoints.Clear();

                //create other end
                for (int j = 0; j < points.Count; j++)
                {
                    

                    Vector3 dir = connection.node1.position - connection.node2.position;
                    //add little room for node
                    Vector3 rotatedPos = connection.node2.position + (dir * nodeDepth) + Quaternion.LookRotation(dir) * points[j];
                    layerPoints.Add(rotatedPos);


                    //UV
                    //float t = (float)segment / (float)segmentsCountOnDistance;
                    float distanceToNext = 0;
                    if (j + 1 == points.Count)
                    {
                        distanceToNext = (points[j] - points[0]).magnitude;
                    }
                    else
                    {
                        distanceToNext = (points[j] - points[0]).magnitude;
                    }
                    float uvValue = distanceToNext / layerPerimeter;
                    //uvs.Add(new Vector2(uvValue, t));
                   uvs.Add(new Vector2(j / (float)(points.Count - 1), 1f));


                }
                verts.AddRange(layerPoints);
                Vector3 norm = Vector3.back;


                int vertsOnLayer = points.Count;

                //create triangles
                for (int i = layerStart; i < layerStart + vertsOnLayer; i++)
                {

                    int indexInnerRoot = i;
                    int indexOuterRoot = i + vertsOnLayer;


                    int indexInnerNext = (indexInnerRoot + 1) % vertsOnLayer + layerStart;
                    indexInnerNext = indexInnerNext == 0 ? 0 : indexInnerNext;

                    int indexOuterNext = (indexOuterRoot + 1) % verts.Count;
                    indexOuterNext = indexOuterNext == 0 ? layerStart + vertsOnLayer : indexOuterNext;



                    trigs.Add(indexInnerRoot);
                    trigs.Add(indexOuterNext);
                    trigs.Add(indexInnerNext);



                    trigs.Add(indexInnerRoot);
                    trigs.Add(indexOuterRoot);
                    trigs.Add(indexOuterNext);





                }

               
                mesh.SetVertices(verts);
                mesh.SetTriangles(trigs, 0);
                mesh.RecalculateNormals();
                mesh.SetUVs(0, uvs);

                coll.sharedMesh = mesh;
                connection.connectionMesh = mesh;
            }
       // }


    }


}
