using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* --------------------
The purpose of this class is to move the current object towards a selected target. This is done via a SmoothDamp for positioning.
-------------------- */
public class MoveTowardsTarget : MonoBehaviour
{

    private Rigidbody m_rigidbody;

    // What should be the target that we're shooting towards?
    public Transform m_Target;

    // How smooth the time should be for the smoothdamp
    public float m_smoothPositionTime = 0.3f;

    // Stores the positional velocity of this object.
    private Vector3 m_movementVelocity = Vector3.zero;
    // Stores the rotational velocity of this object.
    private Vector3 m_rotationalVelocity = Vector3.zero;

    void Awake() {
        m_rigidbody = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_Target == null) return;
        MoveTo();
        RotateTo();
    }

    private void MoveTo() {
        // Determine the target position
        Vector3 targetPosition = m_Target.position;
        // Smoothly damp towards the target position
        if (m_rigidbody != null) {
            // if we have a rigidbody, use movePosition instead
            m_rigidbody.MovePosition(targetPosition);
        } else {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_movementVelocity, m_smoothPositionTime);
        }
    }

    private void RotateTo() {
        /*
        Vector3 targetRotation = m_Target.eulerAngles;
        transform.rotation = Quaternion.Euler(
            Vector3.SmoothDamp(transform.rotation.eulerAngles, targetRotation, ref m_rotationalVelocity, m_smoothPositionTime)
        );
        */
        if (m_rigidbody != null) {
            m_rigidbody.MoveRotation(m_Target.rotation);
        } else {
            transform.rotation = m_Target.rotation;
        }
    }
}
