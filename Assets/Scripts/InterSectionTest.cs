using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterSectionTest : MonoBehaviour
{

    public Transform A;
    public Transform B;
    public Transform C;
    public Transform D;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (VectorUtilities.CheckInterSectionOnXZ(A.position, B.position, C.position, D.position)) Gizmos.color = Color.red;
        Gizmos.DrawLine(A.position, B.position);
        Gizmos.DrawLine(C.position, D.position);
    }
}
