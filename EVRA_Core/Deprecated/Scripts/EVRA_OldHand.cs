using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using EVRA;
using EVRA.Events;

public class EVRA_OldHand : MonoBehaviour
{
    [Header("References")]
    #region Controller Type
        [SerializeField] [Tooltip("Reference to '__ControllerAnchor' gameObject under '__HandAnchor'")]
        private Transform m_controllerAnchor;

        [SerializeField] [Tooltip("Reference to controller appearance")]
        private OVRControllerHelper m_OVRControllerHelper;
        
        [SerializeField] [Tooltip("Reference to OVR controller this hand is attached to")]
        private OVRInput.Controller m_OVRController = OVRInput.Controller.None;
        public OVRInput.Controller OVRController {
            get {   return m_OVRController;    }
        }
    #endregion
    
    /*
    [Header("New Event System")]
    #region InputEvents
    public List<EVRA_HandEvent> newEvents;
    #endregion
    */

    #region InputValues
        private float m_IndexTriggerValue = 0f; // 0f == neutral, 1f == pressed
        public float IndexTriggerValue {
            get {   return m_IndexTriggerValue; }
            set {}
        }
        private float m_GripTriggerValue = 0f;  // 0f == neutral, 1f == pressed
        public float GripTriggerValue {
            get {   return m_GripTriggerValue;  }
            set {}
        }
        private Vector2 m_ThumbstickDirection = Vector2.zero;
        public Vector2 ThumbstickDirection {
            get { return m_ThumbstickDirection; }
            set {}
        }
        private Vector2 m_ThumbstickAngle = Vector2.zero;
        public Vector2 ThumbstickAngle {
            get {   return m_ThumbstickAngle;   }
            set {}
        }
        private bool m_ThumbstickPress = false;
        public bool ThumbstickPress {
            get {   return m_ThumbstickPress;   }
            set {}
        }
        private bool m_ButtonTwoValue = false;
        public bool ButtonTwoValue {
            get {   return m_ButtonTwoValue;  }
            set {}
        }
        private bool m_ButtonOneValue = false;
        public bool ButtonOneValue {
            get {   return m_ButtonOneValue;  }
            set {}
        }
        private bool m_StartButtonValue = false;
        public bool StartButtonValue {
            get {   return m_StartButtonValue;  }
            set {}
        }
    #endregion

    #region OutputValues
        private Vector3 prevPosition = Vector3.zero;
        private Vector3 m_velocity;
        public Vector3 velocity {
            get {   return m_velocity;  }
            set {}
        }
    #endregion

    [Header("Input Event Mapping")]
    #region Events
        public EVRA_OldEvent IndexTriggerDown;
        public EVRA_OldEvent IndexTriggerUp;

        public EVRA_OldEvent GripTriggerDown;
        public EVRA_OldEvent GripTriggerUp;

        public EVRA_OldEvent ThumbstickButtonDown;
        public EVRA_OldEvent ThumbstickButtonUp;

        public EVRA_OldThumbstick ThumbstickMove;
        public EVRA_OldThumbstick ThumbstickRest;

        public EVRA_OldEvent ButtonTwoDown;
        public EVRA_OldEvent ButtonTwoUp;

        public EVRA_OldEvent ButtonOneDown;
        public EVRA_OldEvent ButtonOneUp;

        public EVRA_OldEvent StartButtonDown;
        public EVRA_OldEvent StartButtonUp;
    #endregion

