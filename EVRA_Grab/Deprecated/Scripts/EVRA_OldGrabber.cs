using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using EVRA.Inputs;

public class EVRA_OldGrabber : MonoBehaviour
{
    [SerializeField] [Tooltip("Grabber currently associated with our grabbing mechanism")]
    private EVRA_Hand m_CustomGrabber;

    [SerializeField] [Tooltip("Is there a custom pointer v2 attached?")]
    private EVRA_Pointer m_Pointer;

    [SerializeField] [Tooltip("The transform where the collision should occur")]
    private Transform m_collisionOrigin;

    [Tooltip("Reference to another grabber that's attempting to hold the same object")] // NOT SERIALIZED
    private EVRA_OldGrabber m_OtherGrabVolume = null;
    public EVRA_OldGrabber OtherGrabVolume {
        get {   return m_OtherGrabVolume;   }
        set {   m_OtherGrabVolume = value; }
    }
    private Quaternion otherGrabVolRelRotation;
    private Vector3 otherGrabVolRelPosition;

    /*
    [SerializeField] [Tooltip("The world-space radius where detection should occur")]
    private float m_collisionRadius = 0.5f;
    public float collisionRadius {
        get {   return m_collisionRadius;   }
        set {   m_collisionRadius = value;  }
    }
    */

    [Tooltip("Reference to all objects that are in range")] // NOT SERIALIZED
    private List<EVRA_OldGrabTrigger> m_inRange = new List<EVRA_OldGrabTrigger>();
    public List<EVRA_OldGrabTrigger> inRange {
        get {   return m_inRange;   }
    }

    [Tooltip("Reference to the object currently being grabbed")] // NOT SERIALIZED
    private EVRA_OldGrabTrigger m_grabbed = null;
    public EVRA_OldGrabTrigger grabbed {
        get {   return m_grabbed;   }
        set {}
    }

    [Tooltip("Reference to mesh renderer")] // NOT SERIALIZED
    private Renderer m_Renderer;

    public enum GrabType {
        Normal,
        Distance,
        Both,
    }
    [SerializeField] [Tooltip("What kind of grab mechanism should we use?")]
    private GrabType m_grabType = GrabType.Normal;

    [Tooltip("Reference to the grab volume's original Color")] // NOT SERIALIZED
    private Color originalColor;

    [SerializeField, Tooltip("Radius from the point of origin where collisions for grabbable objects must be tracked")]
    private float m_grabRadius = 0.1f;

    public GameObject DebugObject;

    private void Awake() {
        if (!m_collisionOrigin) m_collisionOrigin = this.transform;
        m_Renderer = m_collisionOrigin.GetComponent<Renderer>();

        originalColor = m_Renderer.material.color;
    }

