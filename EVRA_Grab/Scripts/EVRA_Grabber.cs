using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using EVRA;
using EVRA.Inputs;
using EVRA.PseudoParenting;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider))]
public class EVRA_Grabber : MonoBehaviour
{
    #region Private References
        [Header("Important References")]
        [SerializeField, Tooltip("The pivot point where a grabbed object anchors to when being grabbed.")]
        private Transform m_pivot;
        public Transform pivot {
            get { return m_pivot; }
            set {}
        }
        [SerializeField, Tooltip("A EVRA_Pointer that can be used for distance grabbing.\nIf you do not want distance grabbing, then make sure this is empty.")]
        private EVRA_Pointer m_pointer;
        
        private EVRA_Hand m_parentHand;   // Needed as a reference to the EVRA_Hand this is under
        public EVRA_Hand parentHand {
            get { return m_parentHand; }
            set {}
        }
        private FakeChild parentHandRel = new FakeChild();
        private Vector3 parentHandDisplacement = Vector3.zero;      // Needed to calculate how much this object must move, given its original position relative to EVRA_Hand
        private Rigidbody _rigidbody;   // Needed as a reference to this object's rigidbody, if it has one.
        private SphereCollider _collider;
    #endregion

    //private FixedJoint joint1, joint2;
    //private Vector3 potentialGrabPosition = Vector3.zero;

    /*
    [SerializeField, Range(0f,1f)]
    private float m_gripRadius = 0.15f;
    public float gripRadius {
        get { return m_gripRadius; }
        set {}
    }
    */

    /*
    private Renderer renderer;
    private Color originalColor;
    */

    /*
    public enum RaycastBehavior {
        FollowHitNormal,
        OriginDirection
    }
    public RaycastBehavior raycastBehavior = RaycastBehavior.FollowHitNormal;
    public LayerMask layersToHit;
    public Transform normalPointRef, oppositePointRef, oppositeSidePointRef;
    */

    #region Grabbable-related Stuff
        [Header("Grabbing Mechanics")]
        [SerializeField, Range(0f,1f), Tooltip("How far (from the center of this object's position) should we seek a grabbable object?")]
        private float m_reachDistance = 0.1f;

        [Header("Potential and Currently Grabbed")]
        [ShowOnly, SerializeField]
        private EVRA_GrabTrigger potentiallyGrabbable;  // Involved specifically with triggers. It's use is dependent on if we are dealing with an object that has a rigidbody or not.

        [ShowOnly, SerializeField]
        private Rigidbody grabbedObject;
        private RigidbodyInterpolation grabbedObjectInterpolationMode;
        private bool grabbedObjectGravity;
        private bool grabbedObjectIsKinematic;
        private EVRA_Grabbable grabbedObjectGrabbable;
        private bool grabbedObjectShouldSnap = false;

        [ShowOnly, SerializeField]
        private Collider grabbedObject_ClosestCollider;
        private Vector3 m_pivotDisplacement;
        
    #endregion


    void Awake() {
        SetupParent();
        SetupCollider();
        SetupRigidbody();
        SetupPivot();
        // these ones can be removed later
        /*
        SetupRenderer();
        */
    }

    void SetupParent() {
        m_parentHand = GetComponentInParent<EVRA_Hand>();
        parentHandRel.CalculateOffsets(m_parentHand.transform, this.transform);
    }

    void SetupCollider() {
        _collider = gameObject.GetComponent<SphereCollider>();
        if (_collider == null) {
            _collider = gameObject.AddComponent<SphereCollider>();
        }
        _collider.isTrigger = true;
        _collider.radius = 0.5f;
        transform.localScale = Vector3.one * m_reachDistance;
        
    }