    /*
    [System.Serializable]
    public class InputMatch {
        public HandFunctions.InputType input;
        public List<HandFunctions.InputState> states = new List<HandFunctions.InputState>();

        public InputResponse response;
        public HandFunctions.InputState currentState = HandFunctions.InputState.Rest;

        public Dictionary<HandFunctions.InputState, bool> results;

        [System.Serializable]
        public class InputResult {
            public HandFunctions.InputState state;
            public bool result;
            public InputResult(HandFunctions.InputState state, bool result) {
                this.state = state;
                this.result = result;
            }
        }
        public List<InputResult> printResults = new List<InputResult>();

        private System.Func<OVRInput.Controller, InputResponse> GetMethod;
        private bool isDown = false, prevDown = false;
        
        public InputMatch(HandFunctions.InputType input) {
            this.input = input;
            GetMethod = HandFunctions.InputMap[this.input];
        }
        public void Init() {
            results = new Dictionary<HandFunctions.InputState, bool>();
            foreach(HandFunctions.InputState state in states) {
                results.Add(state, false);
            }
        }
        public void CheckInput(OVRInput.Controller controller = OVRInput.Controller.None) {
            response = GetMethod(controller);
            isDown = response.direction.magnitude > 0.1f;
            currentState = GetCurrentState(isDown, prevDown);

            printResults = new List<InputResult>();
            foreach(HandFunctions.InputState state in states) {
                bool result = DetermineInputState(state, isDown, prevDown);
                results[state] = result;
                printResults.Add(new InputResult(state, result));
            }

            prevDown = isDown;
        }

        public static bool DetermineInputState(HandFunctions.InputState state, bool isDown, bool prevDown) {
            bool answer;
            switch(state) {
                case (HandFunctions.InputState.Rest):
                    // Q: How does "rest" compute?
                    // A: current frame and previous frame magnitude is within dead zone
                    answer = !isDown && !prevDown;
                    break;
                case (HandFunctions.InputState.Stay):
                    // Q: How does "stay" compute?
                    //  A: current frame and previous frame magnitudes are not within dead zone
                    answer = isDown && prevDown;
                    break;
                case (HandFunctions.InputState.Down):
                    // Q: How does "down" compute?
                    //  A: current frame's magnitude is not within dead zone, but previous frame's was
                    answer = isDown && !prevDown;
                    break;
                case (HandFunctions.InputState.Up):
                    // Q: How does "up" compute?
                    //  A: current frame's magnitude is within dead zone, but previous frame's wasn't
                    answer = !isDown && prevDown;
                    break;
                case (HandFunctions.InputState.Changed):
                    // Q: How does "changed" compute?
                    //  A: As long as previous state wasn't same as this state.
                    answer = isDown != prevDown;
                    break;
                default:
                    // Defaulting to NOPE to avoid bad cases
                    answer = false;
                    break;
            }
            
            return answer;
        }

        public static HandFunctions.InputState GetCurrentState(bool isDown, bool prevDown) {
            if (!isDown && !prevDown) return HandFunctions.InputState.Rest;
            if (isDown && prevDown) return HandFunctions.InputState.Stay;
            if (isDown && !prevDown) return HandFunctions.InputState.Down;
            if (!isDown && prevDown) return HandFunctions.InputState.Up;
            if (isDown != prevDown) return HandFunctions.InputState.Changed;
            return HandFunctions.InputState.Rest;
        }
    }

    [System.Serializable]
    public class EventMatch {
        public HandFunctions.InputType input;
        public List<int> inputKeys;
    }
    */

    #region Misc 
    public GameObject DebugObject;
    private float lastTime;
    /*
    private Dictionary<HandFunctions.InputType, InputMatch> inputsToCheck = new Dictionary<HandFunctions.InputType, InputMatch>();
    private Dictionary<EVRA_New_Event, List<EventMatch>> eventsToCheck = new Dictionary<EVRA_New_Event, List<EventMatch>>();
    // For show only, for now
    public List<InputMatch> inputsToPrint = new List<InputMatch>();
    public List<EventMatch> eventsToCall = new List<EventMatch>();
    */
    #endregion

    private void Awake() {
        m_OVRControllerHelper.m_controller = m_OVRController;
        lastTime = Time.time;
    }
    private void Start() {
        prevPosition = transform.localPosition;
    }
    private void Update() {
        // Old System
        this.CheckInputs();
        this.CheckOutputs();
        this.InvokeEvents();
        float thisTime = Time.time;
        Debug.Log("Old Hand: " + (thisTime - lastTime));
        lastTime = thisTime;
    }

