using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sorter : MonoBehaviour
{

    public List<Transform> transforms = new List<Transform>();
    public List<Vector3> points = new List<Vector3>();


    public Transform center;
    // Start is called before the first frame update
    void Start()
    {
        //PathGenerator.PathNode node 
    }

    // Update is called once per frame
    void Update()
    {
        points.Clear();
        foreach (Transform t in transforms)
        {
           points.Add(t.position);
        }
      
        points.Sort(new VectorUtilities.ClockWiseComparer(center.position));

    }
    private void OnDrawGizmos()
    {
        Color color= Color.white;

      
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.color = color;
            if (i+1 >= points.Count)
            {
               // Gizmos.DrawLine(points[i], points[0]);
            }
            else
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }
            color-= Color.white/points.Count;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(points[i], center.position);
        }
    }



}
