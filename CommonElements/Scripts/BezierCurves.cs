using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierCurves
{

    /* Public Functions - accessible outside of this script */
    /*
     * Determines the points necessary for a Linear Bezier Curve (aka a Straight Line)
     * - DOESN'T DRAW IT, ONLY DETERMINES POINTS
    */
    public static List<Vector3> DetermineLinearCurve(int numPoints, GameObject point0, GameObject point1, int startPosition = 0)
    {
        List<Vector3> positions = new List<Vector3>();
        for(int i = startPosition; i <= numPoints; i++)
        {
            float t = i / (float)numPoints;
            positions.Add(CalculateLinearBezierPoint(t, point0.transform.position, point1.transform.position));
        }
        return positions;
    }
    public static List<Vector3> DetermineLinearCurve(int numPoints, Vector3 point0, Vector3 point1, int startPosition = 0)
    {
        List<Vector3> positions = new List<Vector3>();
        for(int i = startPosition; i <= numPoints; i++)
        {
            float t = i / (float)numPoints;
            positions.Add(CalculateLinearBezierPoint(t, point0, point1));
        }
        return positions;
    }

    /*
     * Determines the points necessary for a Quadratic Bezier Curve (aka a curve generated w/ 3 reference points)
     * - DOESN'T DRAW IT, ONLY DETERMINES POINTS
    */
    public static List<Vector3> DetermineQuadraticCurve(int numPoints, GameObject point0, GameObject point1, GameObject point2, int startPosition = 0)
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = startPosition; i <= numPoints; i++)
        {
            float t = i / (float)numPoints;
            positions.Add(CalculateQuadraticBezierPoint(t, point0.transform.position, point1.transform.position, point2.transform.position));
        }
        return positions;
    }
    public static List<Vector3> DetermineQuadraticCurve(int numPoints, Vector3 point0, Vector3 point1, Vector3 point2, int startPosition = 0)
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = startPosition; i <= numPoints; i++)
        {
            float t = i / (float)numPoints;
            positions.Add(CalculateQuadraticBezierPoint(t, point0, point1, point2));
        }
        return positions;
    }

    /*
     * Draws the points based on the positions provided.
     * Highly recommended that the points are determined from either of the two public functions above.
    */
    public static void DrawCurve(LineRenderer renderer, List<Vector3> positions)
    {
        Vector3[] positionsArray = positions.ToArray();
        renderer.SetPositions(positionsArray);
    }

    /* ---------------------------------------------------------------------------------------------------- */
    /* ---------------------------------------------------------------------------------------------------- */
    /* Private functions - only used in this script, no need to have it used anywhere else */

    private static Vector3 CalculateLinearBezierPoint(float t, Vector3 p0, Vector3 p1)
    {
        return p0+t*(p1-p0);
    }

    private static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }
}
