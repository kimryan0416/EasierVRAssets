using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeAveragePosition : MonoBehaviour
{

    public List<Transform> targetObjects = new List<Transform>();

    public bool modifyPosition = true;
    public bool modifyRot = true;
    public bool modifyScale = true;

    // Update is called once per frame
    void LateUpdate() {
        if (targetObjects.Count == 0) return;
        Vector3 pos = Vector3.zero;
        Vector3 rot = Vector3.zero;
        Vector3 sca = Vector3.zero;
        foreach(Transform t in targetObjects) {
            pos += t.position;
            sca += t.localScale; 
            rot += t.rotation.eulerAngles;
        }
        if (modifyPosition) transform.position = pos / targetObjects.Count;
        if (modifyRot) transform.rotation = Quaternion.Euler(rot / targetObjects.Count);
        if (modifyScale) transform.localScale = sca / targetObjects.Count;
    }
}
