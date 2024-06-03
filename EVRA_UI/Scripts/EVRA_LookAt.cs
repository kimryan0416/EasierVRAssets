using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_LookAt : MonoBehaviour
{
    public Transform lookAtTargetRef;

    private void Update() {
        if (lookAtTargetRef != null) 
            transform.rotation = Quaternion.LookRotation(transform.position - lookAtTargetRef.position);
    }
}