    private void Update() {
        if (DebugObject != null) {
            DebugObject.GetComponent<Renderer>().enabled = m_inRange.Count > 0;
        }

        if (m_OtherGrabVolume) {
            /*
            Vector3 dir = m_OtherGrabVolume.transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
            partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
            */
            //Vector3 parentSpaceDirection = m_OtherGrabVolume.transform.position - transform.position;
            //transform.rotation = Quaternion.LookRotation(parentSpaceDirection) * otherGrabVolRelRotation;
            //transform.LookAt(m_OtherGrabVolume.transform.position);
            transform.rotation = Quaternion.LookRotation(m_OtherGrabVolume.transform.position - transform.position) * otherGrabVolRelRotation;
        }

        if (m_grabbed != null) {
            if (m_OtherGrabVolume) m_Renderer.material.SetColor("_Color", Color.black);
            else m_Renderer.material.SetColor("_Color", Color.blue);
        } else {
            Color oldColor = originalColor;
            float alphaVal = (m_inRange.Count > 0 || (m_Pointer != null && m_Pointer.raycastTarget != null && m_Pointer.raycastTarget.GetComponent<EVRA_OldGrabTrigger>())) ? 1f : 0f;
            Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alphaVal);
            m_Renderer.material.SetColor("_Color", newColor);
        }
    }

    public void GrabBegin() {
        m_grabbed = this.FindClosestObject();        
        if (!m_grabbed) return;   // If no grabbable objects in range or at the end of our pointer, end quickly
        // There's a difference in what'll happen:
        // If the current object that's being grabbed has no grabber attached to it, we grab it like normal.
        // However, if the current object is being grabbed already, we do some things:
        //  1) If we grab at the same exact trigger point, we simply switch the hands, no problem.
        //  2) if we grab at different trigger points, then we make it so that the hand that's currently grabbing the object will rotate based on the position of our new hand.
        //      For example, if the right hand is grabbing a gun's handle and the left hand tries to grab the gun's barrel, then the right hand won't let go - rather, it will rotate to match the direction to our left hand.
        //      The rotation resistance will be relative to how loosely or strongly the grab value is. If the grab value >= 0.9, then it ain't moving. If the grab value 0.5 <= x < 0.9, then resisteance is little but it'll rotate. If x < 0.5, then it'll rotate without resistance
        EVRA_OldGrabbable grabbedParent = m_grabbed.GrabbableRef;
        if (grabbedParent.currentGrabber == null) m_grabbed.GrabbableRef.GrabBegin(this, m_grabbed);
        else if (grabbedParent.currentGrabber.grabbed == m_grabbed) m_grabbed.GrabbableRef.GrabBegin(this, m_grabbed);
        else {
            grabbedParent.currentGrabber.AddOtherGrabVolume(this);
            AddOtherGrabVolume(grabbedParent.currentGrabber);
        }
    }
    public void GrabBegin(OVRInput.Controller c, Dictionary<InputType, InputResult> r) {
        GrabBegin();
    }
    

    public void GrabEnd() {
        EVRA_OldGrabber otherGrabberRef = m_OtherGrabVolume;
        RemoveOtherGrabVolume();

        if (!m_grabbed) return;   // If no objects currently grabbed, end quickly
        
        // There are two situations:
        // 1) the grabbed object has only one hand grabbing it (aka this one)
        // 2) the grabbed object has two hands grabbing it

        // 1) We need to call `GrabEnd()` with the appropriate linear and angular velocity
        // 2) We need to switch parenting over to the other hand via `SwitchHand()`. To tell if there is another grab volume grabbing the object, we just check if m_OtherGrabVolume is not null

        if (otherGrabberRef) {
            // If we have an other grabber ref, then that means that multiple objects are holding the current item.
            // Now, technically, it can be either the initial holder of the item or the rotation ref.
            // If it's the initial holder, then we let go AND transfer possession to the other grab volume
            // If it's just the rotation ref, then we just need to decouple the other grab volume - we don't need to let go of it.
            if (m_grabbed.GrabbableRef.currentGrabber == this) {
                //otherGrabberRef.transform.localRotation = Quaternion.Euler(0f,0f,0f);
                m_grabbed.GrabbableRef.SwitchHand(otherGrabberRef);
                //m_grabbed.GrabbableRef.GrabEnd();
            }
            otherGrabberRef.RemoveOtherGrabVolume();
            //if (m_grabbed.GrabbableRef.currentGrabber.grabbed != m_grabbed) m_grabbed.GrabbableRef.currentGrabber.RemoveOtherGrabVolume();
        } else {
            OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(m_CustomGrabber.OVRController), orientation = OVRInput.GetLocalControllerRotation(m_CustomGrabber.OVRController) };
            OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
            Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_CustomGrabber.OVRController);
            Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_CustomGrabber.OVRController);
            angularVelocity *= -1;
            m_grabbed.GrabbableRef.GrabEnd(linearVelocity,angularVelocity);
            //m_grabbed.GrabbableRef.currentGrabber.RemoveOtherGrabVolume();
        }

        /*
        if (m_grabbed.GrabbableRef.currentGrabber && m_grabbed.GrabbableRef.currentGrabber.OtherGrabVolume) {
            if (m_grabbed.GrabbableRef.currentGrabber.grabbed && m_grabbed.GrabbableRef.currentGrabber.grabbed != m_grabbed) m_grabbed.GrabbableRef.currentGrabber.RemoveOtherGrabVolume();
        }
        if (m_grabbed.GrabbableRef.currentGrabber == this) {
            OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(m_CustomGrabber.OVRController), orientation = OVRInput.GetLocalControllerRotation(m_CustomGrabber.OVRController) };
            OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
            Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_CustomGrabber.OVRController);
            Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_CustomGrabber.OVRController);
            angularVelocity *= -1;
            m_grabbed.GrabbableRef.GrabEnd(linearVelocity,angularVelocity);
        }
        */
        transform.localRotation = Quaternion.Euler(0f,0f,0f);
        m_grabbed = null;
    }
    public void GrabEnd(OVRInput.Controller c, Dictionary<InputType, InputResult> r) {
        GrabEnd();
    }

    // When we initiate the grabbing via `GrabBegin()`, we need to figure out which CustomGrabbable we want to refer to.
    private EVRA_OldGrabTrigger FindClosestObject() {
        EVRA_OldGrabTrigger closest = null;
        if (
            m_inRange.Count > 0 && 
            (m_grabType == GrabType.Normal || m_grabType == GrabType.Both)
        ) {
            closest = m_inRange[0];
            float dist = Vector3.Distance(m_collisionOrigin.position, closest.transform.position);
            foreach (EVRA_OldGrabTrigger obj in m_inRange) {
                float tempDist = Vector3.Distance(m_collisionOrigin.position, obj.transform.position);
                if ( tempDist < dist) {
                    closest = obj;
                    dist = tempDist;
                }
            }
        } else if (
            m_inRange.Count == 0 &&
            (m_grabType == GrabType.Distance || m_grabType == GrabType.Both) &&
            m_Pointer != null &&
            m_Pointer.raycastTarget && 
            m_Pointer.raycastTarget.GetComponent<EVRA_OldGrabTrigger>()
        ) {
            closest = m_Pointer.raycastTarget.GetComponent<EVRA_OldGrabTrigger>();
        }
        return closest;
    }

    // Detect when we encounter a Grabbable trigger.
    public void OnTriggerEnter(Collider other) {
        Debug.Log("EVRA_GRABBER - Collider Trigger entered");
        // Are we looking at a CustomGrabbable... or a Trigger?
        EVRA_OldGrabTrigger possibleTrigger = other.GetComponent<EVRA_OldGrabTrigger>();
        // If we aren't looking at either, we yeet on out of here
        if (!possibleTrigger || possibleTrigger.GrabbableRef == null) return;
        // If we already don't contain the CustoGrabbable related to this collider, then we add it to what's in range.
        if (!m_inRange.Contains(possibleTrigger)) m_inRange.Add(possibleTrigger);
    }

    // Detect when we a Grabbable trigger leaves.
    public void OnTriggerExit(Collider other) {
        Debug.Log("EVRA_GRABBER - Collider Trigger exited");
        // Are we looking at a CustomGrabbable... or a Trigger?
        EVRA_OldGrabTrigger possibleTrigger = other.GetComponent<EVRA_OldGrabTrigger>();
        // If we aren't looking at either, we yeet on out of here
        if (!possibleTrigger || possibleTrigger.GrabbableRef == null) return;
        // If we have the CustoGrabbable in our inRange list, then we remove it.
        if (m_inRange.Contains(possibleTrigger)) m_inRange.Remove(possibleTrigger);
    }

    /*
    // If no collider attached, we need to manually track colliders that are in range of the object.
    private ManualTriggerCheck() {
        Collider[] hitColliders = Physics.OverlapSphere(m_collisionOrigin.position, m_grabRadius);
        // Firstly, check if there are any colliders stored inside m_inRange that need to be removed
        foreach(EVRA_GrabTrigger trig in m_inRange) {
            if (trig.gameObject.GetComponent<Collider>())
        }
    }
    */
    
    public void AddOtherGrabVolume(EVRA_OldGrabber other) {
        m_OtherGrabVolume = other;
        /*
        otherGrabVolRelRotation = Quaternion.FromToRotation(other.transform.forward, transform.forward);
        otherGrabVolRelPosition = other.transform.position - transform.position;
        */
        Quaternion rot = Quaternion.LookRotation(other.transform.position - transform.position);
        otherGrabVolRelRotation = Quaternion.Inverse(rot) * transform.rotation;
    }
    public void RemoveOtherGrabVolume() {
        m_OtherGrabVolume = null;
    }
}
