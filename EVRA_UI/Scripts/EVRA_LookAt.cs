using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_LookAt : MonoBehaviour
{
    public Transform lookAtTargetRef;
    
    public enum VectorDir { TowardTarget, TowardSource }
    public VectorDir dir = VectorDir.TowardTarget;

    private void Update() {
        if (lookAtTargetRef != null) {
            Vector3 d = (dir == VectorDir.TowardSource) 
                ? transform.position - lookAtTargetRef.position 
                : lookAtTargetRef.position - transform.position;
            transform.rotation = Quaternion.LookRotation(d);
        }
    }
}
