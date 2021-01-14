using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* --------------------
The purpose of this class is to move the current object towards a selected target. This is done via a SmoothDamp for positioning.
-------------------- */
public class MoveTowardsTarget : MonoBehaviour
{

    // What should be the target that we're shooting towards?
    public Transform m_Target;

    // How smooth the time should be for the smoothdamp
    public float m_smoothPositionTime = 0.3f;

    // Stores the positional velocity of this object.
    private Vector3 m_movementVelocity = Vector3.zero;
    // Stores the rotational velocity of this object.
    private Vector3 m_rotationalVelocity = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (m_Target == null) return;
        MoveTo();
        RotateTo();
    }

    private void MoveTo() {
        // Determine the target position
        Vector3 targetPosition = m_Target.position;
        // Smoothly damp towards the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_movementVelocity, m_smoothPositionTime);
    }

    private void RotateTo() {
        /*
        Vector3 targetRotation = m_Target.eulerAngles;
        transform.rotation = Quaternion.Euler(
            Vector3.SmoothDamp(transform.rotation.eulerAngles, targetRotation, ref m_rotationalVelocity, m_smoothPositionTime)
        );
        */
        transform.rotation = m_Target.rotation;
    }
}
