using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CustomGrabbable : MonoBehaviour
{

    #region External References
    public List<Transform> snapTransforms;
    private Rigidbody m_RigidBody;
    #endregion

    #region Private Variables
    private Dictionary<Transform, bool> snapTransformsTaken = new Dictionary<Transform, bool>();
    private List<CustomGrabber> grabbers = new List<CustomGrabber>();
    private Dictionary<CustomGrabber, Transform> heldBy = new Dictionary<CustomGrabber, Transform>();
    private bool m_grabbedKinematic;
    private CustomGrabber m_GrabbedBy = null;
    private Transform originalParent = null;
    private int numGrabbersAllowed = 1;
    #endregion

    private void Start() {
        m_RigidBody = this.GetComponent<Rigidbody>();
        m_grabbedKinematic = m_RigidBody.isKinematic;
        originalParent = transform.parent;
        numGrabbersAllowed = (snapTransforms.Count > 0) ? snapTransforms.Count : 1;
        foreach(Transform t in snapTransforms) {    snapTransformsTaken.Add(t, false);  }
        StartCoroutine(SetParent());
    }
    private IEnumerator SetParent() {
        while(true) {
            if (grabbers.Count == 0) {  transform.SetParent(originalParent);    }
            else {
                transform.SetParent(grabbers[0].gameObject.transform);
                if (grabbers[0].shouldSnap) {   SnapToTransform();  }
            }
            yield return null;
        }
    }

    public void GrabBegin(CustomGrabber g) {
        if (grabbers.Count >= numGrabbersAllowed) {  return; }
        m_RigidBody.isKinematic = true;
        m_RigidBody.velocity = Vector3.zero;
        m_RigidBody.angularVelocity = Vector3.zero;

        Transform snapTo = null;
        if (snapTransforms.Count > 0) { snapTo = FindClosestSnapTransform(g);   }
        AddGrabber(g, snapTo);
    }
    public void GrabEnd(CustomGrabber cg, Vector3 linearVelocity, Vector3 angularVelocity) {
        RemoveController(cg);
        if (grabbers.Count == 0) {
            m_RigidBody.isKinematic = m_grabbedKinematic;
            m_RigidBody.velocity = linearVelocity;
            m_RigidBody.angularVelocity = angularVelocity;
        }
    }
    public void SnapToTransform() {
        Transform snapReference = heldBy[grabbers[0]];
        if (snapReference == null) {
            snapReference = this.transform;
            transform.rotation = grabbers[0].grabVol.transform.rotation * Quaternion.Euler(45,0,0);
        } else {
            transform.rotation = (grabbers[0].grabVol.transform.rotation * Quaternion.Inverse(snapReference.localRotation)) * Quaternion.Euler(45,0,0);
        }
        transform.position = grabbers[0].grabVol.transform.position + (transform.position - snapReference.position);
    }
    public void AddGrabber(CustomGrabber cg, Transform to) {
        grabbers.Add(cg);
        heldBy.Add(cg, to);
        if (to != null) {   snapTransformsTaken[to] = true; }
        if (grabbers.Count == 1 && grabbers[0].shouldSnap) {    SnapToTransform();  }
    }
    public void RemoveController(CustomGrabber cg) {
        Transform t = null;
        if (heldBy.TryGetValue(cg, out t)) {
            if (t != null) {    snapTransformsTaken[t] = false; }
        }
        heldBy.Remove(cg);
        int grabberIndex = grabbers.IndexOf(cg);
        if (grabberIndex > -1) {    grabbers.RemoveAt(grabberIndex);    }
        if (grabbers.Count > 0) {  SnapToTransform();   }
    }
    public List<OVRInput.Controller> GetGrabbers() {
        List<OVRInput.Controller> toReturn = new List<OVRInput.Controller>();
        foreach(CustomGrabber cg in grabbers) { toReturn.Add(cg.GetController());   }
        return toReturn;
    }
    public OVRInput.Controller GetGrabber() {
        return grabbers[0].GetController();
    }
    public bool CanBeGrabbed() {
        return numGrabbersAllowed - grabbers.Count > 0;
    }

    private Transform FindClosestSnapTransform(CustomGrabber cg) {
        Transform closest = null;
        float distance = 0f;
        foreach(Transform t in snapTransforms) {
            if (snapTransformsTaken[t]) {   continue;   }
            if (closest == null) {
                closest = t;
                distance = Vector3.Distance(cg.grabVol.transform.position, t.position);
            }
            float tempDist = Vector3.Distance(cg.grabVol.transform.position, t.position);
            if (tempDist < distance) {
                closest = t;
                distance = tempDist;
            }
        }
        return closest;
    }

}
