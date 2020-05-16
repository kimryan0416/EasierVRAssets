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

    [SerializeField]
    [Tooltip("Hand references")]
    private CustomGrabber m_leftHand, m_rightHand;

    [SerializeField]
    [Tooltip("Track times when buttons are pressed down")]
    private bool m_trackingTime = false, m_trackingDown = true;
    [SerializeField]
    [Tooltip("List of colliders that must be tracked")]
    private List<CustomGrabber_GrabVolume> m_colliders = new List<CustomGrabber_GrabVolume>();

    private void Awake() {
        current = this;
        if (m_leftHand != null) PrepareLeft(m_leftHand);
        if (m_rightHand != null) PrepareRight(m_rightHand);
    }

    private void Update() {
        if (m_trackingTime) CheckTimes();
        if (m_trackingDown) CheckDowns();
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

    private void UpdateEvents() {

        // Left grip
        if (m_inputDowns["left_grip"] == 1f) {
            current.LeftGripDown(OVRInput.Controller.LTouch); 
            current.GripDown(OVRInput.Controller.None);
        }
        if (m_inputDowns["left_grip"] == -1f) {  
            current.LeftGripUp(OVRInput.Controller.LTouch);
            current.GripUp(OVRInput.Controller.None);
        }
        // Right Grip
        if (m_inputDowns["right_grip"] == 1f) {
            current.RightGripDown(OVRInput.Controller.RTouch);
            current.GripDown(OVRInput.Controller.None);
        }
        if (m_inputDowns["right_grip"] == -1f) {
            current.RightGripUp(OVRInput.Controller.RTouch);
            current.GripUp(OVRInput.Controller.None);
        }

        // Left Index Trigger
        if (m_inputDowns["left_index"] == 1f) {
            current.LeftTriggerDown(OVRInput.Controller.LTouch);
            current.TriggerDown(OVRInput.Controller.None);
        }
        if (m_inputDowns["left_index"] == -1f) {
            current.LeftTriggerUp(OVRInput.Controller.LTouch);
            current.TriggerUp(OVRInput.Controller.None);
        }
        // Right Index Trigger
        if (m_inputDowns["right_index"] == 1f) {
            current.RightTriggerDown(OVRInput.Controller.RTouch);
            current.TriggerDown(OVRInput.Controller.None);
        }
        if (m_inputDowns["right_index"] == -1f) {
            current.RightTriggerUp(OVRInput.Controller.RTouch);
            current.TriggerUp(OVRInput.Controller.None);
        }

        // Left One Button
        if (m_inputDowns["left_one"] == 1f) {
            current.LeftOneDown(OVRInput.Controller.LTouch);
            current.OneDown(OVRInput.Controller.None);
        }
        if (m_inputDowns["left_one"] == -1f) {
            current.LeftOneUp(OVRInput.Controller.None);
            current.OneUp(OVRInput.Controller.None);
        }
        // Right One Button
        if (m_inputDowns["right_one"] == 1f) {
            current.RightOneDown(OVRInput.Controller.RTouch);
            current.OneDown(OVRInput.Controller.None);
        }
        if (m_inputDowns["right_one"] == -1f) {
            current.RightOneUp(OVRInput.Controller.RTouch);
            current.OneUp(OVRInput.Controller.None);
        }

        // Left Two Button
        if (m_inputDowns["left_two"] == 1f) {
            current.LeftTwoDown(OVRInput.Controller.LTouch);
            current.TwoDown(OVRInput.Controller.None);
        }
        if (m_inputDowns["left_two"] == -1f) {
            current.LeftTwoUp(OVRInput.Controller.None);
            current.TwoUp(OVRInput.Controller.None);
        }
        // Right Two Button
        if (m_inputDowns["right_two"] == 1f) {
            current.RightTwoDown(OVRInput.Controller.RTouch);
            current.TwoDown(OVRInput.Controller.None);
        }
        if (m_inputDowns["right_two"] == -1f) {
            current.RightTwoUp(OVRInput.Controller.RTouch);
            current.TwoUp(OVRInput.Controller.None);
        }

        // Left Thumbstick Press Button
        if (m_inputDowns["left_thumbPress"] == 1f) {
            current.LeftThumbPress(OVRInput.Controller.LTouch);
            current.ThumbPress(OVRInput.Controller.None);
        }
        if (m_inputDowns["left_thumbPress"] == -1f) {
            current.LeftThumbRelease(OVRInput.Controller.None);
            current.ThumbRelease(OVRInput.Controller.None);
        }
        // Right Thumbstick Press Button
        if (m_inputDowns["right_thumbPress"] == 1f) {
            current.RightThumbPress(OVRInput.Controller.RTouch);
            current.ThumbPress(OVRInput.Controller.None);
        }
        if (m_inputDowns["right_thumbPress"] == -1f) {
            current.RightThumbRelease(OVRInput.Controller.RTouch);
            current.ThumbRelease(OVRInput.Controller.None);
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
    }

    // Events for grip
    public event Action<OVRInput.Controller> onLeftGripDown, onLeftGripUp, onRightGripDown, onRightGripUp, onGripDown, onGripUp;
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
    public void GripDown(OVRInput.Controller c) {
        onGripDown?.Invoke(c);
    }
    public void GripUp(OVRInput.Controller c) {
        onGripUp?.Invoke(c);
    }

    public event Action<OVRInput.Controller> onLeftTriggerDown, onLeftTriggerUp, onRightTriggerDown, onRightTriggerUp, onTriggerDown, onTriggerUp;
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
    public void TriggerDown(OVRInput.Controller c) {
        onTriggerDown?.Invoke(c);
    }
    public void TriggerUp(OVRInput.Controller c) {
        onTriggerUp?.Invoke(c);
    }

    public event Action<OVRInput.Controller> onLeftOneDown, onLeftOneUp, onRightOneDown, onRightOneUp, onOneDown, onOneUp;
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
    public void OneDown(OVRInput.Controller c) {
        onOneDown?.Invoke(c);
    }
    public void OneUp(OVRInput.Controller c) {
        onOneUp?.Invoke(c);
    }

    public event Action<OVRInput.Controller> onLeftTwoDown, onLeftTwoUp, onRightTwoDown, onRightTwoUp, onTwoDown, onTwoUp;
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
    public void TwoDown(OVRInput.Controller c) {
        onTwoDown?.Invoke(c);
    }
    public void TwoUp(OVRInput.Controller c) {
        onTwoUp?.Invoke(c);
    }

    public event Action<OVRInput.Controller> onLeftThumbPress, onLeftThumbRelease, onRightThumbPress, onRightThumbRelease, onThumbPress, onThumbRelease;
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
    public void ThumbPress(OVRInput.Controller c) {
        onThumbPress?.Invoke(c);
    }
    public void ThumbRelease(OVRInput.Controller c) {
        onThumbRelease?.Invoke(c);
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
}
