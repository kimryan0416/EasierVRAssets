using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class CommonFunctions : MonoBehaviour
{
    // idsToIgnore = gameobject IDs
    public static List<Out> GetInRange<Out, Check>(Transform origin, float rad, int idToIgnore) {
        if (origin == null) return null;
        Dictionary<Out, float> possible = new Dictionary<Out, float>();
        Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
        foreach (Collider c in hitColliders) {
            if (c.GetComponent<Check>() != null && idToIgnore!=c.gameObject.GetInstanceID()) {
                possible.Add(c.GetComponent<Out>(), Vector3.Distance(origin.position, c.transform.position));
            }
        }
        List<Out> inRange = possible.OrderBy(x => x.Value).Select(pair => pair.Key).ToList();
        
        //inRange = inRange.OrderBy(x => Vector3.Distance(origin.position, x.transform.position)).ToList();
        return inRange;
    }

    public static List<Out> GetInRange<Out, Check, Exclude>(Transform origin, float rad) {
        if (origin == null) return null;
        Dictionary<Out, float> possible = new Dictionary<Out, float>();
        Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
        foreach (Collider c in hitColliders) {
            if (c.GetComponent<Check>() != null && c.GetComponent<Exclude>() == null) {
                //inRange.Add(c.GetComponent<Out>());
                possible.Add(c.GetComponent<Out>(), Vector3.Distance(origin.position, c.transform.position));
            }
        }
        List<Out> inRange = possible.OrderBy(x => x.Value).Select(pair => pair.Key).ToList();
        
        //inRange = inRange.OrderBy(x => Vector3.Distance(origin.position, x.transform.position)).ToList();
        return inRange;
    }
    public static List<Out> GetInRange<Out, Check, Exclude>(Transform origin, float rad, int layerToAvoid) {
        if (origin == null) return null;
        Dictionary<Out, float> possible = new Dictionary<Out, float>();
        Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
        foreach (Collider c in hitColliders) {
            if (c.GetComponent<Check>() != null && c.GetComponent<Exclude>() == null && layerToAvoid != c.gameObject.layer) {
                //inRange.Add(c.GetComponent<Out>());
                possible.Add(c.GetComponent<Out>(), Vector3.Distance(origin.position, c.transform.position));
            }
        }
        List<Out> inRange = possible.OrderBy(x => x.Value).Select(pair => pair.Key).ToList();
        
        //inRange = inRange.OrderBy(x => Vector3.Distance(origin.position, x.transform.position)).ToList();
        return inRange;
    }
    public static List<Out> GetInRange<Out, Check, Exclude>(Transform origin, float rad, List<int> layersToAvoid) {
        if (origin == null) return null;
        Dictionary<Out, float> possible = new Dictionary<Out, float>();
        Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
        foreach (Collider c in hitColliders) {
            if (c.GetComponent<Check>() != null && c.GetComponent<Exclude>() == null && !layersToAvoid.Contains(c.gameObject.layer)) {
                //inRange.Add(c.GetComponent<Out>());
                possible.Add(c.GetComponent<Out>(), Vector3.Distance(origin.position, c.transform.position));
            }
        }
        List<Out> inRange = possible.OrderBy(x => x.Value).Select(pair => pair.Key).ToList();
        
        //inRange = inRange.OrderBy(x => Vector3.Distance(origin.position, x.transform.position)).ToList();
        return inRange;
    }

    public static float GetAngleFromVector2(Vector2 original, OVRInput.Controller source = OVRInput.Controller.None) {
        // Derive angle from y and x
        float angle = Mathf.Atan2(original.y, original.x) * Mathf.Rad2Deg + 180f;
        // We need to do some offsettting becuase for some inane reason the thumbsticks have a ~5-degree offset
        switch(source) {
            case(OVRInput.Controller.LTouch):
                // need to add 5 degrees
                angle += 5f;
                break;
            case(OVRInput.Controller.RTouch):
                // Need to subtract 5 degrees
                angle -= 5f;
                break;
        }
        // We need to recenter the angle so that it's between 0 and 360, not 5 and 365
        angle = (angle > 360f) ? angle - 360 : angle;
        // Return
        return angle;
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        //Get a direction from the pivot to the point
        Vector3 dir = point - pivot;
        //Rotate vector around pivot
        dir = rotation * dir; 
        //Calc the rotated vector
        point = dir + pivot; 
        //Return calculated vector
        return point; 
    }

    //arrayToCurve is original Vector3 array, smoothness is the number of interpolations. 
    public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve,float smoothness){
        List<Vector3> points;
        List<Vector3> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;
         
        if(smoothness < 1.0f) smoothness = 1.0f;
         
        pointsLength = arrayToCurve.Length;
         
        curvedLength = (pointsLength*Mathf.RoundToInt(smoothness))-1;
        curvedPoints = new List<Vector3>(curvedLength);
         
        float t = 0.0f;
        for(int pointInTimeOnCurve = 0;pointInTimeOnCurve < curvedLength+1;pointInTimeOnCurve++){
            t = Mathf.InverseLerp(0,curvedLength,pointInTimeOnCurve);
             
            points = new List<Vector3>(arrayToCurve);
             
            for(int j = pointsLength-1; j > 0; j--){
                for (int i = 0; i < j; i++){
                    points[i] = (1-t)*points[i] + t*points[i+1];
                }
            }
             
            curvedPoints.Add(points[0]);
        }
         
        return(curvedPoints.ToArray());
    }

}
