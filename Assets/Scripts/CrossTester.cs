using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossTester : MonoBehaviour
{
    public Transform A;
    public Transform B;
    public Transform C;
    public Transform D;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(A.position , B.position);
        Gizmos.DrawLine(C.position, D.position);
        if((VectorUtilities.LineLineIntersection(out Vector3 cross, A.position, B.position- A.position, C.position, D.position-B.position))){
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(cross,0.5f);
        }
        else
        {
            Gizmos.DrawSphere(A.position, 0.5f);
        }
       
    }
}
