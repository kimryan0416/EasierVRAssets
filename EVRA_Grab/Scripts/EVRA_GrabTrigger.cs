using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EVRA_GrabTrigger : MonoBehaviour
{   
    //private EVRA_New_Grabbable parent;
    [Header("Parenting References")]
    [SerializeField, Tooltip("Which grabbable object should this trigger act as a grabbable area for?")]
    private Rigidbody m_parentBody;
    public Rigidbody parentBody {
        get { return m_parentBody; }
        set {}
    }
    /*
    private Collider m_parentCollider;
    public Collider parentCollider {
        get { return m_parentCollider; }
        set {}
    }
    */
    [ShowOnly, SerializeField]
    private Collider m_immediateParent;
    public Collider immediateParent {
        get { return m_immediateParent; }
        set {}
    }


    [Header("Snapping Preferences")]
    [SerializeField, Tooltip("Should the grabbable object this is a trigger for snap to match the hand's position?")]
    private bool m_shouldSnap = false;
    public bool shouldSnap {
        get { return m_shouldSnap; }
        set {}
    }
    [SerializeField, Tooltip("If this grabbable object should snap, which Transform's position and orientation should it match?\n\nIf not set, will auto-set to itself.")]
    private Transform m_shouldSnapReference;
    public Transform shouldSnapReference {
        get { return m_shouldSnapReference; }
        set {}
    }

    /*
    public bool GrabBegin(EVRA_New_Grabber grabber, Transform pivot) {
        if (!parent.enabled) return false;
        //if (m_currentGrabber != null) return false;
        bool result = parent.TriggerGrabbed(this, grabber, pivot);
        //if (result) m_currentGrabber = grabber;
        return result;
    } 

    public bool GrabEnd(EVRA_New_Grabber grabber) {
        //if (m_currentGrabber == null) return false;
        //if (m_currentGrabber != grabber) return false;
        bool result = parent.TriggerLetGo(this, grabber);
        //if (result) m_currentGrabber = null;
        return result;
    }
    */

    void Awake() {

        gameObject.GetComponent<Collider>().isTrigger = true;

        if (m_shouldSnap && m_shouldSnapReference == null) m_shouldSnapReference = transform;

        // Get immediate parent collider
        m_immediateParent = transform.parent.GetComponentInParent<Collider>();

        // Finding rigidbody parent
        if (m_parentBody == null) {
            m_parentBody = transform.gameObject.GetComponentInParent<Rigidbody>();
            // If body is nonexistent, then ultimately we're not going to be able to do anythign with this object in particular.
        }
        /*
        if (m_parentBody != null) {
            m_parentCollider = m_parentBody.gameObject.GetComponent<Collider>();
            // If our parent "rigidbody" doesn't happen to have a collider, then we're screwed.
            // So we auto-sent m_parentBody back to 'null' to prevent this from being tracked.
            if (m_parentBody.gameObject != this.gameObject && m_parentCollider == null) m_parentBody = null;
        }
        */
    }

    /*
    public void Init(EVRA_New_Grabbable parent) {
        this.parent = parent;
    }
    */
}
