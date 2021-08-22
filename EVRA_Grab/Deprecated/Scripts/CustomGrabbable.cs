using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Limits {
    public bool x;
    public bool y;
    public bool z;
    public Limits(bool x, bool y, bool z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
public class CustomGrabbable : MonoBehaviour
{
    private Rigidbody m_rigidbody;
    private ConfigurableJoint m_joint;
    [SerializeField] [Tooltip("Reference to a debug text")]
    private TextMeshProUGUI m_debugText;

    [SerializeField] [Tooltip("How many grabbers can we attach to this?")]
    private int m_numGrabbers = 2;
    private List<CustomGrabber> m_grabbers = new List<CustomGrabber>();
    private GameObject destinationObject = null;
    public bool isGrabbed {
        get {   return amIGrabbed(); }
        set {}
    }

    [Tooltip("Translation and Rotation Limits")]
    public Limits  m_motionLimits = new Limits(false, false, false), 
                    m_rotationLimits = new Limits(false, false, false);

    [SerializeField] [Tooltip("Should we set our Rigidbody to Kinematic upon being grabbed?")]
    private bool m_shouldBeKinematic = true;
    [SerializeField] [Tooltip("Should we detect collisions upon being grabbed?")]
    private bool m_shouldDetectCollisions = false;
    private bool m_originalKinematicSetting = false;
    private bool m_originalDetectCollisions = false;
    [SerializeField] [Tooltip("Should we snap to parent?")]
    private bool m_shouldSnap = false;
    [SerializeField] [Tooltip("If any snap transformations are provided, the snapping will be relegated to the closest snap position to the fakeParent")]
    private List<Transform> m_snapTransforms = new List<Transform>();
    private Transform m_snapReference;

    private Vector3 m_positionOriginalValues = new Vector3(0f,0f,0f);
    private Vector3 m_rotationOriginalValues = new Vector3(0f,0f,0f);

    private void Awake() {
        m_rigidbody = this.GetComponent<Rigidbody>();
        if (m_rigidbody != null) {
            m_originalKinematicSetting = m_rigidbody.isKinematic;
            m_originalDetectCollisions = m_rigidbody.detectCollisions;
        }
        m_joint = this.GetComponent<ConfigurableJoint>();
        if (m_joint != null) {
            // Have to set limits based on configuration joint
            m_motionLimits.x = m_joint.xMotion.ToString() == "Locked";
            m_motionLimits.y = m_joint.yMotion.ToString() == "Locked";
            m_motionLimits.z = m_joint.zMotion.ToString() == "Locked";
            m_rotationLimits.x = m_joint.angularXMotion.ToString() == "Locked";
            m_rotationLimits.y = m_joint.angularYMotion.ToString() == "Locked";
            m_rotationLimits.z = m_joint.angularZMotion.ToString() == "Locked";
        }
    }
    private void FixedUpdate() {
        if (m_grabbers.Count == 0 || destinationObject == null) return;

        Vector3 posDestination = GetPositionDestination();
        Quaternion rotDestination = GetRotationDestination();

        if (m_rigidbody != null) {
            m_rigidbody.MovePosition(posDestination);
            //m_rigidbody.AddForce((objToSpawn.transform.position - transform.position), ForceMode.VelocityChange);
            m_rigidbody.MoveRotation(rotDestination);
            /*
            if (Vector3.Distance(objToSpawn.transform.position,transform.position) <= 0.05f) {
                m_rigidbody.velocity = Vector3.zero;
                transform.position = objToSpawn.transform.position;
            }
            */
        } else {
            transform.position = posDestination;
            transform.rotation = rotDestination;
        }
    }

    private Vector3 GetPositionDestination() {
        Transform snapReference = (m_snapReference != null) ? m_snapReference : this.transform;
        //float x = (m_motionLimits.x) ? m_positionOriginalValues.x : destinationObject.transform.position.x + (transform.position.x - snapReference.position.x);
        //float y = (m_motionLimits.y) ? m_positionOriginalValues.y : destinationObject.transform.position.y + (transform.position.y - snapReference.position.y);
        //float z = (m_motionLimits.z) ? m_positionOriginalValues.z : destinationObject.transform.position.z + (transform.position.z - snapReference.position.z);  
        //return new Vector3(x,y,z);
        Vector3 destination = destinationObject.transform.position + (transform.position - snapReference.position); 
        return destination;
    }
    private Quaternion GetRotationDestination() {
        Quaternion destinationRotation;
        if (m_snapReference != null) {
            //destinationRotation = (destinationObject.transform.rotation * Quaternion.Inverse(m_snapReference.localRotation)) * Quaternion.Euler(45,0,0);
            destinationRotation = (destinationObject.transform.rotation * Quaternion.Inverse(m_snapReference.localRotation));
        } else {
            //destinationRotation = destinationObject.transform.rotation * Quaternion.Euler(45,0,0);
            destinationRotation = destinationObject.transform.rotation;
        }
        return destinationRotation;
        /*
        x = (m_rotationLimits.x) ? m_rotationOriginalValues.x : destinationRotation.eulerAngles.x;
        y = (m_rotationLimits.y) ? m_rotationOriginalValues.y : destinationRotation.eulerAngles.y;
        z = (m_rotationLimits.z) ? m_rotationOriginalValues.z : destinationRotation.eulerAngles.z;
        return Quaternion.Euler(x,y,z);
        */
    }

    public bool GrabBegin(CustomGrabber cg) {
        if (m_debugText != null) m_debugText.text = "GRAB BEGAN";
        if (m_grabbers.Count >= m_numGrabbers || m_grabbers.Contains(cg)) return false;
        m_grabbers.Add(cg);
        if (m_rigidbody != null) {
            m_rigidbody.isKinematic = m_shouldBeKinematic;
            m_rigidbody.detectCollisions = m_shouldDetectCollisions;
        }
        if (m_grabbers.Count == 1) CreatePseudoParent();
        return true;
    }
    public void GrabEnd(CustomGrabber cg, Vector3 linearVelocity, Vector3 angularVelocity) {
        Destroy(destinationObject);
        destinationObject = null;
        m_grabbers.Remove(cg);
        if (m_grabbers.Count >= 1) CreatePseudoParent();
        else if (m_rigidbody != null) {
            m_rigidbody.isKinematic = m_originalKinematicSetting;
            m_rigidbody.detectCollisions = m_originalDetectCollisions;
            m_rigidbody.velocity = linearVelocity;
            m_rigidbody.angularVelocity = angularVelocity;
        }
        if (m_debugText != null) m_debugText.text = "GRAB ENDED";
        return;
    }

    private void CreatePseudoParent() {
        // Calculate the destination's position and rotation based on whether we should snap or not
        Vector3 destinationPosition;
        Quaternion destinationRotation;

        if (m_shouldSnap) {
            destinationPosition = m_grabbers[0].transform.position;
            destinationRotation = m_grabbers[0].transform.rotation;
            m_snapReference = FindClosestSnapTransformation();
            //Transform snapReference = (m_snapReference != null) ? m_snapReference : this.transform;
            //m_positionOriginalValues = destinationPosition;
            //m_rotationOriginalValues = destinationRotation.eulerAngles;
            //m_positionOriginalValues = destinationPosition + (transform.position - snapReference.position);
            //m_rotationOriginalValues = (m_snapReference != null) ? (destinationRotation * Quaternion.Inverse(m_snapReference.localRotation)).eulerAngles : destinationRotation.eulerAngles;
        } else {
            destinationPosition = transform.position;
            destinationRotation = transform.rotation;
            m_snapReference = null;
            //m_positionOriginalValues = destinationPosition;
            //m_rotationOriginalValues = destinationRotation.eulerAngles;
        }

        destinationObject = new GameObject("Object In Hand Reference");
        destinationObject.transform.position = destinationPosition;
        destinationObject.transform.rotation = destinationRotation;
        destinationObject.transform.parent = m_grabbers[0].transform;
    }

    private Transform FindClosestSnapTransformation() {
        if (m_snapTransforms.Count == 0) return null;
        Transform closest = null;
        float distance = -1f;
        foreach(Transform t in m_snapTransforms) {
            float d = Vector3.Distance(m_grabbers[0].transform.position, t.position);
            if (distance == -1f || d < distance) {
                closest = t;
                distance = d;
            }
        }
        return closest;
    }

    public OVRInput.Controller GetGrabber() {
        if (m_grabbers.Count == 0) return OVRInput.Controller.None;
        return m_grabbers[0].OVRController;
    }
    public bool amIGrabbed() {
        return (m_grabbers.Count > 0 && destinationObject != null);
    }
}
