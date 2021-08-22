using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CustomGrabbable_Old : MonoBehaviour
{

    #region External References
    public List<Transform> snapTransforms;
    private Rigidbody m_RigidBody;
    [SerializeField] [Tooltip("Debug text mesh pro")]
    private TextMeshProUGUI m_debugText;
    #endregion

    #region Public Variables
    public bool shouldSetParent = true;
    // If shouldSetParent == true, the bottom six are no longer valid
    /*
    public bool restrictTranslateX = false, restrictTranslateY = false, restrictTranslateZ = false,
                restrictRotationX = false, restrictRotationY = false, restrictRotationZ = false;
    */
    #endregion

    #region Private Variables
    private Dictionary<Transform, bool> snapTransformsTaken = new Dictionary<Transform, bool>();
    private List<CustomGrabber> grabbers = new List<CustomGrabber>();
    public bool isGrabbed {
        get {   return grabbers.Count > 0;  }
        set {}
    }
    private Dictionary<CustomGrabber, Transform> heldBy = new Dictionary<CustomGrabber, Transform>();
    private bool m_grabbedKinematic;
    private Transform originalParent = null;
    private Vector3 originalPosition = Vector3.zero;
    private int numGrabbersAllowed = 2;


    private int m_originalLayer;
    private Dictionary<int, int> m_childrenOriginalLayers = new Dictionary<int, int>();
    private IEnumerator SnapToTransformProcess;
    private bool isSnapped = false, isSnapping = false;
    private Vector3 m_velocity = Vector3.zero;
    private float m_rotationTime = 0f;


    private CustomGrabber m_leftHand = null, m_rightHand = null;
    private bool leftFirst = false;
    private Vector3 l0 = Vector3.zero,
                    r0 = Vector3.zero,
                    h0 = Vector3.zero,
                    l1 = Vector3.zero,
                    r1 = Vector3.zero,
                    h1 = Vector3.zero,
                    posDiff = Vector3.zero;
    private Quaternion rot = Quaternion.identity,
                    rotDiff = Quaternion.identity;
    private CustomGrabber currentGrabber;
    #endregion

    private void Start() {
        Init();
    }
    
    public void Init() {
        m_RigidBody = this.GetComponent<Rigidbody>();
        m_grabbedKinematic = m_RigidBody.isKinematic;
        originalParent = transform.parent;
        //if (m_debugText) m_debugText.text = (originalParent == null).ToString();
        originalPosition = transform.position;
        numGrabbersAllowed = (snapTransforms.Count > 0) ? snapTransforms.Count : 2;
        m_originalLayer = this.gameObject.layer;
        Transform[] children = this.GetComponentsInChildren<Transform>();
        foreach (Transform c in children) {
            m_childrenOriginalLayers.Add(c.gameObject.GetInstanceID(), c.gameObject.layer);
        }
        foreach(Transform t in snapTransforms) {    snapTransformsTaken.Add(t, false);  }
        SnapToTransformProcess = SnapToTransform();
        //StartCoroutine(SetParent());
    }

    /*
    private IEnumerator SetParent() {
        while(true) {
            if (grabbers.Count == 0) {  transform.SetParent(originalParent);    }
            else {
                transform.SetParent(grabbers[0].gameObject.transform);
                if (grabbers[0].shouldSnap && !isSnapped && !isSnapping) {   
                    StartCoroutine(SnapToTransformProcess);
                    // SnapToTransform();  
                }
            }
            yield return null;
        }
    }
    */
    private void Update () {
        //if (m_debugText) m_debugText.text = grabbers.Count.ToString() + " | " + heldBy.Keys.Count.ToString() + " | " + snapTransformsTaken.Keys.Count.ToString();
        //if (m_debugText) m_debugText.text = this.transform.parent.name;
        //if (m_debugText) m_debugText.text = (m_leftHand != null).ToString() + " | " + (m_rightHand != null).ToString();
        if (m_debugText) m_debugText.text = posDiff.ToString() + " | " + rotDiff.ToString();

        //if (grabbers.Count == 0) return;
        if (m_leftHand == null && m_rightHand == null) return;

        // If held by two hands, we rotate using some unique mumbo-jumbo
        if (m_leftHand != null && m_rightHand != null) {

            // Handle Rotation with both hands
            l1 = m_leftHand.transform.position;
            r1 = m_rightHand.transform.position;
            h1 = currentGrabber.transform.position + Vector3.forward;

            Vector3 handDir0 = (r0 - l0).normalized; // the hand direction when the object was grabbed
            Vector3 handDir1 = (r1 - l1).normalized; // the current hand direction
            Quaternion handRot = Quaternion.FromToRotation(handDir0, handDir1) * Quaternion.FromToRotation(h0, h1);

            transform.rotation = handRot * rot;

            // Handle Transformation with both hands
            /*
            Vector3 destination = currentGrabber.grabDestination.position + (transform.position - this.transform.position);
            Vector3 frameDestination = destination;
            float distanceToDestination = Vector3.Distance(transform.position, destination);
            isSnapped = distanceToDestination <= 0.01f;
            if (currentGrabber.shouldSnap && !isSnapped) {
                frameDestination = Vector3.SmoothDamp(transform.position, destination, ref m_velocity, Time.deltaTime, 2f);
                distanceToDestination = Vector3.Distance(transform.position, destination);
            }
            transform.position = frameDestination;
            */
        } else {
            // Handle Rotaiton with Single Hand
            //if (m_debugText) m_debugText.text = "One Hand Grabbing";

             // We have 2x2 scenarios:
            //  Scenario A and B: Our hand either requries snapping or doesn't
            //  Scenario 1 and 2: Our object has a snap transform or not
            //  A1: Hand requires snapping, our object has snap transform => we snap to hand, our object matches the snap
            //  A2: Hand requires snapping, our object doesn't have a snap transform => we snap to hand, but our object snaps in the middle
            //  B1: Hand doesn't require snapping, our object has snap transform => we don't snap to hand, object having snap doesn't matter
            //  B2: Hand doesn't require snapping, our object doesn't have a snap transform => we don't snap to hand, object having snap doesn't matter
            /*
            if (currentGrabber.shouldSnap) {
                // Scenario A
                //Pre-setup
                Quaternion rotationDestination, rotationDifference;
                Transform snapReference = heldBy[currentGrabber];
                if (snapReference == null) {
                    snapReference = this.transform;
                    rotationDestination = currentGrabber.grabDestination.rotation * Quaternion.Euler(45,0,0);
                } else {
                    rotationDestination = (currentGrabber.grabDestination.rotation * Quaternion.Inverse(snapReference.localRotation)) * Quaternion.Euler(45,0,0);
                }
                //rotationDestination = currentGrabber.grabDestination.rotation * Quaternion.Euler(45,0,0);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotationDestination, m_rotationTime);
                m_rotationTime += Time.deltaTime;

                // Handle Transformation with Single Hand
                Vector3 destination = currentGrabber.grabDestination.position + (transform.position - snapReference.position);
                Vector3 frameDestination = destination;
                float distanceToDestination = Vector3.Distance(transform.position, destination);
                isSnapped = isSnapped || distanceToDestination <= 0.01f;
                //if (m_debugText) m_debugText.text = distanceToDestination.ToString();
                if (!isSnapped) {
                    frameDestination = Vector3.SmoothDamp(transform.position, destination, ref m_velocity, Time.deltaTime, 2f);
                    distanceToDestination = Vector3.Distance(transform.position, destination);
                }
                transform.position = frameDestination;

            } else {
                // Scenario B
                // In Scenario B, we don't need to be considerate about any snap transforms in the chid. 
                // We just need to make the object move as if they were a child of our hand
                Vector3 targetPos = currentGrabber.grabDestination.position - posDiff;
                Quaternion targetRot = currentGrabber.grabDestination.rotation * rotDiff;
                transform.position = CommonFunctions.RotatePointAroundPivot(targetPos, currentGrabber.grabDestination.position, targetRot);
                //transform.position = currentGrabber.grabDestination.position;
                transform.rotation = targetRot;
                //transform.Translate(posDiff);
            }
            */
        }
        // Handle Transformations
        /*
        Vector3 targetPos = currentGrabber.grabDestination.position - posDiff;
        transform.position = targetPos;
        */

        /*
        CustomGrabber currentGrabber = grabbers[0];
        Transform snapReference = heldBy[currentGrabber];

        // Setting rotation
        Quaternion rotationDestination, rotationDifference;
        if (currentGrabber.shouldSnap && snapReference == null) {
            snapReference = this.transform;
            rotationDestination = currentGrabber.grabDestination.rotation * Quaternion.Euler(45,0,0);
        } else {
            rotationDestination = (currentGrabber.grabDestination.rotation * Quaternion.Inverse(snapReference.localRotation)) * Quaternion.Euler(45,0,0);
        }
        //rotationDifference = this.rotation * Quaternion.Inverse(rotationDestination);
        //if (currentGrabber.shouldSnap) {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotationDestination, m_rotationTime);
            m_rotationTime += Time.deltaTime;
        //} else {
        //    transform.rotation = rotationDestination;
        //    m_rotationTime = 1f;
        //}

        // Setting Position
        Vector3 destination = currentGrabber.grabDestination.position + (transform.position - snapReference.position);
        Vector3 frameDestination = destination;
        float distanceToDestination = Vector3.Distance(transform.position, destination);
        isSnapped = distanceToDestination <= 0.01f;
        if (currentGrabber.shouldSnap && !isSnapped) {
            frameDestination = Vector3.SmoothDamp(transform.position, destination, ref m_velocity, Time.deltaTime, 2f);
            distanceToDestination = Vector3.Distance(transform.position, destination);
        }
        /*
        float destinationX = (restrictTranslateX) ? originalPosition.x : frameDestination.x;
        float destinationY = (restrictTranslateY) ? originalPosition.y : frameDestination.y;
        float destinationZ = (restrictTranslateZ) ? originalPosition.z : frameDestination.z;
        frameDestination = new Vector3(destinationX, destinationY, destinationZ);
        */
        //transform.position = frameDestination;
    }

    public bool GrabBegin(CustomGrabber g) {

        if (m_debugText) m_debugText.text = "GRAB BEGIN";

        if (grabbers.Count >= numGrabbersAllowed) {  
            // end early if we're not allowed to add this grabber due to total num of grabbers being achieved
            return false; 
        }
        
        // Set rigidbody mechanics...
        m_RigidBody.isKinematic = true;
        m_RigidBody.velocity = Vector3.zero;
        m_RigidBody.angularVelocity = Vector3.zero;

        // Set parent to the hand
        if (shouldSetParent) transform.SetParent(g.transform, true);
        m_rotationTime = 0f;

        // Add the "AvoidHover" layer to this to prevent hover effect
        this.gameObject.layer = LayerMask.NameToLayer("AvoidHover");
        Transform[] children = this.GetComponentsInChildren<Transform>();
        foreach (Transform c in children) {
            c.gameObject.layer = LayerMask.NameToLayer("AvoidHover");
        }
        
        /*
        // snapTransforms refers to the snap transformations that are attached to this object.
        // If we have any snap transforms attached to this object, we need to find the closest one and orient the object to match the nsap transform
        Transform snapTo = null;
        if (snapTransforms.Count > 0) { snapTo = FindClosestSnapTransform(g);   }
        // We then continue with the add grabber script
        */

        Transform snapTo = null;
        if (snapTransforms.Count > 0) { snapTo = FindClosestSnapTransform(g);   }
        //AddGrabber(g, snapTo);
        AddGrabber(g, snapTo);

        // We return true to indicate that we can associate this object to being grabbed by whatever hand grabbed this.
        return true;
    }
    public void GrabEnd(CustomGrabber cg, Vector3 linearVelocity, Vector3 angularVelocity) {
        if (m_debugText) m_debugText.text = "GRAB END";

        RemoveController(cg);

        //if (m_debugText) m_debugText.text += " | RESETTING LAYERS";
        this.gameObject.layer = m_originalLayer;
        Transform[] children = this.GetComponentsInChildren<Transform>();
        foreach (Transform c in children) {
            //if (m_debugText) m_debugText.text += "Reset layer for " + c.name + " ";
            c.gameObject.layer = m_childrenOriginalLayers[c.gameObject.GetInstanceID()];
        }
        //if (m_debugText) m_debugText.text += " | " + (originalParent == null).ToString();
        if (shouldSetParent) {
            //if (m_debugText) m_debugText.text += " | RESETTING PARENT";
            transform.SetParent(originalParent, true);
        } 
        else {
            //if (m_debugText) m_debugText.text += " | NULL PARENT RESET";
        }
        if (grabbers.Count == 0) {
            m_RigidBody.isKinematic = m_grabbedKinematic;
            m_RigidBody.velocity = linearVelocity;
            m_RigidBody.angularVelocity = angularVelocity;
        }
    }
    public IEnumerator SnapToTransform() {
        /*
        isSnapping = true;
        Transform snapReference = heldBy[grabbers[0]];
        if (snapReference == null) {
            snapReference = this.transform;
            transform.rotation = grabbers[0].grabDestination.rotation * Quaternion.Euler(45,0,0);
        } else {
            transform.rotation = (grabbers[0].grabDestination.rotation * Quaternion.Inverse(snapReference.localRotation)) * Quaternion.Euler(45,0,0);
        }

        Vector3 destination = grabbers[0].grabDestination.position + (transform.position - snapReference.position);
        float distanceToDestination = Vector3.Distance(transform.position, destination);
        float timeToDestination = distanceToDestination / 2f; // 2m / sec * TIME = DISTANCE, so TIME = DISTANCE * (sec / 2m)
        Vector3 velocity = Vector3.zero;
        isSnapped = distanceToDestination > 0.01f;
        while (distanceToDestination > 0.01f && !isSnapped) {
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, timeToDestination);
            distanceToDestination = Vector3.Distance(transform.position, destination);
            yield return null;
        }
        transform.position = destination;
        isSnapping = false;
        isSnapped = true;
        */
        yield return null;
    }
    public void AddGrabber(CustomGrabber cg, Transform to) {
        /*
        // We add this grabber to our list of grabbers
        grabbers.Add(cg);
        heldBy.Add(cg, to);

        if (m_debugText) m_debugText.text = "Hand Being Added";

        // Set the hint0 since this is a new hand that's grabbing this. This is set whenever we grab or let go of an object.
        h0 = cg.transform.position + Vector3.forward;
        rot = this.transform.rotation;
        //posDiff = transform.InverseTransformPoint(transform.position) - transform.InverseTransformPoint(cg.grabDestination.position);
        posDiff = cg.grabDestination.position - transform.position;
        rotDiff = Quaternion.Inverse(cg.grabDestination.rotation * transform.rotation);
        currentGrabber = cg;

        // We need to assign the hands based on if they're left or right
        switch(cg.OVRController) {
            case (OVRInput.Controller.LTouch):
                m_leftHand = cg;        // Save hand to left hand reference
                //if (m_debugText) m_debugText.text = "Left Hand Set: " + (m_leftHand != null).ToString();
                l0 = m_leftHand.transform.position;     // Get the initial hand position, save it
                if (m_rightHand == null) {
                    // now we're in the situation that this is the first grabber touching and holding this object. We need to initially assign some variables.
                    // mainly that if we're not grabbing the right hand, then obviously we need our left hand as our r0 reference
                    r0 = l0;
                }
                break;
            case (OVRInput.Controller.RTouch):
                m_rightHand = cg;
                //if (m_debugText) m_debugText.text = "Right Hand Set: " + (m_rightHand != null).ToString();
                r0 = m_rightHand.transform.position;
                if (m_leftHand == null) {
                    l0 = r0;
                }
                break;
        }

        /*
        // heldBy refers to the mapping between grabber and snap transform, even if it's null (because we don't have any snaps available)
        heldBy.Add(cg, to);
        // If we are passing a snap transform, we reserve it within snapTransformsTaken
        if (to != null) {   snapTransformsTaken[to] = true; }
        */

        /*
        if (grabbers.Count >= 1 && grabbers[0].shouldSnap) {
            StopCoroutine(SnapToTransformProcess);
            StartCoroutine(SnapToTransformProcess);
            // SnapToTransform();  
        }
        */
    }
    public void RemoveController(CustomGrabber cg) {
        //if (m_debugText) m_debugText.text += " | Removing Controller";

        /*
        Transform t = null;
        if (heldBy.TryGetValue(cg, out t)) {
            if (t != null) {    snapTransformsTaken[t] = false; }
        }
        //StopCoroutine(SnapToTransformProcess);
        heldBy.Remove(cg);
        */

        /*
        rot = this.transform.rotation;
        heldBy.Remove(cg);

        switch(cg.OVRController) {
            case(OVRInput.Controller.LTouch):
                // We need to reset the l0 and h0
                m_leftHand = null;
                l0 = Vector3.zero;

                if (m_rightHand == null) {
                    h0 = Vector3.zero;
                    r0 = Vector3.zero;
                    posDiff = Vector3.zero;
                    rotDiff = Quaternion.identity;
                    currentGrabber = null;
                } else {
                    h0 = m_rightHand.transform.position + Vector3.forward;
                    l0 = m_rightHand.transform.position;
                    r0 = l0;
                    posDiff = m_rightHand.grabDestination.position - transform.position;
                    rotDiff = Quaternion.Inverse(m_rightHand.grabDestination.rotation * transform.rotation);
                    currentGrabber = m_rightHand;
                }
                break;
            case(OVRInput.Controller.RTouch):
                m_rightHand = null;
                r0 = Vector3.zero;
                if (m_leftHand == null) {
                    h0 = Vector3.zero;
                    l0 = Vector3.zero;
                    leftFirst = false;
                    posDiff = Vector3.zero;
                    rotDiff = Quaternion.identity;
                    currentGrabber = null;
                } else {
                    h0 = m_leftHand.transform.position + Vector3.forward;
                    r0 = m_leftHand.transform.position + Vector3.forward;
                    l0 = r0;
                    leftFirst = true;
                    posDiff = m_leftHand.grabDestination.position - transform.position;
                    rotDiff = Quaternion.Inverse(m_leftHand.grabDestination.rotation * transform.rotation);
                    currentGrabber = m_leftHand;
                }
                break;
        }
        //int grabberIndex = grabbers.IndexOf(cg);
        //if (grabberIndex > -1) {    grabbers.RemoveAt(grabberIndex);    }
        grabbers.Remove(cg);

        //if (m_debugText) m_debugText.text += " | Successfully Removed Controller";
        /*
        if (grabbers.Count > 0) {  
            StartCoroutine(SnapToTransformProcess);
            // SnapToTransform();   
        }
        */
    }
    public List<OVRInput.Controller> GetGrabbers() {
        List<OVRInput.Controller> toReturn = new List<OVRInput.Controller>();
        foreach(CustomGrabber cg in grabbers) { toReturn.Add(cg.OVRController);   }
        return toReturn;
    }
    public OVRInput.Controller GetGrabber() {
        if (grabbers.Count == 0) return OVRInput.Controller.None;
        return grabbers[0].OVRController;
    }
    public bool CanBeGrabbed() {
        return numGrabbersAllowed - grabbers.Count > 0;
    }

    private Transform FindClosestSnapTransform(CustomGrabber cg) {
        /*
        Transform closest = null;
        float distance = 0f;
        foreach(Transform t in snapTransforms) {
            if (snapTransformsTaken[t]) {   continue;   }
            if (closest == null) {
                closest = t;
                distance = Vector3.Distance(cg.grabDestination.position, t.position);
            }
            float tempDist = Vector3.Distance(cg.grabDestination.position, t.position);
            if (tempDist < distance) {
                closest = t;
                distance = tempDist;
            }
        }
        return closest;
        */
        return null;
    }

}
