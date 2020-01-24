using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OVRGrabbable))]
public class KeyboardController : MonoBehaviour
{

    #region External References
    private CustomGrabbable m_CustomGrabbable;
    public Transform m_KeyboardOrigin;
    //[HideInInspector]
    public List<KeyboardTypeController> m_KeyboardTypeList = new List<KeyboardTypeController>();
    private KeyboardTypeController m_KeyboardTypeController;
    private KeyboardKey[] m_Keys;
    public KeyboardInput m_Input;
    public KeyboardOverlay m_Overlay;
    public DebugKeyboard m_debugConsole;
    #endregion

    #region Controllers
    private OVRInput.Controller m_GrabbedBy = OVRInput.Controller.None;
    private OVRInput.Controller m_ButtonController = OVRInput.Controller.None;
    #endregion

    #region Public Variables
    public float fontSize = 0.025f;
    public enum KeyboardTypeDropdown {
        Square,
        Short,
        Short_With_Space,
        Wheel,
        Wheel_Smaller
    }
    public KeyboardTypeDropdown m_KeyboardType;
    public float AddCharacterThresholdTime = 0.5f;
    public float HoldButtonThresholdTime = 0.3f;
    public bool shouldBeGrabbed = true;
    public bool allowCollision = false;
    public bool debugToggle = false;
    #endregion

    #region Private Grabber Variables
    private Vector2 grabberThumb = new Vector2(0f, 0f);
    private Vector2 grabberThumbProcessed = new Vector2(0f, 0f);
    private int currentKeyIndex = -1;
    #endregion

    #region Private Non-Grabber Variables
    private float lastActionTime = 0f;
    private IEnumerator holdCoroutine;

    private Vector2 buttonThumb = new Vector2(0f, 0f);
    private Vector2 buttonThumbProcessed = new Vector2(0f, 0f);
    private bool buttonThumbRegistered = false;

    private bool selectButton = false;
    private float selectButtonTime = 0f, selectButtonHeldTime = 0f;

    private bool deleteButton = false;
    private float deleteButtonTime = 0f, deleteButtonHeldTime = 0f;

    private bool triggerButton = false;
    private bool changedKeys = false;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        m_CustomGrabbable = this.GetComponent<CustomGrabbable>();
        m_Input.Initialize(this, fontSize);
        m_Overlay.Initialize(this, m_Input);
        //StartCoroutine(m_Overlay.Initialize(this, m_Input));
        //m_debugConsole.Initialize(this);

        foreach(KeyboardTypeController k in m_KeyboardTypeList) {
            k.gameObject.SetActive(false);
        }
        switch(m_KeyboardType) {
            case(KeyboardTypeDropdown.Square):
                m_KeyboardTypeController = m_KeyboardTypeList[0];
                break;
            case(KeyboardTypeDropdown.Short):
                m_KeyboardTypeController = m_KeyboardTypeList[1];
                break;
            case(KeyboardTypeDropdown.Short_With_Space):
                m_KeyboardTypeController = m_KeyboardTypeList[2];
                break;
            case(KeyboardTypeDropdown.Wheel):
                m_KeyboardTypeController = m_KeyboardTypeList[3];
                break;
            case(KeyboardTypeDropdown.Wheel_Smaller):
                m_KeyboardTypeController = m_KeyboardTypeList[4];
                break;
            default:
                m_KeyboardTypeController = m_KeyboardTypeList[0];
                break;
        }
        m_KeyboardTypeController.gameObject.SetActive(true);
        m_Keys = m_KeyboardTypeController.keys;
        for (int i = 0; i < m_Keys.Length; i++) {
            m_Keys[i].CollisionToggle(allowCollision);
            m_Keys[i].ResetKey();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Check if the object is being grabbed or not
        CheckGrabbedBy();
        // Prevent any actions unless it's being held by a hand
        if (m_GrabbedBy == OVRInput.Controller.None) {
            return;
        }

        // Check if any inputs have been detected
        CheckInputs();
        // Process any Inputs
        ProcessInputs();

        // Add scaffold if enough time has passed
        if (lastActionTime != 0f && Time.time - lastActionTime > AddCharacterThresholdTime) {
            AddScaffold(true);
        }

        // Reset some bools
        changedKeys = false;
    }

