using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EVRA;
using EVRA.PseudoParenting;

[RequireComponent(typeof(Rigidbody))]
public class EVRA_Grabbable : MonoBehaviour
{
    /*
    [SerializeField]
    private List<EVRA_New_GrabTrigger> triggers = new List<EVRA_New_GrabTrigger>();
    */
    
    [SerializeField, Range(0,2)]
    private int maxGrabbers = 2;

    [SerializeField, Range(0f,1f)]
    private float m_secondGrabWeight = 0.25f;

    private Rigidbody rb = null;
    private bool isKinematic = false;
    private bool useGravity = false;

    private List<EVRA_Grabber> grabbers = new List<EVRA_Grabber>();
    //private Dictionary<EVRA_New_Grabber, FakeChildTwoParents> grabberDisplacements = new Dictionary<EVRA_New_Grabber, FakeChildTwoParents>();
    private Dictionary<EVRA_Grabber, FakeChild> grabberDisplacements = new Dictionary<EVRA_Grabber, FakeChild>();

    void Awake() {
        rb = gameObject.GetComponent<Rigidbody>();
        isKinematic = rb.isKinematic;
        useGravity = rb.useGravity;
        /*
        triggers = new List<EVRA_New_GrabTrigger>(GetComponentsInChildren<EVRA_New_GrabTrigger>());
        foreach(EVRA_New_GrabTrigger trigger in triggers) {
            trigger.Init(this);
        }
        */
    }

    public bool AddGrabber(EVRA_Grabber grabber) {
        if (grabbers.Count >= maxGrabbers) return false;
        if (!grabbers.Contains(grabber)) {
            // if (!grabberDisplacements.ContainsKey(grabber)) grabberDisplacements.Add(grabber, new FakeChildTwoParents());
            if (!grabberDisplacements.ContainsKey(grabber)) grabberDisplacements.Add(grabber, new FakeChild());
            rb.isKinematic = true;
            grabberDisplacements[grabber].CalculateOffsets(grabber.pivot, this.transform);
            rb.isKinematic = isKinematic;
            rb.useGravity = false;
            grabbers.Add(grabber);
        }
        return true;
    }

    public bool RemoveGrabber(EVRA_Grabber grabber) {
        if (!grabbers.Contains(grabber)) {
            return false;
        }

        grabbers.Remove(grabber);
        grabberDisplacements[grabber].ResetOffsets();
        
        if (grabbers.Count > 0 && grabbers[0] != grabber) {
            // There's another grabber...
            grabberDisplacements[grabbers[0]].CalculateOffsets();
        } else {
            rb.velocity = grabber.parentHand.velocity;
            rb.angularVelocity = grabber.parentHand.angularVelocity;
        }

        return true;
    }

    public bool IsFirstGrabber(EVRA_Grabber grabber) {
        return (grabbers.Contains(grabber) && grabbers[0]==grabber);
    }

    public bool HasNoGrabbers() {
        return grabbers.Count == 0;
    }

    void Update() {
        if (grabbers.Count > 0) {
            EVRA_Grabber grabber = grabbers[0];
            grabberDisplacements[grabber].Move();
        } else {
            Die();
        }

        // FakeChildTwoParents offsets = grabberDisplacements[grabber];
        // Transform parent = offsets.parent;
        // offsets.RotateParentToOtherParent();

        // Vector3 newpos = parent.TransformPoint(offsets.pos);
        // Vector3 newfw = parent.TransformDirection(offsets.fw);
        // Vector3 newup = parent.TransformDirection(offsets.up);
        // Quaternion newrot = Quaternion.LookRotation(newfw, newup);
        // transform.position = newpos;
        // transform.rotation = newrot;
    }

    public void Die() {
        rb.isKinematic = isKinematic;
        rb.useGravity = useGravity;
        Destroy(this);
    }

    /*
    public bool TriggerGrabbed(EVRA_New_GrabTrigger trigger, EVRA_New_Grabber grabber, Transform pivot) {
        if (
            !triggers.Contains(trigger) ||
            grabbers.Count >= maxGrabbers
        ) {
            Debug.Log("Grabber - Failed to Grab");
            return false;
        }

        if (!grabbers.Contains(grabber)) {
            if (!grabberDisplacements.ContainsKey(grabber)) grabberDisplacements.Add(grabber, new FakeChildTwoParents());
            grabberDisplacements[grabber].SetWeight(m_secondGrabWeight);
            if (rb) {
                rb.isKinematic = false;
                grabberDisplacements[grabber].CalculateOffsets(pivot, this.transform);
                rb.isKinematic = isKinematic;
            } else {
                grabberDisplacements[grabber].CalculateOffsets(pivot, this.transform);
            }
            grabbers.Add(grabber);
        }

        if (
            grabbers.Count > 1 && 
            grabbers[0] != grabber
        ) {
            // We're already being grabbed by another grabber.
            // We need to make it so that the pivot of the first grabber looks at the second pivot. And recalculate everything
            grabberDisplacements[grabbers[0]].SetOtherParent(pivot);
        }

        return true;
    }

    public bool TriggerLetGo(EVRA_New_GrabTrigger trigger, EVRA_New_Grabber grabber) {
       
       if (
            !triggers.Contains(trigger) ||
            !grabbers.Contains(grabber)
        ) {
            Debug.Log("Grabber - Failed to let go");
            return false;
        }

        grabbers.Remove(grabber);
        grabberDisplacements[grabber].ResetOffsets();
        
        if (grabbers.Count > 0 && grabbers[0] != grabber) {
            // There's another grabber...
            grabberDisplacements[grabbers[0]].ResetOtherParent();
        }

        return true;
    }
    */
}
