using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using EVRA;
using EVRA.Inputs;
using EVRA.Events;

public class EVRA_Hand : MonoBehaviour
{

    [System.Serializable]
    public class EVRA_HandEvent {
        public string name = "";
        public bool enabled = true;
        public List<InputEventMap> ifs = new List<InputEventMap>();
        public EVRA_Event thens = new EVRA_Event();
        public EVRA_HandEvent() {}
        public EVRA_HandEvent(string name, List<InputEventMap> ifs, UnityAction<InputEventDataPackage> call, bool enabled = true) {
            this.name = name;
            this.ifs = ifs;
            thens.AddListener(call);
            this.enabled = enabled;
        }
    }

    #region Controller Type
        [Header("==: References :==")]
        [SerializeField] [Tooltip("Reference to '__ControllerAnchor' gameObject under '__HandAnchor'")]
        private Transform m_controllerAnchor;

        [Tooltip("Reference to controller appearance")]
        private OVRControllerHelper m_OVRControllerHelper;
        public OVRControllerHelper ovrControllerHelper {
            get { return m_OVRControllerHelper; }
            set {}
        }
        
        [SerializeField] [Tooltip("Reference to OVR controller this hand is attached to")]
        private OVRInput.Controller m_OVRController = OVRInput.Controller.None;
        public OVRInput.Controller OVRController {
            get {   return m_OVRController;    }
            set {}
        }

        [Tooltip("Reference to Hand Animator")]
        private EVRA_HandAnimator m_handAnimator;
        private EVRA_HandAnimator_NoController m_handAnimator_NoController;

    #endregion
    
    #region InputEvents
        [Header("==: Event System :==")]
        [SerializeField, Tooltip("List the events you want to attribute to values here.")]
        private List<EVRA_HandEvent> m_events;
        private Dictionary<InputType, InputResult> m_inputs;
        public Dictionary<InputType, InputResult> inputs {
            get { return m_inputs; }
            set {}
        }

        [Tooltip("Set this to determine which specific input types to track. Ya gotta optimize wherever you can!\n\n- \"All Inputs\": EVRA_Hand will measure all inputs.\n- \"Only Event Inputs\" (Recommended): EVRA_Hand will measure only the inputs aggregated from \"Events\".")]
        public TrackInputSettings m_trackInputSettings = TrackInputSettings.OnlyEventInputs;
        public enum TrackInputSettings {
            AllInputs,
            OnlyEventInputs
        }
    #endregion

    #region OutputValues
        [Header("==: Estimated Velocity Values :==")]
        [SerializeField, Range(2,30), Tooltip("Number of frames that estimated velocity is averaged over.\n\n2 frames is most accurate, but can be too accurate (ex. if your hand flicks downward, velocity is based on that flick rather than the average arm movement.\n\nWe recommend 5 frames to balance between accurate and averaged movement.")]
        private int m_velocityFrameRate = 5;
        [ShowOnly, SerializeField, Tooltip("Change in position over time. Averaged over 5 frames.")]
        private Vector3 m_velocity;
        public Vector3 velocity {
            get {   return m_velocity;  }
            set {}
        }
        [ShowOnly, SerializeField, Tooltip("Change in angular rotation over time. Calculated between current frame and previous frame.")]
        private Vector3 m_angularVelocity;
        public Vector3 angularVelocity {
            get { return m_angularVelocity; }
            set {}
        }

        private List<Vector3> prevPositions = new List<Vector3>();
        private Vector3 averageVelocity = Vector3.zero;
        private Quaternion prevRotation = Quaternion.identity;
        private float rotAngle = 0f;
        private Vector3 rotAxis = Vector3.zero;

        private InputEventDataPackage packageToSend;
    #endregion

    /*
    #region Misc 
        [Header("Miscellaneous")]
        public GameObject DebugObject;
    #endregion
    */

    private void Awake() {
        // If we have an OVRControllerHelper referenced, we set it so that the proper controller appears
        m_OVRControllerHelper = gameObject.GetComponentInChildren<OVRControllerHelper>();
        // If we have a hand animator, we set it
        m_handAnimator = gameObject.GetComponentInChildren<EVRA_HandAnimator>();
        m_handAnimator_NoController = gameObject.GetComponentInChildren<EVRA_HandAnimator_NoController>();
        // We add to prevPositions so that there's at least 2 positions to compare the velocities to.
        prevPositions.Add(Vector3.zero);
        // We need to look through each HandEvent we added through the inspector, if we wnt to look at only the specific event inputs
        OnValidate();
    }

    private void Start() {
        // If we have an OVRControllerHelper referenced, we set it so that the proper controller appears
        if (m_OVRControllerHelper != null)
            m_OVRControllerHelper.m_controller = m_OVRController;
        // If we have a hand animator, we set it
        if (m_handAnimator != null) {
            m_handAnimator.Init(this);
        }
        if (m_handAnimator_NoController != null) {
            m_handAnimator_NoController.Init(this);
        }
    }

