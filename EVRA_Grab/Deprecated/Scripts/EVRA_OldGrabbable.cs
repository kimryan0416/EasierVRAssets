using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_OldGrabbable : MonoBehaviour
{
    #region Wordly things... aka scoped outside of this object
        private Transform originalWorldParent;
        public EVRA_OldGrabber currentGrabber;
    #endregion

    #region Grabbable Parent
        [SerializeField] [Tooltip("If this is the child of a grander object that we want to grab, then we can use this as a referral.")]
        private Transform grabbableParent;
        private Rigidbody grabbableParentRB;
        private bool originalKinematicState;
    #endregion

    #region Triggers that will trigger the grabber
        [SerializeField] [Tooltip("If you wish to make this object snap to position, add these transforms to this list. If left blank, the script will use whatever is set to 'grabbableParent' (or the object itself, if 'grabbableParent' is not set).")]
        private List<EVRA_OldGrabTrigger> m_triggers = new List<EVRA_OldGrabTrigger>();
    #endregion

    #region Should we Snap?
        [SerializeField] [Tooltip("Should this object, when grabbed, snap into position and rotation to match the grabber?")]
        private bool m_shouldSnap = false;
    #endregion

    private void Awake() {
        // If we don't have a grabbable parent set, we set it to this own object for safety
        if (!grabbableParent) grabbableParent = this.transform;
        // Based on the grabbable parent, we collect various aspects such as who's the parrent of our grabbable parent and what are their rigidobyd aspects, if they ahve any
        originalWorldParent = grabbableParent.parent;
        grabbableParentRB = grabbableParent.GetComponent<Rigidbody>();
        originalKinematicState = (grabbableParentRB) ? grabbableParentRB.isKinematic : false;
        // For each of the triggers, we need to make sure they know that THIS is the CustomGrabbable that the grabber needs to refer to
        foreach(EVRA_OldGrabTrigger t in m_triggers) {
            t.Init(this);
        }
    }

    // This is called by the grabber currently grabbing the object
    public void GrabBegin(EVRA_OldGrabber newGrabber, EVRA_OldGrabTrigger detectedTrigger) {
        // If we're actually being held by another grabber, we prematurely end that grabber's grabbing
        if (currentGrabber != null && currentGrabber != newGrabber) currentGrabber.GrabEnd();
        // If our grabbable parent has a rigidbody, we set its kinematic setting to true to prevent collisiosn
        if (grabbableParentRB) grabbableParentRB.isKinematic = true;
        // We set grabbable parent as a child of our grabber
        grabbableParent.SetParent(newGrabber.transform);
        // We save a reference to our new grabber
        currentGrabber = newGrabber;

        // If we want to snap, we usually snap based on the clsoest detected trigger (passed on from the grabber).
        // In other words, the grabber has already detected what's the closest trigger. All we need to do is snap our grabbable object relative to that trigger's position on the grabbable object
        if (m_shouldSnap) {
            Transform to = detectedTrigger.transform;
            Quaternion destinationRotation = currentGrabber.transform.rotation * Quaternion.Inverse(to.localRotation) * Quaternion.Euler(45,0,0);
            grabbableParent.transform.rotation = destinationRotation;
            Vector3 destinationPosition = currentGrabber.transform.position + (grabbableParent.position - to.position);
            grabbableParent.transform.position = destinationPosition;
        }
    }
    public void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity) {
        grabbableParent.SetParent(originalWorldParent);
        if (grabbableParentRB) {
            grabbableParentRB.isKinematic = originalKinematicState;
            grabbableParentRB.velocity = linearVelocity;
            grabbableParentRB.angularVelocity = angularVelocity;
        }
        currentGrabber = null;
    }
    public void GrabEnd() {
        grabbableParent.SetParent(originalWorldParent);
        currentGrabber = null;
    }
    // This one is called under the unique situation that the other grab volume is called in its own GrabEnd() instance.
    // Therefore, we don't need to call `GrabEnd()` with the other hand nor do any snapping
    public void SwitchHand(EVRA_OldGrabber newGrabber) {
        // We set grabbable parent as a child of our grabber
        grabbableParent.SetParent(newGrabber.transform, true);
        // We save a reference to our new grabber
        currentGrabber = newGrabber;
    }
}
