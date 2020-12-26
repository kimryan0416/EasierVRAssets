using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGrabber : MonoBehaviour
{

    [SerializeField]
    [Tooltip("Reference to '__ControllerAnchor' gameObject under '__HandAnchor'")]
    private Transform m_controllerAnchor;
    [SerializeField]
    [Tooltip("Reference to OVR controller this hand is attached to")]
    private OVRInput.Controller m_OVRController = OVRInput.Controller.None;
    public OVRInput.Controller OVRController {
        get {   return m_OVRController;    }
    }
    [SerializeField]
    [Tooltip("Reference to object that acts as object detection for the grabbing function")]
    private CustomGrabber_GrabVolume m_gripDetector;
    [SerializeField]
    [Tooltip("Reference to object that acts as object detection for the tooltip function")]
    private CustomGrabber_GrabVolume m_tooltipDetector;
    [SerializeField]
    [Tooltip("Reference to a Custom Pointer")]
    private CustomPointer m_pointer;
    public CustomPointer pointer{
        get {   return m_pointer;     }
        set {   m_pointer = value;    }
    }
    [SerializeField]
    [Tooltip("Reference to where objects to attach to when being grabbed")]
    private Transform m_grabDestination;
    public Transform grabDestination {
        get {   return m_grabDestination;   }
    }

    private enum grabType {
        Grip,
        Distance,
        Both
    }
    [SerializeField]
    [Tooltip("The grab type")]
    private grabType m_grabType = grabType.Grip;

    // NOT SERIALIZED
    [Tooltip("If we're holding something... we store a reference to it")]
    private CustomGrabbable m_grabbedObject = null;
    public CustomGrabbable grabbedObject {
        get {   return m_grabbedObject;     }
        set {   m_grabbedObject = value;    }
    }

    [SerializeField]
    [Tooltip("Reference to controller appearance")]
    private OVRControllerHelper m_OVRControllerHelper;
<<<<<<< HEAD

    [SerializeField]
    [Tooltip("Should grabbed objects snap to the hand?")]
    private bool m_shouldSnap = true;
    public bool shouldSnap {
        get {   return m_shouldSnap;    }
    }
=======

    // NOT SERIALIZED
    [Tooltip("A dictionary of all keys available")]
    private Dictionary<string, float> m_inputTimes = new Dictionary<string, float>();
    public Dictionary<string, float> inputTimes {
        get {   return m_inputTimes;    }
    }
    // NOT SERIALIZED
    [Tooltip("A dictionary of all key presses")]
    private Dictionary<string, bool> m_inputDowns = new Dictionary<string, bool>();
    public Dictionary<string, bool> inputDowns {
        get {   return m_inputDowns;    }
    }
    // NOT SERIALIZED
    [Tooltip("Storing thumbstick position of the current controller")]
    private Vector2 m_thumbDirection = Vector2.zero;
    public Vector2 thumbDirection {
        get {   return m_thumbDirection;    }
    }
    // NOT SERIALIZED
    [Tooltip("Storing the angle and distance from center of the joystick of the current controller")]
    Vector2 m_thumbAngle = Vector2.zero;
    public Vector2 thumbAngle {
        get {   return m_thumbAngle;    }
    }

    public bool shouldSnap = true;

    /*
    private enum grabAppearance {
        Never,
        Detecting,
        Holding,
        DetectAndHold,
        Always
    }
    [SerializeField]
    [Tooltip("Setting the appearance of the grabbing grab_vol")]
    private grabAppearance m_grabAppearance = grabAppearance.DetectAndHold;

    private enum debugType {
        None,
        Grab,
        Tooltip,
        Both
    }
    [SerializeField]
    [Tooltip("Choose your debug type")]
    private debugType m_debugType = debugType.None;
    */
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f

    // Start is called before the first frame update
    public void Init()
    {
        // Set the hand controller
        m_OVRControllerHelper.m_controller = m_OVRController;
<<<<<<< HEAD

        // Each hand comes with two detectors: a grip detector, and a tooltip detector
        // Their inclusion in the hand is totally optional. But a hand wouldn't really work if there wasn't at least one grab detection at some point...
        // A hand can still work without any detection for grip or tooltip... but that would seem odd, in a sense.
        if (m_gripDetector != null) {
            if (!m_gripDetector.shouldStartOnRun) m_gripDetector.Init(true);
            switch(m_OVRController) {
                case(OVRInput.Controller.LTouch):
                    CustomEvents.current.onLeftGripDown += GripDown;
                    CustomEvents.current.onLeftGripUp +=  GripUp;
                    break;
                case(OVRInput.Controller.RTouch):
                    CustomEvents.current.onRightGripDown += GripDown;
                    CustomEvents.current.onRightGripUp += GripUp;
                    break;
            }
        }
        if (m_tooltipDetector) {
            if(!m_tooltipDetector.shouldStartOnRun) m_tooltipDetector.Init(true);
        }

        // If a custom pointer is attached to this hand, we initialize it
        if (m_pointer != null) {
            m_pointer.Init(true, this);
            switch(m_OVRController) {
                case(OVRInput.Controller.LTouch):
                    // CustomEvents.current.onLeftTriggerDown += TriggerDown;
                    // CustomEvents.current.onLeftTriggerUp += TriggerUp;
                    CustomEvents.current.onLeftTriggerDown += m_pointer.LineOn;
                    CustomEvents.current.onLeftTriggerUp += m_pointer.LineOff;
                    break;
                case(OVRInput.Controller.RTouch):
                    // CustomEvents.current.onRightTriggerDown += TriggerDown;
                    // CustomEvents.current.onRightTriggerUp += TriggerUp;
                    CustomEvents.current.onRightTriggerDown += m_pointer.LineOn;
                    CustomEvents.current.onRightTriggerUp += m_pointer.LineOff;
                    break;
=======

        // Each hand comes with two detectors: a grip detector, and a tooltip detector
        // Their inclusion in the hand is totally optional. But a hand wouldn't really work if there wasn't at least one grab detection at some point...
        // A hand can still work without any detection for grip or tooltip... but that would seem odd, in a sense.
        if (m_gripDetector && !m_gripDetector.shouldStartOnRun) m_gripDetector.Init(true);
        if (m_tooltipDetector && !m_tooltipDetector.shouldStartOnRun) m_tooltipDetector.Init(true);

        // If a custom pointer is attached to this hand, we initialize it
        if (m_pointer != null) m_pointer.Init(true);

        // This script also checks key presses and button inputs for the controller
        // For reference, One = A/X, Two = B/Y
        // These values are updated in Update() rather than a coroutine to keep consistent with other scripts running in Update
        m_inputTimes.Add("index",       -1f);
        m_inputTimes.Add("grip",        -1f);
        m_inputTimes.Add("one",         -1f);
        m_inputTimes.Add("two",         -1f);
        m_inputTimes.Add("thumbDir",    -1f);
        m_inputTimes.Add("thumbPress",  -1f);

        m_inputDowns.Add("index",       false);
        m_inputDowns.Add("grip",        false);
        m_inputDowns.Add("one",         false);
        m_inputDowns.Add("two",         false);
        m_inputDowns.Add("thumbNorth",  false);
        m_inputDowns.Add("thumbSouth",  false);
        m_inputDowns.Add("thumbEast",   false);
        m_inputDowns.Add("thumbWest",   false);
        m_inputDowns.Add("thumbPress",  false);

        m_thumbDirection = Vector2.zero;
        m_thumbAngle = Vector2.zero;

        /*
        if (debugToggle && m_gripTrans != m_controllerAnchor) {
            m_gripTrans.gameObject.SetActive(true);
        }
        m_OVRControllerHelper.m_controller = m_controller;
        StartCoroutine(CheckGrip());
        */
    }

    private void Update() {
        // Update input detection
        CheckInputs();
        // if trigger is held down, check pointer
        CheckPointer();
        // If grip is held down, check for close objects of grip and whatnot
        CheckGrip();
    }

    private void CheckInputs() {
        m_inputTimes["index"] =         (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, m_OVRController)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, m_OVRController)) ? -1f : m_inputTimes["index"];
        m_inputTimes["grip"] =          (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, m_OVRController)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, m_OVRController)) ? -1f : m_inputTimes["grip"];
        m_inputTimes["one"] =           (OVRInput.GetDown(OVRInput.Button.One, m_OVRController)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.One, m_OVRController)) ? -1f : m_inputTimes["one"];
        m_inputTimes["two"] =           (OVRInput.GetDown(OVRInput.Button.Two, m_OVRController)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.Two, m_OVRController)) ? -1f : m_inputTimes["two"];
        m_inputTimes["thumbDir"] =      (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_OVRController) != Vector2.zero) ? (m_inputTimes["thumbDir"] == -1f) ? Time.time : m_inputTimes["thumbDir"] : -1f;
        m_inputTimes["thumbPress"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, m_OVRController)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, m_OVRController)) ? -1f : m_inputTimes["thumbPress"];
        
        m_inputDowns["index"] =         OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, m_OVRController);
        m_inputDowns["grip"] =          OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, m_OVRController);
        m_inputDowns["one"] =           OVRInput.GetDown(OVRInput.Button.One, m_OVRController);
        m_inputDowns["two"] =           OVRInput.GetDown(OVRInput.Button.Two, m_OVRController);
        m_inputDowns["thumbNorth"] =    OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, m_OVRController);
        m_inputDowns["thumbSouth"] =    OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, m_OVRController);
        m_inputDowns["thumbEast"] =     OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, m_OVRController);
        m_inputDowns["thumbWest"] =     OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, m_OVRController);
        m_inputDowns["thumbPress"] =    OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, m_OVRController);

        m_thumbDirection = Vector2.ClampMagnitude(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_OVRController), 1f);
        m_thumbAngle = new Vector2(CommonFunctions.GetAngleFromVector2(m_thumbDirection,m_OVRController), Vector2.Distance(Vector2.zero, m_thumbDirection));

        return;
    }

    private void CheckPointer() {
        // If our pointer is null, no point in continuing
        if (m_pointer == null) return;

        // Adjust the state of the line renderer by trigger grip
        m_pointer.LineSet(m_inputTimes["index"] != -1f);

        // End with return;
        return;
    }

    private void CheckGrip() {
        // If our grip detection is null, no point in continuing
        if ((m_grabType == grabType.Grip || m_grabType == grabType.Both) && m_gripDetector == null) return;

        // Grabbing is somewhat dependent on the grab type we've picked for ourselves
        // if only grip, we check only the grip stuff
        // if only distance, we have to check if our line is actually hitting anything
        // if both, we check both - we prioritize the grip in this case

        // Detect the state of the button - if down and we're not grabbing anything, gotta do the deed
        if (m_inputDowns["grip"] && m_grabbedObject == null) {

            Transform closest = null;
            
            switch(m_grabType) {
                case(grabType.Grip):
                    // If grip, we grab from our grip detector
                    closest = m_gripDetector.closestInRange;
                    break;
                case(grabType.Distance):
                    closest = (m_pointer.raycastTarget != null && m_pointer.raycastTarget.GetComponent<CustomGrabbable>()) ? m_pointer.raycastTarget.transform : null;
                    break;
                case(grabType.Both):
                    // If grip, we grab from our grip detector
                    closest = m_gripDetector.closestInRange;
                    // closest is updated based on if it's already not null or not - if it is still null, we check the pointer
                    closest = (closest != null) ? closest : (m_pointer.raycastTarget != null && m_pointer.raycastTarget.GetComponent<CustomGrabbable>()) ? m_pointer.raycastTarget.transform : null;
                    break;
            }
            // If closest is not == null, we grab it!
            if (closest != null) GrabBegin(closest.GetComponent<CustomGrabbable>());
        }
        
        // If our inputTimes is actually -1 (aka we let go) but we're holding something, we gotta end the relationship... :P
        if (m_inputTimes["grip"] == -1f && m_grabbedObject != null) GrabEnd();

        // End with return
        return;
    }

    /*
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
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
            }
        }
    }

    private void GripDown(OVRInput.Controller c) {
        if (c == m_OVRController) {
             // Storing a possible reference
            Transform closest = null;
            
            // switch based on grab type
            switch(m_grabType) {
                case(grabType.Grip):
                    // If grip, we grab from our grip detector
                    closest = m_gripDetector.closestInRange;
                    break;
                case(grabType.Distance):
                    // We cannot use the teleport ponter for grabbing -_-
                    closest = (m_pointer.GetPointerType() != "Teleport" && m_pointer.raycastTarget != null && m_pointer.raycastTarget.GetComponent<CustomGrabbable>()) ? m_pointer.raycastTarget.transform : null;
                    break;
                case(grabType.Both):
                    // If grip, we grab from our grip detector
                    closest = m_gripDetector.closestInRange;
                    // closest is updated based on if it's already not null or not - if it is still null, we check the pointer
                    closest = (closest != null) ? closest : (m_pointer.GetPointerType() != "Teleport" && m_pointer.raycastTarget != null && m_pointer.raycastTarget.GetComponent<CustomGrabbable>()) ? m_pointer.raycastTarget.transform : null;
                    break;
            }
            // If closest is not == null, we grab it!
            if (closest != null && m_grabbedObject == null) GrabBegin(closest.GetComponent<CustomGrabbable>());
        }
    }

    private void GripUp(OVRInput.Controller c) {
        if (c == m_OVRController && m_grabbedObject != null) GrabEnd();
    }

    private void TriggerDown(OVRInput.Controller c) {
        if (c == m_OVRController) m_pointer.LineSet(true);
    }
    private void TriggerUp(OVRInput.Controller c) {
        if (c == m_OVRController) m_pointer.LineSet(false);
    }
    */

    private void GrabBegin(CustomGrabbable c) {
        c.GrabBegin(this);
        m_grabbedObject = c;
    }

    private void GrabEnd() {        
        OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(m_OVRController), orientation = OVRInput.GetLocalControllerRotation(m_OVRController) };

		OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
		Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_OVRController);
		Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_OVRController);
        angularVelocity *= -1;
        
        m_grabbedObject.GrabEnd(this, linearVelocity, angularVelocity);
        m_grabbedObject = null;
    }
}
