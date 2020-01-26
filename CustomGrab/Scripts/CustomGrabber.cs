using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrabber : MonoBehaviour
{

    #region External References
    public Transform ControllerAnchor;
    public Transform m_gripTrans;
    public CustomGrabber_GrabVolume grabVol;
    public CustomOVRControllerHelper m_OVRControllerHelper;
    public CustomPointer m_CustomPointer;
    #endregion

    #region Public Variables
    public OVRInput.Controller m_controller;
    public bool shouldSnap = true;
    public bool debugToggle = false;
    #endregion

    #region Private Variables
    private CustomGrabbable m_grabbedObject;
    private List<GameObject> inRange = new List<GameObject>();
    #endregion

    // Start is called before the first frame update
    void Start()
    {

        if (m_gripTrans == null) m_gripTrans = ControllerAnchor;
        if (debugToggle && m_gripTrans != ControllerAnchor) {
            m_gripTrans.gameObject.SetActive(true);
        }
        if (m_CustomPointer != null) {  m_CustomPointer.Activate(); }
        m_OVRControllerHelper.m_controller = m_controller;
        StartCoroutine(CheckGrip());
    }

    public OVRInput.Controller GetController() {
        return m_controller;
    }

    private IEnumerator CheckGrip() {
        while(true) {
            //grabVol.GetComponent<Renderer>().enabled = (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller) > 0.1f);
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller) > 0.1f) {
                if (debugToggle) grabVol.GetComponent<Renderer>().enabled = true;
                // If the grip is being held down
                if (m_grabbedObject == null) {
                    // Check if any objects are in range
                    inRange = grabVol.GetInRange();
                    // Find Closest
                    GameObject closest = GetClosestInRange();
                    // If there is a closest, then we initialize the grab
                    if (closest != null) {
                        GrabBegin(closest.GetComponent<CustomGrabbable>());
                    }
                }
            } else {
                if (debugToggle) grabVol.GetComponent<Renderer>().enabled = false;
                if (m_grabbedObject != null) {
                    GrabEnd();
                }
            }

            if (m_grabbedObject != null) {
                m_CustomPointer.LineOff();
            }
            else if (m_CustomPointer != null) {
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller) > 0.1f) {
                    m_CustomPointer.LineOn();
                } else {
                    m_CustomPointer.LineOff();
                }
            }
            yield return null;
        }
    }

    private GameObject GetClosestInRange() {
        GameObject closest = null;
        float closestDistance = 0f;
        float d = 0f;
        foreach(GameObject cg in inRange) {
            if (!cg.GetComponent<CustomGrabbable>().CanBeGrabbed()) {   continue;   }
            d = Vector3.Distance(m_gripTrans.position, cg.transform.position);
            if (closest == null || d < closestDistance) {
                closest = cg;
                closestDistance = d;
            }
        }
        return closest;
    }

    private void GrabBegin(CustomGrabbable c) {
        c.GrabBegin(this);
        m_grabbedObject = c;
    }

    private void GrabEnd() {        
        // Imported from Oculus Implementations
        OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(m_controller), orientation = OVRInput.GetLocalControllerRotation(m_controller) };

		OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
		Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_controller);
		Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_controller);
        angularVelocity *= -1;
        
        m_grabbedObject.GrabEnd(this, linearVelocity, angularVelocity);
        m_grabbedObject = null;
    }
}