    private void CheckInputs() {
        m_IndexTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_OVRController);
        m_GripTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_OVRController);
        m_ThumbstickPress = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, m_OVRController);
        m_ThumbstickDirection = Vector2.ClampMagnitude(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_OVRController), 1f);
        m_ThumbstickAngle = new Vector2(CommonFunctions.GetAngleFromVector2(m_ThumbstickDirection, m_OVRController), Vector2.Distance(Vector2.zero, m_ThumbstickDirection));
        m_ButtonTwoValue = OVRInput.Get(OVRInput.Button.Two, m_OVRController);
        m_ButtonOneValue = OVRInput.Get(OVRInput.Button.One, m_OVRController);
        m_StartButtonValue = OVRInput.Get(OVRInput.Button.Start, m_OVRController);

        /*
        // m_inputDowns["left_thumbNorth"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch))        ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickUp, OVRInput.Controller.LTouch))     ? -1f : 0f;
        // m_inputDowns["left_thumbSouth"] =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.LTouch))      ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickDown, OVRInput.Controller.LTouch))   ? -1f : 0f;
        // m_inputDowns["left_thumbEast"]  =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.LTouch))     ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickRight, OVRInput.Controller.LTouch))  ? -1f : 0f;
        // m_inputDowns["left_thumbWest"]  =    (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.LTouch))      ? 1f    : (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstickLeft, OVRInput.Controller.LTouch))   ? -1f : 0f;
        m_inputDowns["start"]           =    (OVRInput.GetDown(OVRInput.Button.Start))                                                  ? 1f    : (OVRInput.GetUp(OVRInput.Button.Start))                                               ? -1f : 0f;
        m_thumbAngles["left"]           = new Vector2(CommonFunctions.GetAngleFromVector2(m_thumbDirections["left"], OVRInput.Controller.LTouch), Vector2.Distance(Vector2.zero, m_thumbDirections["left"]));
        */
    }

    private void CheckOutputs() {
        // Velocity check
        m_velocity = (m_controllerAnchor.transform.localPosition - prevPosition) / Time.deltaTime;
        prevPosition = m_controllerAnchor.transform.localPosition;
    }

    private void InvokeEvents() {
        // A few pointers:
        // GetDown() and GetUp() are within the context of the CURRENT FRAME. They're used here to detect changes in the current frame.
        // Get() returns the status of the input currently, regardless of the current frame.
        // So for example, in the moment that the player presses down on the trigger, GetDown() returns true... but if the player keeps it consistently down then GetDown() will return false in the next frame.
        // Whereas as long as the player is holding down the index trigger, Get() will always return a TRUE or 1f.

        // INDEX TRIGGER EVENTS
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, m_OVRController)) {
            // Index pressed down in this current frame
            IndexTriggerDown?.Invoke();
        } else if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, m_OVRController)) {
            // Index released in this current frame
            IndexTriggerUp?.Invoke();
        }

        // GRIP TRIGGER EVENTS
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, m_OVRController)) {
            // Grips pressed down in this current frame
            GripTriggerDown?.Invoke();
        } else if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, m_OVRController)) {
            // Grips released in this current frame
            GripTriggerUp?.Invoke();
        }

        // THUMBSTICK MOVE TRIGGER EVENTS
        if (m_ThumbstickDirection.magnitude >= 0.1f) {
            // Thumbstick is moved beyond the deadzone of 0.1f in this current frame
            ThumbstickMove?.Invoke(m_ThumbstickDirection);
        } else {
            ThumbstickRest?.Invoke(m_ThumbstickDirection);
        }

        // THUMBSTICK PRESS TRIGGER EVENTS
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, m_OVRController)) {
            // Thumbstick button pressed down in this current frame
            ThumbstickButtonDown?.Invoke();
        } else if (OVRInput.GetUp(OVRInput.Button.PrimaryThumbstick, m_OVRController)) {
            // Thumbstick button released in this current frame
            ThumbstickButtonUp?.Invoke();
        }

        // BUTTON 2 PRESS TRIGGER EVENTS
        if (OVRInput.GetDown(OVRInput.Button.Two, m_OVRController)) {
            // Button 2 pressed down in this current frame
            ButtonTwoDown?.Invoke();
        } else if (OVRInput.GetUp(OVRInput.Button.Two, m_OVRController)) {
            // Button 2 released in this current frame
            ButtonTwoUp?.Invoke();
        }

        // BUTTON 1 PRESS TRIGGER EVENTS
        if (OVRInput.GetDown(OVRInput.Button.One, m_OVRController)) {
            // Button 1 pressed down in this current frame
            ButtonOneDown?.Invoke();
        } else if (OVRInput.GetUp(OVRInput.Button.One, m_OVRController)) {
            // Button 1 released in this current frame
            ButtonOneUp?.Invoke();
        }

        // START BUTTON PRESS TRIGGER EVENTS
        if (OVRInput.GetDown(OVRInput.Button.Start, m_OVRController)) {
            // Start Button pressed down in this current frame
            StartButtonDown?.Invoke();
        } else if (OVRInput.GetUp(OVRInput.Button.Start, m_OVRController)) {
            // Start Button released in this current frame
            StartButtonUp?.Invoke();
        }

    }

    public void HandleDebug(bool to = true) {
        if (DebugObject) DebugObject.GetComponent<Renderer>().enabled = to;
    }

    /*
    public void DebugActivate(OVRInput.Controller c, Dictionary<InputType, InputResponse> r) {
        if (c != m_OVRController) return;
        HandleDebug(true);
    }
    public void DebugDeactivate(OVRInput.Controller c, Dictionary<InputType, InputResponse> r) {
        if (c != m_OVRController) return;
        HandleDebug(false);
    }
    */

    /*
    [SerializeField] [Tooltip("Reference to object that acts as object detection for the grabbing function")]
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

    [SerializeField]
    [Tooltip("Should grabbed objects snap to the hand?")]
    private bool m_shouldSnap = true;
    public bool shouldSnap {
        get {   return m_shouldSnap;    }
        set {}
    }

    [SerializeField] [Tooltip("Debug text object - for debugging")]
    private TextMeshProUGUI m_debugText;

    // Start is called before the first frame update
    public void Init()
    {
        // Set the hand controller
        m_OVRControllerHelper.m_controller = m_OVRController;

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

    private void GrabBegin(CustomGrabbable c) {

        if (c.GrabBegin(this)) {
            m_grabbedObject = c;
        }

        // We also shoot out an event that tells our event system that something was let go
        switch(m_OVRController) {
            case(OVRInput.Controller.LTouch):
                CustomEvents.current.LeftGrabbedTurnedOn(c);
                break;
            case(OVRInput.Controller.RTouch):
                CustomEvents.current.RightGrabbedTurnedOn(c);
                break;
        }
        CustomEvents.current.GrabbedTurnedOn(this, OVRController, c);
    }

    private void GrabEnd() {        
        OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(m_OVRController), orientation = OVRInput.GetLocalControllerRotation(m_OVRController) };

		OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
		Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_OVRController);
		Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_OVRController);
        angularVelocity *= -1;
        
        m_grabbedObject.GrabEnd(this, linearVelocity, angularVelocity);

        // We also shoot out an event that tells our event system that something was let go
        switch(m_OVRController) {
            case(OVRInput.Controller.LTouch):
                CustomEvents.current.LeftGrabbedTurnedOff(m_grabbedObject);
                break;
            case(OVRInput.Controller.RTouch):
                CustomEvents.current.RightGrabbedTurnedOff(m_grabbedObject);
                break;
        }
        CustomEvents.current.GrabbedTurnedOff(this, OVRController, m_grabbedObject);

        m_grabbedObject = null;
    }
    */
}
