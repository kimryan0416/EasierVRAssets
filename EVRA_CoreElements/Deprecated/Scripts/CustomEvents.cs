using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEvents : MonoBehaviour
{

    public static CustomEvents current;
    
    // NOT SERIALIZED
    [Tooltip("Saves when buttons were pressed down")]
    private Dictionary<string, float> m_inputTimes = new Dictionary<string, float>();
    // NOT SERIALIZED
    [Tooltip("Saves whether buttons were pushed down in the current update or not")]
    private Dictionary<string, float> m_inputDowns = new Dictionary<string, float>();
    // NOT SERIALIZED
    [Tooltip("Saves the joystick direction of each controller in a vector2 format")]
    private Dictionary<string, Vector2> m_thumbDirections = new Dictionary<string, Vector2>();
    // NOT SERIALIZED
    [Tooltip("Saves the angle of each joystick")]
    private Dictionary<string, Vector2> m_thumbAngles = new Dictionary<string, Vector2>();
    // NOT SERIALIZED
    [Tooltip("Links between strings and actual OVR inputs")]
    private Dictionary<string, OVRInput.Button> m_buttonMappings = new Dictionary<string, OVRInput.Button>();

    [Tooltip("Saving the pointer raycast target of each hand")]
    private Dictionary<string, GameObject> m_pointerTargets = new Dictionary<string, GameObject>();
    private Dictionary<string, bool> m_pointerTargetsChanged = new Dictionary<string, bool>();

    [Tooltip("Saving the grabbed object of each hand")]
    private Dictionary<string, CustomGrabbable> m_grabbedObjects = new Dictionary<string, CustomGrabbable>();
    private Dictionary<string, bool> m_grabbedObjectsChanged = new Dictionary<string, bool>();

    [SerializeField] [Tooltip("Hand references")]
    private CustomGrabber m_leftHand, m_rightHand;

    [SerializeField] [Tooltip("Track times when buttons are pressed down")]
    private bool m_trackingTime = false, m_trackingDown = true;
    [SerializeField] [Tooltip("Track pointer targets toggle")]
    private bool m_trackingPointerTargets = true;
    [SerializeField] [Tooltip("Track grabbed objects toggle")]
    private bool m_trackingGrabbedObjects = true;
    [SerializeField] [Tooltip("List of colliders that must be tracked")]
    private List<CustomGrabber_GrabVolume> m_colliders = new List<CustomGrabber_GrabVolume>();

    private void Awake() {
        current = this;
        if (m_leftHand != null) PrepareLeft(m_leftHand);
        if (m_rightHand != null) PrepareRight(m_rightHand);
    }

    private void Update() {
        if (m_trackingTime) CheckTimes();
        if (m_trackingDown) CheckDowns();
        if (m_trackingGrabbedObjects) CheckGrabbedObjects();
        if (m_trackingPointerTargets) CheckPointerTargets();
        UpdateEvents();
    }

    // Left controller prep - returns early 
    private void PrepareLeft(CustomGrabber g) {
        if (g == null) return;
        m_leftHand = g;
        m_leftHand.Init();
        if (m_trackingTime) {
            m_inputTimes.Add("left_index",       -1f);
            m_inputTimes.Add("left_grip",        -1f);
            m_inputTimes.Add("left_one",         -1f);
            m_inputTimes.Add("left_two",         -1f);
            m_inputTimes.Add("left_thumbDir",    -1f);
            m_inputTimes.Add("left_thumbPress",  -1f);
            m_inputTimes.Add("start",            -1f);
        }
        m_inputDowns.Add("left_index",       0f);
        m_inputDowns.Add("left_grip",        0f);
        m_inputDowns.Add("left_one",         0f);
        m_inputDowns.Add("left_two",         0f);
        // m_inputDowns.Add("left_thumbNorth",  0f);
        // m_inputDowns.Add("left_thumbSouth",  0f);
        // m_inputDowns.Add("left_thumbEast",   0f);
        // m_inputDowns.Add("left_thumbWest",   0f);
        m_inputDowns.Add("left_thumbPress",  0f);
        m_inputTimes.Add("start",            0f);
        
        // Left Joystick
        m_thumbDirections.Add("left",Vector2.zero);
        m_thumbAngles.Add("left",Vector2.zero);

        // Grab object
        m_grabbedObjects.Add("left",null);

        // If there's a pointer...
        if (m_leftHand.pointer != null) {
            m_pointerTargets.Add("left",null);
        }

    }
    // Right controller prep - returns early
    private void PrepareRight(CustomGrabber g) {
        if (g == null) return;
        m_rightHand = g;
        m_rightHand.Init();
        if (m_trackingTime) {
            m_inputTimes.Add("right_index",       -1f);
            m_inputTimes.Add("right_grip",        -1f);
            m_inputTimes.Add("right_one",         -1f);
            m_inputTimes.Add("right_two",         -1f);
            m_inputTimes.Add("right_thumbDir",    -1f);
            m_inputTimes.Add("right_thumbPress",  -1f);
        }
        m_inputDowns.Add("right_index",       0f);
        m_inputDowns.Add("right_grip",        0f);
        m_inputDowns.Add("right_one",         0f);
        m_inputDowns.Add("right_two",         0f);
        // m_inputDowns.Add("right_thumbNorth",  0f);
        // m_inputDowns.Add("right_thumbSouth",  0f);
        // m_inputDowns.Add("right_thumbEast",   0f);
        // m_inputDowns.Add("right_thumbWest",   0f);
        m_inputDowns.Add("right_thumbPress",  0f);
        // Right joystick
        m_thumbDirections.Add("right",Vector2.zero);
        m_thumbAngles.Add("right",Vector2.zero);

        // Grab object
        m_grabbedObjects.Add("right",null);

        // If there's a pointer...
        if (m_rightHand.pointer != null) {
            m_pointerTargets.Add("right",null);
        }
    }

    private void CheckTimes() {
        if (m_leftHand != null) CheckLeftTimes();
        if (m_rightHand != null) CheckRightTimes();
    }
    private void CheckLeftTimes() {
        m_inputTimes["left_index"]      =    (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))            ? Time.time     : (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) ? -1f : m_inputTimes["left_index"];
        m_inputTimes["left_grip"]       =    (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))             ? Time.time     : (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch)) ? -1f : m_inputTimes["left_grip"];
        m_inputTimes["left_one"]        =    (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))                            ? Time.time     : (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.LTouch)) ? -1f : m_inputTimes["left_one"];
        m_inputTimes["left_two"]        =    (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))                            ? Time.time     : (OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.LTouch)) ? -1f : m_inputTimes["left_two"];
        m_inputTimes["left_thumbDir"]   =    (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch) != Vector2.zero)  ? (m_inputTimes["left_thumbDir"] == -1f) ? Time.time : m_inputTimes["left_thumbDir"] : -1f;
        m_inputTimes["left_thumbPress"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))              ? Time.time     : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch)) ? -1f : m_inputTimes["left_thumbPress"];
        m_inputTimes["start"]           =    (OVRInput.GetDown(OVRInput.Button.Start))                                                      ? Time.time     : (OVRInput.GetUp(OVRInput.Button.Start)) ? -1f : m_inputTimes["start"];
        return;
    }
    private void CheckRightTimes() {
        m_inputTimes["right_index"]     =     (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) ? -1f : m_inputTimes["right_index"];
        m_inputTimes["right_grip"]      =     (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch)) ? -1f : m_inputTimes["right_grip"];
        m_inputTimes["right_one"]       =     (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch)) ? -1f : m_inputTimes["right_one"];
        m_inputTimes["right_two"]       =     (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.RTouch)) ? -1f : m_inputTimes["right_two"];
        m_inputTimes["right_thumbDir"]  =     (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch) != Vector2.zero) ? (m_inputTimes["right_thumbDir"] == -1f) ? Time.time : m_inputTimes["right_thumbDir"] : -1f;
        m_inputTimes["right_thumbPress"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch)) ? Time.time : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch)) ? -1f : m_inputTimes["right_thumbPress"];
        return;
    }


    private void CheckDowns() {
        if (m_leftHand != null) CheckLeftDown();
        if (m_rightHand != null) CheckRightDown();
    }
    private void CheckLeftDown() {
        m_inputDowns["left_index"]      =    (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))        ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))     ? -1f : 0f;
        m_inputDowns["left_grip"]       =    (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))         ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))      ? -1f : 0f;
        m_inputDowns["left_one"]        =    (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))                        ? 1f    : (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.LTouch))                     ? -1f : 0f;
        m_inputDowns["left_two"]        =    (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))                        ? 1f    : (OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.LTouch))                     ? -1f : 0f;
        // m_inputDowns["left_thumbNorth"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch))        ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch))     ? -1f : 0f;
        // m_inputDowns["left_thumbSouth"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.LTouch))      ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.LTouch))   ? -1f : 0f;
        // m_inputDowns["left_thumbEast"]  =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.LTouch))     ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.LTouch))  ? -1f : 0f;
        // m_inputDowns["left_thumbWest"]  =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.LTouch))      ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.LTouch))   ? -1f : 0f;
        m_inputDowns["left_thumbPress"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))          ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))       ? -1f : 0f;
        m_inputDowns["start"]           =    (OVRInput.GetDown(OVRInput.Button.Start))                                                  ? 1f    : (OVRInput.GetUp(OVRInput.Button.Start))                                               ? -1f : 0f;
        m_thumbDirections["left"]       = Vector2.ClampMagnitude(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch), 1f);
        m_thumbAngles["left"]           = new Vector2(CommonFunctions.GetAngleFromVector2(m_thumbDirections["left"], OVRInput.Controller.LTouch), Vector2.Distance(Vector2.zero, m_thumbDirections["left"]));
        return;
    }
    private void CheckRightDown() {
        m_inputDowns["right_index"]     =     (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))       ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))     ? -1f : 0f;
        m_inputDowns["right_grip"]      =     (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))        ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))      ? -1f : 0f;
        m_inputDowns["right_one"]       =     (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))                       ? 1f    : (OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch))                     ? -1f : 0f;
        m_inputDowns["right_two"]       =     (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))                       ? 1f    : (OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.RTouch))                     ? -1f : 0f;
        // m_inputDowns["right_thumbNorth"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.RTouch))       ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.RTouch))     ? -1f : 0f;
        // m_inputDowns["right_thumbSouth"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.RTouch))     ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.RTouch))   ? -1f : 0f;
        // m_inputDowns["right_thumbEast"] =     (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch))    ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.RTouch))  ? -1f : 0f;
        // m_inputDowns["right_thumbWest"] =     (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch))     ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.RTouch))   ? -1f : 0f;
        m_inputDowns["right_thumbPress"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))         ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))       ? -1f : 0f;
        m_thumbDirections["right"]      = Vector2.ClampMagnitude(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch), 1f);
        m_thumbAngles["right"]          = new Vector2(CommonFunctions.GetAngleFromVector2(m_thumbDirections["right"], OVRInput.Controller.RTouch), Vector2.Distance(Vector2.zero, m_thumbDirections["right"]));
        return;
    }

    private void CheckGrabbedObjects() {
        if (m_leftHand != null) CheckLeftGrabbedTargets();
        if (m_rightHand != null) CheckRightGrabbedTargets();
    }
    private void CheckLeftGrabbedTargets() {
        if (
            //(m_grabbedObjects["left"] == null && m_leftHand.grabbedObject != null) 
            //|| (m_grabbedObjects["left"] != null && m_leftHand.grabbedObject == null) 
            //|| (m_grabbedObjects["left"] != null && m_leftHand.grabbedObject != null && m_grabbedObjects["left"] != m_leftHand.grabbedObject)
            m_grabbedObjects["left"] != m_leftHand.grabbedObject
        ) {
            m_grabbedObjectsChanged["left"] = true;
        } else {
            m_grabbedObjectsChanged["left"] = false;
        }
        m_grabbedObjects["left"] = m_leftHand.grabbedObject;
    }
    private void CheckRightGrabbedTargets() {
        if (
            (m_grabbedObjects["right"] == null && m_rightHand.grabbedObject != null) ||
            (m_grabbedObjects["right"] != null && m_rightHand.grabbedObject == null) ||
            (m_grabbedObjects["right"] != null && m_rightHand.grabbedObject != null && m_grabbedObjects["right"].gameObject.GetInstanceID() != m_rightHand.grabbedObject.gameObject.GetInstanceID())
        ) {
            m_grabbedObjectsChanged["right"] = true;
        } else {
            m_grabbedObjectsChanged["right"] = false;
        }
        m_grabbedObjects["right"] = m_rightHand.grabbedObject;
    }

    private void CheckPointerTargets() {
        if (m_leftHand != null) CheckLeftPointerTargets();
        if (m_rightHand != null) CheckRightPointerTargets();
    }
    private void CheckLeftPointerTargets() {
        if (
            (m_pointerTargets["left"] == null && m_leftHand.pointer.raycastTarget != null) ||
            (m_pointerTargets["left"] != null && m_leftHand.pointer.raycastTarget == null) ||
            (m_pointerTargets["left"] != null && m_leftHand.pointer.raycastTarget != null && m_pointerTargets["left"].GetInstanceID() != m_leftHand.pointer.raycastTarget.GetInstanceID())
        ) {
            m_pointerTargetsChanged["left"] = true;
        } else {
            m_pointerTargetsChanged["left"] = false;
        }
        m_pointerTargets["left"] = m_leftHand.pointer.raycastTarget;
    }
    private void CheckRightPointerTargets() {
        if (
            (m_pointerTargets["right"] == null && m_rightHand.pointer.raycastTarget != null) ||
            (m_pointerTargets["right"] != null && m_rightHand.pointer.raycastTarget == null) ||
            (m_pointerTargets["right"] != null && m_rightHand.pointer.raycastTarget != null && m_pointerTargets["right"].GetInstanceID() != m_rightHand.pointer.raycastTarget.GetInstanceID())
        ) {
            m_pointerTargetsChanged["right"] = true;
        } else {
            m_pointerTargetsChanged["right"] = false;
        }
        m_pointerTargets["right"] = m_rightHand.pointer.raycastTarget;
    }

    private void UpdateEvents() {

        // Left grip
        if (m_inputDowns["left_grip"] == 1f) {
            current.LeftGripDown(OVRInput.Controller.LTouch); 
            current.GripDown(m_leftHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["left_grip"] == -1f) {  
            current.LeftGripUp(OVRInput.Controller.LTouch);
            current.GripUp(m_leftHand, OVRInput.Controller.None);
        }
        // Right Grip
        if (m_inputDowns["right_grip"] == 1f) {
            current.RightGripDown(OVRInput.Controller.RTouch);
            current.GripDown(m_rightHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["right_grip"] == -1f) {
            current.RightGripUp(OVRInput.Controller.RTouch);
            current.GripUp(m_rightHand, OVRInput.Controller.None);
        }

        // Left Index Trigger
        if (m_inputDowns["left_index"] == 1f) {
            current.LeftTriggerDown(OVRInput.Controller.LTouch);
            current.TriggerDown(m_leftHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["left_index"] == -1f) {
            current.LeftTriggerUp(OVRInput.Controller.LTouch);
            current.TriggerUp(m_leftHand, OVRInput.Controller.None);
        }
        // Right Index Trigger
        if (m_inputDowns["right_index"] == 1f) {
            current.RightTriggerDown(OVRInput.Controller.RTouch);
            current.TriggerDown(m_rightHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["right_index"] == -1f) {
            current.RightTriggerUp(OVRInput.Controller.RTouch);
            current.TriggerUp(m_rightHand, OVRInput.Controller.None);
        }

        // Left One Button
        if (m_inputDowns["left_one"] == 1f) {
            current.LeftOneDown(OVRInput.Controller.LTouch);
            current.OneDown(m_leftHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["left_one"] == -1f) {
            current.LeftOneUp(OVRInput.Controller.None);
            current.OneUp(m_leftHand, OVRInput.Controller.None);
        }
        // Right One Button
        if (m_inputDowns["right_one"] == 1f) {
            current.RightOneDown(OVRInput.Controller.RTouch);
            current.OneDown(m_rightHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["right_one"] == -1f) {
            current.RightOneUp(OVRInput.Controller.RTouch);
            current.OneUp(m_rightHand, OVRInput.Controller.None);
        }

        // Left Two Button
        if (m_inputDowns["left_two"] == 1f) {
            current.LeftTwoDown(OVRInput.Controller.LTouch);
            current.TwoDown(m_leftHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["left_two"] == -1f) {
            current.LeftTwoUp(OVRInput.Controller.None);
            current.TwoUp(m_leftHand, OVRInput.Controller.None);
        }
        // Right Two Button
        if (m_inputDowns["right_two"] == 1f) {
            current.RightTwoDown(OVRInput.Controller.RTouch);
            current.TwoDown(m_rightHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["right_two"] == -1f) {
            current.RightTwoUp(OVRInput.Controller.RTouch);
            current.TwoUp(m_rightHand, OVRInput.Controller.None);
        }

        // Left Thumbstick Press Button
        if (m_inputDowns["left_thumbPress"] == 1f) {
            current.LeftThumbPress(OVRInput.Controller.LTouch);
            current.ThumbPress(m_leftHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["left_thumbPress"] == -1f) {
            current.LeftThumbRelease(OVRInput.Controller.None);
            current.ThumbRelease(m_leftHand, OVRInput.Controller.None);
        }
        // Right Thumbstick Press Button
        if (m_inputDowns["right_thumbPress"] == 1f) {
            current.RightThumbPress(OVRInput.Controller.RTouch);
            current.ThumbPress(m_rightHand, OVRInput.Controller.None);
        }
        if (m_inputDowns["right_thumbPress"] == -1f) {
            current.RightThumbRelease(OVRInput.Controller.RTouch);
            current.ThumbRelease(m_rightHand, OVRInput.Controller.None);
        }

        // Left Start Button
        if (m_inputDowns["start"] == 1f) {
            current.StartDown();
        }
        if (m_inputDowns["start"] == -1f) {
            current.StartUp();
        }

        if (m_thumbDirections["left"] != Vector2.zero) {
            current.LeftThumbDirection(OVRInput.Controller.LTouch, m_thumbDirections["left"], m_thumbAngles["left"]);
        }
        if (m_thumbDirections["right"] != Vector2.zero) {
            current.RightThumbDirection(OVRInput.Controller.RTouch, m_thumbDirections["right"], m_thumbAngles["right"]);
        }

        // Grabbed Objects
        if (m_grabbedObjectsChanged["left"]) {
            current.LeftHandGrabbedChanged(OVRInput.Controller.LTouch, m_grabbedObjects["left"]);
            current.GrabbedChanged(m_leftHand, OVRInput.Controller.LTouch, m_grabbedObjects["left"]);
        }
        if (m_grabbedObjectsChanged["right"]) {
            current.RightHandGrabbedChanged(OVRInput.Controller.RTouch, m_grabbedObjects["right"]);
            current.GrabbedChanged(m_rightHand, OVRInput.Controller.LTouch, m_grabbedObjects["right"]);
        }

        // Pointer Targets
        if (m_pointerTargetsChanged["left"]) {
            current.LeftPointerTargetChanged(OVRInput.Controller.LTouch, m_pointerTargets["left"]);
        }
        if (m_pointerTargetsChanged["right"]) {
            current.RightPointerTargetchanged(OVRInput.Controller.RTouch, m_pointerTargets["right"]);
        }
    }

    // Events for grip
    public event Action<OVRInput.Controller> onLeftGripDown, onLeftGripUp, onRightGripDown, onRightGripUp;
    public event Action<CustomGrabber, OVRInput.Controller> onGripDown, onGripUp;
    public void LeftGripDown(OVRInput.Controller c) {
        onLeftGripDown?.Invoke(c);
    }
    public void LeftGripUp(OVRInput.Controller c) {
        onLeftGripUp?.Invoke(c);
    }
    public void RightGripDown(OVRInput.Controller c) {
        onRightGripDown?.Invoke(c);
    }
    public void RightGripUp(OVRInput.Controller c) {
        onRightGripUp?.Invoke(c);
    }
    public void GripDown(CustomGrabber cg, OVRInput.Controller c) {
        onGripDown?.Invoke(cg, c);
    }
    public void GripUp(CustomGrabber cg, OVRInput.Controller c) {
        onGripUp?.Invoke(cg, c);
    }

    public event Action<OVRInput.Controller> onLeftTriggerDown, onLeftTriggerUp, onRightTriggerDown, onRightTriggerUp;
    public event Action<CustomGrabber, OVRInput.Controller> onTriggerDown, onTriggerUp;
    public void LeftTriggerDown(OVRInput.Controller c) {
        onLeftTriggerDown?.Invoke(c);
    }
    public void LeftTriggerUp(OVRInput.Controller c) {
        onLeftTriggerUp?.Invoke(c);
    }
    public void RightTriggerDown(OVRInput.Controller c) {
        onRightTriggerDown?.Invoke(c);
    }
    public void RightTriggerUp(OVRInput.Controller c) {
        onRightTriggerUp?.Invoke(c);
    }
    public void TriggerDown(CustomGrabber cg, OVRInput.Controller c) {
        onTriggerDown?.Invoke(cg, c);
    }
    public void TriggerUp(CustomGrabber cg, OVRInput.Controller c) {
        onTriggerUp?.Invoke(cg, c);
    }

    public event Action<OVRInput.Controller> onLeftOneDown, onLeftOneUp, onRightOneDown, onRightOneUp;
    public event Action<CustomGrabber, OVRInput.Controller> onOneDown, onOneUp;
    public void LeftOneDown(OVRInput.Controller c) {
        onLeftOneDown?.Invoke(c);
    }
    public void LeftOneUp(OVRInput.Controller c) {
        onLeftOneUp?.Invoke(c);
    }
    public void RightOneDown(OVRInput.Controller c) {
        onRightOneDown?.Invoke(c);
    }
    public void RightOneUp(OVRInput.Controller c) {
        onRightOneUp?.Invoke(c);
    }
    public void OneDown(CustomGrabber cg, OVRInput.Controller c) {
        onOneDown?.Invoke(cg, c);
    }
    public void OneUp(CustomGrabber cg, OVRInput.Controller c) {
        onOneUp?.Invoke(cg, c);
    }

    public event Action<OVRInput.Controller> onLeftTwoDown, onLeftTwoUp, onRightTwoDown, onRightTwoUp;
    public event Action<CustomGrabber, OVRInput.Controller> onTwoDown, onTwoUp;
    public void LeftTwoDown(OVRInput.Controller c) {
        onLeftTwoDown?.Invoke(c);
    }
    public void LeftTwoUp(OVRInput.Controller c) {
        onLeftTwoUp?.Invoke(c);
    }
    public void RightTwoDown(OVRInput.Controller c) {
        onRightTwoDown?.Invoke(c);
    }
    public void RightTwoUp(OVRInput.Controller c) {
        onRightTwoUp?.Invoke(c);
    }
    public void TwoDown(CustomGrabber cg, OVRInput.Controller c) {
        onTwoDown?.Invoke(cg, c);
    }
    public void TwoUp(CustomGrabber cg, OVRInput.Controller c) {
        onTwoUp?.Invoke(cg, c);
    }

    public event Action<OVRInput.Controller> onLeftThumbPress, onLeftThumbRelease, onRightThumbPress, onRightThumbRelease;
    public event Action<CustomGrabber, OVRInput.Controller> onThumbPress, onThumbRelease;
    public void LeftThumbPress(OVRInput.Controller c) {
        onLeftThumbPress?.Invoke(c);
    }
    public void LeftThumbRelease(OVRInput.Controller c) {
        onLeftThumbRelease?.Invoke(c);
    }
    public void RightThumbPress(OVRInput.Controller c) {
        onRightThumbPress?.Invoke(c);
    }
    public void RightThumbRelease(OVRInput.Controller c) {
        onRightThumbRelease?.Invoke(c);
    }
    public void ThumbPress(CustomGrabber cg, OVRInput.Controller c) {
        onThumbPress?.Invoke(cg, c);
    }
    public void ThumbRelease(CustomGrabber cg, OVRInput.Controller c) {
        onThumbRelease?.Invoke(cg, c);
    }

    public event Action onStartDown, onStartUp;
    public void StartDown() {
        onStartDown?.Invoke();
    }
    public void StartUp() {
        onStartUp?.Invoke();
    }

    public event Action<OVRInput.Controller, Vector2, Vector2> onLeftThumbDirection, onRightThumbDirection;
    public void LeftThumbDirection(OVRInput.Controller c, Vector2 d, Vector2 a) {
        onLeftThumbDirection?.Invoke(c, d, a);
    }
    public void RightThumbDirection(OVRInput.Controller c, Vector2 d, Vector2 a) {
        onRightThumbDirection?.Invoke(c, d, a);
    }

    public event Action<Collider, GameObject> onCollisionEnter, onCollisionExit, onTriggerEnter, onTriggerExit;
    public void CollisionEnter(Collider collidedWith, GameObject obj) {
        onCollisionEnter?.Invoke(collidedWith, obj);
    }
    public void CollisionExit(Collider collidedWith, GameObject obj) {
        onCollisionExit?.Invoke(collidedWith, obj);
    }
    public void TriggerEnter(Collider trigger, GameObject obj) {
        onTriggerEnter?.Invoke(trigger, obj);
    }
    public void TriggerExit(Collider trigger, GameObject obj) {
        onTriggerExit?.Invoke(trigger, obj);
    }

    // grabbed objects events
    public event Action<OVRInput.Controller, CustomGrabbable> onLeftHandGrabbedChanged, onRightHandGrabbedChanged;
    public event Action<CustomGrabber, OVRInput.Controller, CustomGrabbable> onGrabbedChanged, onGrabbedTurnedOn, onGrabbedTurnedOff;
    public event Action<CustomGrabbable> onLeftGrabbedTurnedOn, onLeftGrabbedTurnedOff, onRightGrabbedTurnedOn, onRightGrabbedTurnedOff;
    public void LeftHandGrabbedChanged(OVRInput.Controller c, CustomGrabbable cg) {
        onLeftHandGrabbedChanged?.Invoke(c, cg);
    }
    public void RightHandGrabbedChanged(OVRInput.Controller c, CustomGrabbable cg) {
        onRightHandGrabbedChanged?.Invoke(c, cg);
    }
    public void GrabbedChanged(CustomGrabber grabber, OVRInput.Controller c, CustomGrabbable cg) {
        onGrabbedChanged?.Invoke(grabber, c, cg);
    }

    public void LeftGrabbedTurnedOn(CustomGrabbable cg) {
        onLeftGrabbedTurnedOn?.Invoke(cg);
    }
    public void LeftGrabbedTurnedOff(CustomGrabbable cg) {
        onLeftGrabbedTurnedOff?.Invoke(cg);
    }
    public void RightGrabbedTurnedOn(CustomGrabbable cg) {
        onRightGrabbedTurnedOn?.Invoke(cg);
    }
    public void RightGrabbedTurnedOff(CustomGrabbable cg) {
        onRightGrabbedTurnedOff?.Invoke(cg);
    }
    public void GrabbedTurnedOn(CustomGrabber grabber, OVRInput.Controller c, CustomGrabbable cg) {
        onGrabbedTurnedOn?.Invoke(grabber, c, cg);
    }
    public void GrabbedTurnedOff(CustomGrabber grabber, OVRInput.Controller c, CustomGrabbable cg) {
        onGrabbedTurnedOff?.Invoke(grabber, c, cg);
    }

    // pointer target events
    public event Action<OVRInput.Controller, GameObject> onLeftPointerTargetChanged, onRightPointerTargetChanged;
    public void LeftPointerTargetChanged(OVRInput.Controller c, GameObject go) {
        onLeftPointerTargetChanged?.Invoke(c, go);
    }
    public void RightPointerTargetchanged(OVRInput.Controller c, GameObject go) {
        onRightPointerTargetChanged?.Invoke(c, go);
    }
}
