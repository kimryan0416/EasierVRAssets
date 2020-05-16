using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ExternalCollider : MonoBehaviour
{

    [SerializeField]
    private bool m_colliding = false;
    public bool colliding {
        get {   return m_colliding; }
    }
    
    [SerializeField]
    private string m_colliderTag;

    public void ActivateColliders() {
        this.GetComponent<Collider>().enabled = true;
    }

    public void DeactivateColliders() {
        this.GetComponent<Collider>().enabled = false;
    }

    public void ResetStatus() {
        m_colliding = false;
    }

    private void OnTriggerEnter(Collider collision) {
        if (m_colliderTag == null || m_colliderTag == "" || collision.gameObject.CompareTag(m_colliderTag)) {
            m_colliding = true;
        }
    }

    private void OnTriggerExit(Collider collision) {
        if (m_colliderTag == null || m_colliderTag == "" || collision.gameObject.CompareTag(m_colliderTag)) {
            m_colliding = false;
        }
    }

    private void OnCollisionEnter(Collider collision) {
        if (m_colliderTag == null || m_colliderTag == "" || collision.gameObject.CompareTag(m_colliderTag)) {
            m_colliding = true;
        }
    }

    private void OnCollisionExit(Collider collision) {
        if (m_colliderTag == null || m_colliderTag == "" || collision.gameObject.CompareTag(m_colliderTag)) {
            m_colliding = false;
        }
    }
    
}
