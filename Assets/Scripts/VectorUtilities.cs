using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;


public class VectorUtilities
{
    public class ClockWiseComparer : IComparer<Vector3>
    {

        public ClockWiseComparer(Vector3 _origin)
        {
            m_origin =new Vector2(_origin.x,_origin.z);
        }

        Vector3 m_origin;

        /// <summary>
        /// Returnst 0 if same, 1 if first is before second else -1
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static int IsClockWise(Vector2 first, Vector2 second, Vector2 origin)
        {
            if (first == second)
                return 0;

            Vector2 offset1 = first - origin;
            Vector2 offset2 = second - origin;


            float PointsAngle(float x, float y)
            {
                float a = Mathf.Atan2(x, y);

                return a;
            }
            float angle1 = PointsAngle(offset1.x, offset1.y);
            float angle2 = PointsAngle(offset2.x, offset2.y);

           
            if (angle1 < angle2) return 1;
            if (angle1 > angle2) return -1;



            return (offset1.sqrMagnitude < offset2.sqrMagnitude) ? 1 : -1;
        }

        public int Compare(Vector3 v1, Vector3 v2)
        {

            return IsClockWise(new Vector2(v2.x, v2.z), new Vector2(v1.x, v1.z), m_origin);
        }


    }


 
    /// <summary>
    /// returns midpoint of vectorlist
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static Vector3 MidPoint(List<Vector3> points)
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;
        foreach (var v in points)
        {
            x += v.x;
            y += v.y;
            z += v.z;
        }
        x /= points.Count;
        y /= points.Count;
        z /= points.Count;
        return new Vector3(x, y, z);



    }

    public static Vector3 CrossingPoint(Vector3 A, Vector3 B,Vector3 C, Vector3 D)
    {
        if(LineLineIntersection(out Vector3 cross, A, B , C, D ))
        {
            return cross;
        }
       
        return MidPoint(new List<Vector3>() { A,C}) ;



    }
    /// <summary>
    /// https://forum.unity.com/threads/is-there-a-way-to-check-whether-two-vectors-intersect.69637/
    /// </summary>
    /// <param name="intersection"></param>
    /// <param name="linePoint1"></param>
    /// <param name="lineDirection1"></param>
    /// <param name="linePoint2"></param>
    /// <param name="lineDirection2"></param>
    /// <returns></returns>
    public static bool LineLineIntersection(out Vector3 intersection,
        Vector3 linePoint1, Vector3 lineDirection1,
        Vector3 linePoint2, Vector3 lineDirection2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineDirection1, lineDirection2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineDirection2);
        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineDirection1 * s);
           
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
    //Guess it works
    public static bool CheckInterSectionOnXZ(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        if (A.Equals(C) ||A.Equals(D) || B.Equals(C) || B.Equals(D)) return false;

        bool counterClowWise(Vector3 A, Vector3 B, Vector3 C)
        {
            return (C.z - A.z) * (B.x - A.x) > (B.z - A.z) * (C.x - A.x);

        }
        return counterClowWise(A, C, D) != counterClowWise(B, C, D) && counterClowWise(A, B, C) != counterClowWise(A, B, D);



    }

    public static Vector3 MidVector(Vector3 A, Vector3 B) { return B + ((A - B) / 2); }


}