    void SetupRigidbody() {
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        _rigidbody.isKinematic = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    void SetupPivot() {
        if (m_pivot == null) {
            GameObject objToSpawn = new GameObject("Grabber Pivot");
            m_pivot = objToSpawn.transform;
        }
        m_pivot.parent = this.transform;
        m_pivot.localPosition = Vector3.zero;
        m_pivot.localRotation = Quaternion.identity;
    }

    /*
    void SetupRenderer() {
        renderer = GetComponent<Renderer>();
        originalColor = renderer.material.color;
    }
    */


    void Update() {
        UpdatePosition();
        if (grabbedObjectShouldSnap) {
            m_pivot.localPosition = Vector3.zero;
            m_pivot.localRotation = Quaternion.identity;
        }
        /*
        UpdateRenderer();
        */
    }

    void UpdatePosition() {
        parentHandRel.Move();
    }

    /*
    void UpdateRenderer() {
        if (grabbedObject != null) renderer.material.SetColor("_Color", Color.black);
        else if (potentiallyGrabbable != null) renderer.material.SetColor("_Color", Color.blue);
        else renderer.material.SetColor("_Color", originalColor);
    }
    */

    /*
    void CheckContactPoint() {
        if (potentiallyGrabbable == null) {
            ResetGrabMarkers();
            return;
        }
        // We've in range a grabTrigger. We now need to double-check if 1) there is an object in front of us...
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, layersToHit)) {
            // ... and 2), if it contains any 
        } else {
            ResetGrabMarkers();
            potentialGrabPosition = potentiallyGrabbable.transform.position;
        }

        switch(raycastBehavior) {
            case (RaycastBehavior.FollowHitNormal):
                FollowHitNormal();
                break;
            case (RaycastBehavior.OriginDirection):
                FollowOriginDirection();
                break;
        }
    }
    void ResetGrabMarkers() {
        normalPointRef.gameObject.SetActive(false);
        normalPointRef.localPosition = Vector3.zero;
        oppositePointRef.gameObject.SetActive(false);
        oppositePointRef.localPosition = Vector3.zero;
        oppositeSidePointRef.gameObject.SetActive(false);
        oppositeSidePointRef.localPosition = Vector3.zero;
    }
    void FollowHitNormal() {
        RaycastHit hit, hit2;
        if (Physics.Raycast(transform.position, potentiallyGrabbable.transform.position - transform.position, out hit, Mathf.Infinity, layersToHit)) {
                normalPointRef.gameObject.SetActive(true);
                normalPointRef.position = hit.point;
                //printVector3("Normal of Point",hit.normal);
                Vector3 oppositeDir = -1f * hit.normal * m_gripRadius;
                Vector3 opposite = oppositeDir + normalPointRef.position;
                //printVector3("Opposite of normal",opposite);
                oppositePointRef.gameObject.SetActive(true);
                oppositePointRef.position = opposite;
                if (Physics.Raycast(opposite, hit.normal, out hit2, m_gripRadius, layersToHit)) {
                        oppositeSidePointRef.gameObject.SetActive(true);
                        oppositeSidePointRef.position = hit2.point;
                        potentialGrabPosition = hit.point;
                } else {
                    oppositeSidePointRef.gameObject.SetActive(false);
                    potentialGrabPosition = potentiallyGrabbable.transform.position;
                }
        } else {
            normalPointRef.gameObject.SetActive(false);
            normalPointRef.localPosition = Vector3.zero;
            oppositePointRef.gameObject.SetActive(false);
            oppositePointRef.localPosition = Vector3.zero;
            oppositeSidePointRef.gameObject.SetActive(false);
            oppositeSidePointRef.localPosition = Vector3.zero;
            potentialGrabPosition = potentiallyGrabbable.transform.position;
        }
    }
    void FollowOriginDirection() {
        RaycastHit hit, hit2;
        if (Physics.Raycast(transform.position, potentiallyGrabbable.transform.position - transform.position, out hit, Mathf.Infinity, layersToHit)) {
                normalPointRef.gameObject.SetActive(true);
                normalPointRef.position = hit.point;
                //printVector3("Normal of Point",hit.normal);
                Vector3 oppositeDir = transform.forward * m_gripRadius;
                Vector3 opposite = oppositeDir + normalPointRef.position;
                //printVector3("Opposite of normal",opposite);
                oppositePointRef.gameObject.SetActive(true);
                oppositePointRef.position = opposite;
                if (Physics.Raycast(opposite, -transform.forward, out hit2, m_gripRadius, layersToHit)) {
                    oppositeSidePointRef.gameObject.SetActive(true);
                    oppositeSidePointRef.position = hit2.point;
                    potentialGrabPosition = hit.point;
                } else {
                    oppositeSidePointRef.gameObject.SetActive(false);
                    potentialGrabPosition = potentiallyGrabbable.transform.position;
                }
        } else {
            normalPointRef.gameObject.SetActive(false);
            normalPointRef.localPosition = Vector3.zero;
            oppositePointRef.gameObject.SetActive(false);
            oppositePointRef.localPosition = Vector3.zero;
            oppositeSidePointRef.gameObject.SetActive(false);
            oppositeSidePointRef.localPosition = Vector3.zero;            
            potentialGrabPosition = potentiallyGrabbable.transform.position;
        }
    }
    */

    public async void GrabBegin() {
        // If we're already grabbing something, then we return early
        if (grabbedObject != null) return;

        // Pre-set some variables...
        Collider closestCollider = null;
        Rigidbody bodyToGrab = null;
        bool shouldSnap = false;
        Transform shouldSnapReference = null;

        // So here's the rub... We need to consider two options:
        // Are we using GrabTriggers... or not?
        if (potentiallyGrabbable) {
            // Remember: potentiallyGrabbable is referring to the parent of the grabTrigger we're interacting with. So it's all good.
            closestCollider = potentiallyGrabbable.immediateParent;
            bodyToGrab = potentiallyGrabbable.parentBody;
            //if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(potentiallyGrabbable.gameObject)) {
            shouldSnap = potentiallyGrabbable.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnap;
            shouldSnapReference = potentiallyGrabbable.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnapReference;
            //}
            // We'll skip else case and continue with the function
        } 
        else if (m_pointer != null && m_pointer.raycastTarget != null) {
            Collider collider = m_pointer.raycastTarget.gameObject.GetComponent<Collider>();
            if (CommonFunctions.HasComponent<EVRA_Grabber>(collider.gameObject)) return;
            if (CommonFunctions.HasComponent<EVRA_CharacterController>(collider.gameObject)) return;
            Rigidbody body = collider.gameObject.GetComponent<Rigidbody>();
            if (body != null) {
                closestCollider = collider;
                bodyToGrab = body;
                if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(collider.gameObject)) {
                    shouldSnap = collider.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnap;
                    shouldSnapReference = collider.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnapReference;
                }
            } else {
                body = collider.gameObject.GetComponentInParent<Rigidbody>();
                if (body == null) return;
                if (CommonFunctions.HasComponent<EVRA_Grabber>(body.gameObject)) return;
                if (CommonFunctions.HasComponent<EVRA_CharacterController>(body.gameObject)) return;
                closestCollider = collider;
                bodyToGrab = body;
                if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(body.gameObject)) {
                    shouldSnap = body.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnap;
                    shouldSnapReference = body.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnapReference;
                }
                else if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(collider.gameObject)) {
                    shouldSnap = collider.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnap;
                    shouldSnapReference = collider.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnapReference;
                }
            }
        }
        else {
            // Since we don't use potentiallyGrabbable, we need to grab all colliders within the reach distance
            Collider[] grabbableColliders = Physics.OverlapSphere(transform.position, m_reachDistance);
            if (grabbableColliders.Length == 0) return;
            List<Collider> sortedGrabbableColliders = grabbableColliders.ToList();
            sortedGrabbableColliders.Sort((x,y)=> Vector3.Distance(transform.position, x.ClosestPoint(transform.position)).CompareTo(Vector3.Distance(transform.position, y.ClosestPoint(transform.position))));
            // for(int i = 0; i < grabbableColliders.Length; i++) {
            for(int i = 0; i < sortedGrabbableColliders.Count; i++) {
                Collider collider = grabbableColliders[i];
                if (CommonFunctions.HasComponent<EVRA_Grabber>(collider.gameObject)) continue;
                if (CommonFunctions.HasComponent<EVRA_CharacterController>(collider.gameObject)) continue;
                Rigidbody body = collider.gameObject.GetComponent<Rigidbody>();
                if (body != null) {
                    closestCollider = collider;
                    bodyToGrab = body;
                    if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(collider.gameObject)) {
                        shouldSnap = collider.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnap;
                        shouldSnapReference = collider.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnapReference;
                    }
                    break;
                } else {
                    body = collider.gameObject.GetComponentInParent<Rigidbody>();
                    if (body == null) continue;
                    if (CommonFunctions.HasComponent<EVRA_Grabber>(body.gameObject)) continue;
                    if (CommonFunctions.HasComponent<EVRA_CharacterController>(body.gameObject)) continue;
                    closestCollider = collider;
                    bodyToGrab = body;
                    if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(body.gameObject)) {
                        shouldSnap = body.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnap;
                        shouldSnapReference = body.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnapReference;
                    }
                    if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(collider.gameObject)) {
                        shouldSnap = collider.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnap;
                        shouldSnapReference = collider.gameObject.GetComponent<EVRA_GrabTrigger>().shouldSnapReference;
                    }
                    break;
                }
            }
        }
        
        // If we don't have any object that we want to grab, then we return early.
        if (bodyToGrab == null) return;

        // Initiate our grab function
        GrabObject(closestCollider, bodyToGrab, shouldSnap, shouldSnapReference);
    }

    private void GrabObject(Collider closestCollider, Rigidbody targetBody, bool shouldSnap, Transform shouldSnapReference) {
        // Move pivot point to closest position
        if (shouldSnap) {
            // Move pivot to match position and rotation of closest collider
            m_pivot.position = shouldSnapReference.position;
            m_pivot.rotation = shouldSnapReference.rotation;
            // Then set pivot displacement to zero, so that it follows the object
            m_pivotDisplacement = Vector3.zero;
        } else {
            m_pivot.position = closestCollider.ClosestPoint(transform.position);
            m_pivotDisplacement = m_pivot.position - transform.position;
        }


        // Check if this object is already being grabbed by something
        EVRA_Grabbable grabbable = targetBody.gameObject.GetComponent<EVRA_Grabbable>();
        if (grabbable == null) {
            grabbable = targetBody.gameObject.AddComponent<EVRA_Grabbable>() as EVRA_Grabbable;
        }

        // Add ourselves to this grabbable's list of grabbers
        if (grabbable.AddGrabber(this)) {
            grabbedObject = targetBody;
            grabbedObject_ClosestCollider = closestCollider;
            grabbedObjectGrabbable = grabbable;
            grabbedObjectShouldSnap = shouldSnap;
        }
        else {
            // We failed to grab the object...
            grabbedObject = null;
            grabbedObject_ClosestCollider = null;
            grabbedObjectGrabbable = null;
            grabbedObjectShouldSnap = false;
        }
    }

    /*
    private void GrabObject(Collider targetCollider, Rigidbody targetBody) {
        // Create a grab point
        m_pivot.position = targetCollider.ClosestPoint(transform.position);
        m_pivotDisplacement = m_pivot.position - targetBody.transform.position;
        //m_pivot.parent = grabbedObject;

        // Freeze grabbed object motion
        grabbedObjectGravity = targetBody.useGravity;
        //grabbedObjectIsKinematic = targetBody.isKinematic;
        //grabbedObjectInterpolationMode = targetBody.interpolation;

        targetBody.velocity = Vector3.zero;
        targetBody.angularVelocity = Vector3.zero;
        //targetBody.isKinematic = true;
        //targetBody.interpolation = RigidbodyInterpolation.Interpolate;
        targetBody.useGravity = false;

        // Attach joints
        joint1 = gameObject.AddComponent<FixedJoint>();
        joint1.connectedBody = targetBody;
        joint1.breakForce = float.PositiveInfinity;
        joint1.breakTorque = float.PositiveInfinity;
        joint1.connectedMassScale = 1;
        joint1.massScale = 1;
        joint1.enableCollision = false;
        joint1.enablePreprocessing = false;

        joint2 = targetBody.gameObject.AddComponent<FixedJoint>();
        joint2.connectedBody = _rigidbody;
        joint2.breakForce = float.PositiveInfinity;
        joint2.breakTorque = float.PositiveInfinity;
        joint2.connectedMassScale = 1;
        joint2.massScale = 1;
        joint2.enableCollision = false;
        joint2.enablePreprocessing = false;
    }
    */

    /*
    public void GrabBegin() {
        if (!grabbedObject) return;
        if (grabbedObject.GrabEnd(this)) {
            grabbedObject = null;
            m_pivot.localPosition = Vector3.zero;
        } else {
            Debug.Log("Failed Grab End...");
        }
    }
    */

    public void GrabEnd() {

        if (grabbedObject == null) return;

        if (grabbedObjectGrabbable.RemoveGrabber(this)) {
            grabbedObject = null;
            grabbedObject_ClosestCollider = null;
            if (grabbedObjectGrabbable.HasNoGrabbers()) {
                // need to destroy this one
                grabbedObjectGrabbable.Die();
            }
            grabbedObjectGrabbable = null;
            grabbedObjectShouldSnap = false;
            m_pivot.localPosition = Vector3.zero;
            m_pivot.localRotation = Quaternion.identity;
        }

        /*
        // We'll be destroying all aspects regarding both rigidbody and non-rigidbody 
        if (joint1 != null) Destroy(joint1);
        if (joint2 != null) Destroy(joint2);
        //m_pivot.parent = this.transform;
        m_pivot.localPosition = Vector3.zero;

        if (grabbedObject != null) {
            //grabbedObject.isKinematic = grabbedObjectIsKinematic;
            //grabbedObject.interpolation = grabbedObjectInterpolationMode;
            grabbedObject.useGravity = grabbedObjectGravity;
        }

        grabbedObject = null;
        */
    }

    public void OnTriggerEnter(Collider other) {
        // When we are triggered, we're specifically looking for objects that have an EVRA_New_GrabTrigger attached to it.
        // Otherwise, we ignore the collision
        if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(other.gameObject)) {
            EvaluatePotentiallyGrabbed(other.gameObject.GetComponent<EVRA_GrabTrigger>());
        }
    }

    public void OnTriggerStay(Collider other) {
        // When we are triggered, we're specifically looking for objects that have an EVRA_New_GrabTrigger attached to it.
        // Otherwise, we ignore the collision
        if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(other.gameObject)) {
            EvaluatePotentiallyGrabbed(other.gameObject.GetComponent<EVRA_GrabTrigger>());
        }
    }

    public void OnTriggerExit(Collider other) {
        // When we are triggered, we're specifically looking for objects that have an EVRA_New_GrabTrigger attached to it.
        // Otherwise, we ignore the collision
        if (CommonFunctions.HasComponent<EVRA_GrabTrigger>(other.gameObject)) {
            RemovePotentiallyGrabbed(other.gameObject.GetComponent<EVRA_GrabTrigger>());
        }
    }

    private void EvaluatePotentiallyGrabbed(EVRA_GrabTrigger other) {
        // We're expecting that it's the GrabTrigger's parent that we're hoping to grab
        // GrabTrigger has a reference to "parent". We'll use that.
        // We've ensured that GrabTrigger's parent is a collider. Doesn't matter if it has a rigidbody or not - we'll handle that when it comes.
        if (other.parentBody == null) return;
        if (potentiallyGrabbable == null) {
            // If we don't have any that's potentially grabbable, then it's very easy.
            potentiallyGrabbable = other;
        } 
        else if (potentiallyGrabbable != other) {
            // Uh oh, we found a potential conflict. We need to estimate which one is closer to us.
            if (Vector3.Distance(transform.position, other.immediateParent.ClosestPoint(transform.position)) < Vector3.Distance(transform.position, potentiallyGrabbable.immediateParent.ClosestPoint(transform.position))) {
                // The other.colliderParent was closer to our existing potentially grabbable. So we replace it
                potentiallyGrabbable = other;
            }
        }
    }

    private void RemovePotentiallyGrabbed(EVRA_GrabTrigger other) {
        if (potentiallyGrabbable == other) potentiallyGrabbable = null;
    }
    
}