    private void OnValidate() {
        if (m_trackInputSettings == TrackInputSettings.OnlyEventInputs) {
            m_inputs = new Dictionary<InputType, InputResult>();
            foreach(EVRA_HandEvent ev in m_events) {             
                foreach(InputEventMap ifs_input in ev.ifs) {
                    InputType input = ifs_input.input;       
                    // Check if we already have this input's InputType in our dictionary
                    if(!m_inputs.ContainsKey(input)) {
                        // It doesn't. That means we need to add a new value with this InputType as the key
                        m_inputs.Add(
                            input, 
                            new InputResult()
                        );
                    } 
                }
            }
        } else {
            m_inputs = new Dictionary<InputType, InputResult>() {
                {InputType.ButtonOne, new InputResult()},
                {InputType.ButtonTwo, new InputResult()},
                {InputType.ButtonThumbstick, new InputResult()},
                {InputType.ButtonStart, new InputResult()},
                {InputType.IndexTrigger, new InputResult()},
                {InputType.GripTrigger, new InputResult()},
                {InputType.Thumbstick, new InputResult()},
            };
        }
    }

    private void Update() {
        this.UpdatePosition();
        this.CheckVelocity();
        this.CheckAngularVelocity();
        this.CheckInputs();
        this.CheckOutputs();
    }

    private void UpdatePosition() {
        transform.position = m_controllerAnchor.position;
        transform.rotation = m_controllerAnchor.rotation;
    }

    private async void CheckVelocity() {
        prevPositions.Add(transform.position);
        if (prevPositions.Count > m_velocityFrameRate) prevPositions.RemoveAt(0);
        averageVelocity = Vector3.zero;
        for(var i = 1; i < prevPositions.Count; i++) {
            averageVelocity += (prevPositions[i] - prevPositions[i-1]) / Time.deltaTime;
        }
        m_velocity = averageVelocity / (prevPositions.Count-1);
    }

    private void CheckAngularVelocity() {
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(prevRotation);
        prevRotation = transform.rotation;
        deltaRotation.ToAngleAxis(out rotAngle, out rotAxis);
        rotAngle *= Mathf.Deg2Rad;
        m_angularVelocity = (1.0f / Time.deltaTime) * rotAngle * rotAxis;
    }

    private void CheckInputs() {
        foreach(KeyValuePair<InputType, InputResult> kvp in m_inputs) {
            kvp.Value.CheckInput(kvp.Key, m_OVRController);
        }
    }

    private void CheckOutputs() {
        packageToSend = new InputEventDataPackage(this, m_inputs);
        foreach(EVRA_HandEvent ev in m_events) {
            if (!ev.enabled) continue;
            List<InputEventMap> ifs = ev.ifs;
            EVRA_Event thens = ev.thens;
            bool call = true;
            foreach(InputEventMap inp in ifs) {
                call = call && (
                    inp.state == InputEvent.Any ||
                    inp.state == m_inputs[inp.input].currentState
                );
            }
            if (call) thens?.Invoke(packageToSend);
        }
    }

    public void ToggleEventByName(string name, bool to) {
        foreach(EVRA_HandEvent ev in m_events) {
            if (ev.name == name) {
                ev.enabled = to;
            }
        }
    }

    public void ToggleEventByInputType(InputType inType, bool to) {
        foreach(EVRA_HandEvent ev in m_events) {
            List<InputEventMap> maps = ev.ifs;
            foreach(InputEventMap m in maps) {
                if (m.input == inType) {
                    ev.enabled = to;
                    continue;
                }
            }
        }
    }

    public bool AddNewEvent(string name, List<InputEventMap> ifs, UnityAction<InputEventDataPackage> call, bool enabled = true) {
        try {
            EVRA_HandEvent ev = new EVRA_HandEvent(name, ifs, call, enabled);
            m_events.Add(ev);
            OnValidate();
            return true;
        } catch {
            return false;
        }
    }
    public bool AddNewEvent(string name, InputEventMap ifs, UnityAction<InputEventDataPackage> call, bool enabled = true) {
        List<InputEventMap> maps = new List<InputEventMap>(new InputEventMap[] {ifs});
        return AddNewEvent(name, maps, call, enabled);
    }

    public void RemoveEventByName(string name) {
        List<EVRA_HandEvent> newEvents = new List<EVRA_HandEvent>();
        foreach(EVRA_HandEvent ev in m_events) {
            if (ev.name != name) newEvents.Add(ev);
        }
        m_events = newEvents;
        OnValidate();
    }

    public void RemoveEventByInputType(InputType inType) {
        List<EVRA_HandEvent> newEvents = new List<EVRA_HandEvent>();
        foreach(EVRA_HandEvent ev in m_events) {
            List<InputEventMap> maps = ev.ifs;
            bool addEvent = true;
            foreach(InputEventMap m in maps) {
                if (m.input == inType) {
                    addEvent = false;
                    break;
                }
            }
            if (addEvent) {
                newEvents.Add(ev);
            }
        }
        m_events = newEvents;
        OnValidate();
    }

    /*
    public void HandleDebug(bool to = true) {
        if (DebugObject) {
            if (CommonFunctions.HasComponent<Renderer>(DebugObject)) {
                DebugObject.GetComponent<Renderer>().enabled = to;
            }
        }
    }
    public void DebugActivate(OVRInput.Controller c, Dictionary<InputType, InputResult> r) {
        if (c != m_OVRController) return;
        HandleDebug(true);
    }
    public void DebugDeactivate(OVRInput.Controller c, Dictionary<InputType, InputResult> r) {
        if (c != m_OVRController) return;
        HandleDebug(false);
    }
    */
}