    private void CheckGrabbedBy() {
        // Checks the CustomGrabbable.cs script to see if a grabber is registered.
        // If there is, then we initialize the GrabBegin() for this particular script
        // Else, we initialize the GrabEnd() for this particular script
        OVRInput.Controller gb = m_CustomGrabbable.GetGrabber();
        if (gb != OVRInput.Controller.None) {
            GrabBegin(gb);
        }
        else {
            GrabEnd();
        }
        return;
    }
    public void GrabBegin(OVRInput.Controller g) {
        // This GrabBegin() must do several things:
        // 1) Set the GrabbedBy to what was provided in the argument
        // 2) Determine which controller is the grabber and which is the button presser
        m_GrabbedBy = g;
        m_Input.Activate();
        switch(g) {
            case(OVRInput.Controller.LTouch):
                m_ButtonController = OVRInput.Controller.RTouch;
                break;
            case(OVRInput.Controller.RTouch):
                m_ButtonController = OVRInput.Controller.LTouch;
                break;
            case(OVRInput.Controller.LTrackedRemote):
                m_ButtonController = OVRInput.Controller.RTrackedRemote;
                break;
            case(OVRInput.Controller.RTrackedRemote):
                m_ButtonController = OVRInput.Controller.LTrackedRemote;
                break;
            default:
                m_ButtonController = g;
                break;
        }
        return;
    }
    public void GrabEnd() {
        // This GrabEnd() must do several things:
        // 1) reset the GrabbedBy to None
        // 2) Reset the keyboard itself, which means setting all keys to their default state.
        m_GrabbedBy = OVRInput.Controller.None;
        AddScaffold(true);
        m_Input.Deactivate();
        if (currentKeyIndex != -1) {
            m_Keys[currentKeyIndex].ResetKey();
            currentKeyIndex = -1;
        }
        return;
    }
    public KeyboardKey GetCurrentKey() {
        if (currentKeyIndex != -1) {
            return m_Keys[currentKeyIndex];
        }
        return null;
    }

    private void CheckInputs() {
        #region Inputs For Grabber
        // Check inputs for grabber. Inputs for grabber include:
        // Thumbstick Direction: Numpad Select
        grabberThumb = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_GrabbedBy);
        #endregion

