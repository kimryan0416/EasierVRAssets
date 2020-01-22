using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrabber_GrabVolume : MonoBehaviour
{

    public float collisionRadius = 0.1f;
    private List<GameObject> inRange = new List<GameObject>();

    public List<GameObject> GetInRange() {
        inRange = new List<GameObject>();
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, collisionRadius);
        foreach (Collider c in hitColliders) {
            if (c.GetComponent<CustomGrabbable>() != null) {
                inRange.Add(c.gameObject);
            }
        }
        return inRange;
    }

}