        #region Inputs For Buttons
        // Check inputs for non-grabber. Inputs for non-grabber include:
        // Thumstick Right - Next Character, Space
        // A/X: Select Numpad
        // B/Y: Delete
        buttonThumb = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_ButtonController);
        if (OVRInput.GetDown(OVRInput.Button.One, m_ButtonController)) {
            selectButton = true;
            selectButtonTime = Time.time;
        } else {
            selectButton = false;
        }
        if (OVRInput.GetUp(OVRInput.Button.One, m_ButtonController)) {
            selectButtonTime = 0f;
        }
        if (OVRInput.GetDown(OVRInput.Button.Two, m_ButtonController)) {
            // Registering delete keypress - delete last inputted character
            deleteButton = true;
            deleteButtonTime = Time.time;
        } else {
            deleteButton = false;
        }
        if (OVRInput.GetUp(OVRInput.Button.Two, m_ButtonController)) {
            // Registering delete keyup
            deleteButtonTime = 0f;
        }
        /*
        triggerButton = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_ButtonController) > 0.1f;
        */
        #endregion

        return;
    }

    private bool BetweenValues(float val, float min, float max, bool inclusive) {
        if (inclusive) {
            return (val-min)*(max-val)>=0;
        } else {
            return (val-min)*(max-val)>0;
        }
    }
    private bool BetweenValues(float val, float min, float max, float inclusive) {
        if (inclusive == 1f) {
            return (val-min)*(max-val)>=0;
        } else {
            return (val-min)*(max-val)>0;
        }
    }
    private void ProcessInputs() {
        #region Process Grabber Inputs
        // Process thumbstick input
        float thumbX = grabberThumb.x, thumbY = grabberThumb.y;
        float angle = Mathf.Atan2(thumbY, thumbX) * Mathf.Rad2Deg + 180f;
        //m_Textbox.SetText(angle.ToString());
        int index = -1;
        if ( BetweenValues(thumbX,-0.35f,0.35f,true) && BetweenValues(thumbY,-0.35f,0.35f,true) ) { index = m_KeyboardTypeController.defaultIndex;  }
        else {
            foreach(Vector4 m in m_KeyboardTypeController.thumbstickAngleMapping) {
                if ( BetweenValues(angle, m.x, m.y, m.z) ) {
                    index = (int)m.w;
                    break;
                }
            }
        }
        /*
        else if ( BetweenValues(angle,0f,22.5f,false) || BetweenValues(angle,337.5f,360f,false) ) { index = 3;  } 
        else if ( BetweenValues(angle,22.5f,67.5f,true) ) { index = 6;  }
        else if ( BetweenValues(angle,67.5f,112.5f,false) ) {   index = 7;  }
        else if ( BetweenValues(angle,112.5f,157.5f,true) ) {   index = 8;  }
        else if ( BetweenValues(angle,157.5f,202.5f,false) ) {  index = 5;  }
        else if ( BetweenValues(angle,202.5f,247.5f,true) ) {   index = 2;  }
        else if ( BetweenValues(angle,247.5f,292.5f,false) ) {  index = 1;  }
        else if ( BetweenValues(angle,292.5f,337.5f,true) ) {   index = 0;  }
        */
        if (index == -1 && currentKeyIndex != -1) {
            m_Keys[currentKeyIndex].ResetKey();
            currentKeyIndex = -1;
        }
        if (currentKeyIndex == -1) {
            currentKeyIndex = index;
        }
        else if (index != currentKeyIndex) {
            m_Keys[currentKeyIndex].ResetKey();
            currentKeyIndex = index;
            m_Keys[currentKeyIndex].HighlightKey();
            changedKeys = true;
        }
        //m_Input.SetText(angle.ToString() + " | " + index.ToString());
        //m_Input.SetText(m_Keys[index].gameObject.name);
        //m_Keys[currentKeyIndex].HighlightKey();

        /*
        if(changedKeys) {
            AddScaffold(true);
        }
        */

        #endregion

        #region Process Non-Grabber Inputs
        // Select (A) Button Held
        if (selectButtonTime != 0f) {
            m_Keys[currentKeyIndex].SelectKey();
        } else {
            m_Keys[currentKeyIndex].HighlightKey();
        }
        // Select (A) Button
        /*
        if (selectButtonTime != 0f && Time.time - selectButtonTime > HoldButtonThresholdTime) {
            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            holdCoroutine = HoldButton("Select");
            StartCoroutine(holdCoroutine);
        } else {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
        */

        // If we changed keys in the interim, we add the current scaffold first
        if (changedKeys) {
            AddScaffold(true);
        }

        if (selectButton) {
            // The button has been pushed down
            
            // we select the next character for that key
            m_Keys[currentKeyIndex].NextCharactersIndex();

            // Add the currently selected key as scaffolding
            UpdateScaffold(m_Keys[currentKeyIndex].GetCurrentCharacter());
            /*
            else if (changedKeys) {
                m_Keys[currentKeyIndex].NextCharactersIndex();
                changedKeys = false;
            }
            */
        } else {
            // The button has been released or if it was switched
            /*
            if (m_Keys[currentKeyIndex].isSpace) {
                m_Input.AddSpace();
            }
            */
            /*
            if (changedKeys) {
                m_Input.AddScaffold();
            }
            else {
                m_Keys[currentKeyIndex].HighlightKey();
            }
            */
            //selectButtonRegistered = false;
        }

        /*
        if (deleteButtonTime != 0f && Time.time - deleteButtonTime > HoldButtonThresholdTime) {
            if (holdCoroutine != null) StopCoroutine(holdCoroutine);
            holdCoroutine = HoldButton("Delete");
            StartCoroutine(holdCoroutine);
        } else {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
        */
        if (deleteButton) {
            // The button has been pushed down now

            // For now, we'l have the joystick control the next character in text stuff.
            // it'll just be that for now, if the button has been pressed down, we select the next character for that key
            if (m_Input.GetScaffold() == "") {
                DeleteCharacter();
            } else {
                ResetScaffold();
            }
            //deleteButtonRegistered = true;
        } else {
            //deleteButtonRegistered = false;
        }

        float buttonThumbX = buttonThumb.x, buttonThumbY = buttonThumb.y;
        float buttonAngle = Mathf.Atan2(buttonThumbY, buttonThumbX) * Mathf.Rad2Deg + 180f;
        if ( BetweenValues(buttonThumbX,-0.35f,0.35f,true)&&BetweenValues(buttonThumbY,-0.35f,0.35f,true) ) {
            buttonThumbRegistered = false;
        }
        else if (!buttonThumbRegistered) {
            if (m_Input.GetScaffold() != "") {
                AddScaffold(true);
            } else {
                AddSpace();
            }
            m_Keys[currentKeyIndex].ResetCharactersIndex();
            buttonThumbRegistered = true;
        }
        /*
        else if ( BetweenValues(buttonAngle,157.5f,202.5f,false) && !buttonThumbRegistered ) {
            if (m_Keys[currentKeyIndex].GetCurrentCharacter() != "") {
                AddScaffold(true);
            }
            m_Input.AddSpace();
            m_Keys[currentKeyIndex].ResetCharactersIndex();
            buttonThumbRegistered = true;
        }
        */
        #endregion

        #region Miscellaneous Reset Some Variables
        changedKeys = false;
        #endregion
    }

    private IEnumerator HoldButton(string type) {
        while(true) {
            switch(type) {
                case("Select"):
                    AddScaffold(false);
                    break;
                case("Delete"):
                    DeleteCharacter();
                    break;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void AddScaffold(bool reset) {
        m_Input.AddScaffold(reset);
        lastActionTime = 0f;
    }
    private void AddSpace() {
        m_Input.AddSpace();
        lastActionTime = 0f;
    }
    private void UpdateScaffold(string s) {
        m_Input.UpdateScaffold(s);
        lastActionTime = 0f;
    }
    private void ResetScaffold() {
        m_Input.ResetScaffold();
        lastActionTime = 0f;
    }
    private void DeleteCharacter() {
        m_Input.DeleteCharacter();
        lastActionTime = 0f;
    }
}
