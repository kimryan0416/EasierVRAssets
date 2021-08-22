using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{

    #region Variables and References

    [SerializeField] [Tooltip("Reference to the cursor")]
    private DialogueTextbox m_DialogueCursor;

    [SerializeField] [Tooltip("Reference to the main dialogue box")]
    private DialogueTextbox m_MainTextbox;

    [SerializeField] [Tooltip("Reference to the speaker box")]
    private DialogueTextbox m_Speaker;

    [SerializeField] [Tooltip("References to any hands we want to be pointed by")]
    private List<CustomGrabber> m_Grabbers = new List<CustomGrabber>();

    private enum nextButtonOption {
        A_Down,
        A_Up,
        B_Down,
        B_Up,
        X_Down,
        X_Up,
        Y_Down,
        Y_Up
    }
    [SerializeField] [Tooltip("Select which button should be the 'Next' button")]
    private nextButtonOption m_nextButton = nextButtonOption.A_Up;

    [SerializeField] [Tooltip("Reference to the JSON file that contains dialogue")]
    private TextAsset jsonFile;
    // Our dialogue dictionary
    private Dictionary<string, DialogueItem[]> dialogue = new Dictionary<string, DialogueItem[]>();

    // What was the cursor's original position?
    private Vector3 m_CursorOriginalLocalPosition = Vector3.zero;

    // What pointer are we being pointed by?
    private CustomPointer m_BeingPointedBy = null;

    // What DialogueTarget are we hitting?
    private DialogueTarget m_DialogueTarget = null;

    // Are we allowed to proceed to the next action?
    private bool m_InProgress = false;

    [SerializeField] [Tooltip("Should this be initialized on Start?")]
    private bool m_initializeOnStart = true;
    [Tooltip("Are we allowed to be active?")]
    private bool m_isActive = true;

    [SerializeField] [Tooltip("List of debug objects")]
    private GameObject m_debugObject = null;

    // The Dialogue Items we're currently working with
    private string m_CurrentKey = null;
    private DialogueItem[] m_CurrentDialogueItems = null;
    private int m_CurrentIndex = -1;

    #endregion
    
    #region Unity callbacks

    void Awake() {
        // When we start, we need to rotate the
    }

    // Start is called before the first frame update
    void Start() {
        if (m_initializeOnStart) Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isActive) return;
        // Check if we're being pointed at by any of the controllers
        //CheckIfPointedAt();
        if (m_BeingPointedBy != null) {
            // if we're being pointed by someone, then we need to move our cursor to match the pointer's hit position
            Vector3 hitPosition = this.transform.InverseTransformPoint(m_BeingPointedBy.pointerDest);
            m_DialogueCursor.transform.localPosition = new Vector3(hitPosition.x, hitPosition.y, m_DialogueCursor.transform.localPosition.z);
        } else {
            m_DialogueCursor.transform.localPosition = m_CursorOriginalLocalPosition;
        }


        /*
        if (m_DialogueTarget != null) {
            m_debugObject.transform.position = new Vector3(m_debugObject.transform.position.x, m_debugObject.transform.position.y + 0.01f, m_debugObject.transform.position.z);
        }
        */
    }

    #endregion

    #region Initializers

    public void Init() {
        
        // Save the dialogue cursor's initial local position
        if (m_DialogueCursor != null) {
            m_CursorOriginalLocalPosition = m_DialogueCursor.transform.localPosition;
        }

        // Extract the JSON from jsonFile
        Dialogue dialogueFromJSON = JsonUtility.FromJson<Dialogue>(jsonFile.text);
        foreach(DialogueSegment ds in dialogueFromJSON.dialogueSegments) {
            dialogue.Add(ds.key, ds.dialogueItems);
        }

        foreach(KeyValuePair<string, DialogueItem[]> kvp in dialogue) {
            if (kvp.Key == "start_with") {
                InitializeDialogue(kvp.Key, kvp.Value);
                break;
            }
            /*
            Debug.Log(kvp.Key);
            foreach(DialogueItem di in kvp.Value) {
                string d = di.speaker + ": " + di.text;
                Debug.Log(d);
            }
            */
        }
        if (m_CurrentDialogueItems == null) {
            HideTextbox();
            HideCursor();
            HideSpeaker();
        }

        // Based on our selected `m_NextButton`, we need to adjust the cursor's text to match it.
        switch(m_nextButton) {
            case (nextButtonOption.A_Down):
                m_DialogueCursor.textbox.text = "A";
                CustomEvents.current.onRightOneDown += NextAction;
                CustomEvents.current.onRightPointerTargetChanged += CheckPointerTarget;
                break;
            case (nextButtonOption.A_Up):
                m_DialogueCursor.textbox.text = "A";
                CustomEvents.current.onRightOneUp += NextAction;
                CustomEvents.current.onRightPointerTargetChanged += CheckPointerTarget;
                break;
            case (nextButtonOption.B_Down):
                m_DialogueCursor.textbox.text = "B";
                CustomEvents.current.onRightTwoDown += NextAction;
                CustomEvents.current.onRightPointerTargetChanged += CheckPointerTarget;
                break;
            case (nextButtonOption.B_Up):
                m_DialogueCursor.textbox.text = "B";
                CustomEvents.current.onRightTwoUp += NextAction;
                CustomEvents.current.onRightPointerTargetChanged += CheckPointerTarget;
                break;
            case (nextButtonOption.X_Down):
                m_DialogueCursor.textbox.text = "X";
                CustomEvents.current.onLeftOneDown += NextAction;
                CustomEvents.current.onLeftPointerTargetChanged += CheckPointerTarget;
                break;
            case (nextButtonOption.X_Up):
                m_DialogueCursor.textbox.text = "X";
                CustomEvents.current.onLeftOneUp += NextAction;
                CustomEvents.current.onLeftPointerTargetChanged += CheckPointerTarget;
                break;
            case (nextButtonOption.Y_Down):
                m_DialogueCursor.textbox.text = "Y";
                CustomEvents.current.onLeftTwoDown += NextAction;
                CustomEvents.current.onLeftPointerTargetChanged += CheckPointerTarget;
                break;
            case (nextButtonOption.Y_Up):
                m_DialogueCursor.textbox.text = "Y";
                CustomEvents.current.onLeftTwoUp += NextAction;
                CustomEvents.current.onLeftPointerTargetChanged += CheckPointerTarget;
                break;
        }

        m_isActive = true;
    }

    #endregion

    #region Utility Functions

    public void ShowTextbox() {
        m_MainTextbox.gameObject.SetActive(true);
    }
    public void ShowCursor() {
        m_DialogueCursor.gameObject.SetActive(true);
    }
    public void ShowSpeaker() {
        m_Speaker.gameObject.SetActive(true);
    }

    public void HideTextbox() {
        Debug.Log("HIDING TEXTBOX");
        m_MainTextbox.gameObject.SetActive(false);
    }
    public void HideCursor() {
        Debug.Log("HIDING CURSOR");
        m_DialogueCursor.gameObject.SetActive(false);
    }
    public void HideSpeaker() {
        Debug.Log("HIDING SPEAKER");
        m_Speaker.gameObject.SetActive(false);
    }

    public void InitializeDialogue(string key, DialogueItem[] d) {
        m_CurrentKey = key;
        m_CurrentDialogueItems = d;
        m_CurrentIndex = -1;
        m_InProgress = false;

        NextAction();
    }

    public void EndDialogue() {
        m_CurrentKey = null;
        m_CurrentDialogueItems = null;
        m_CurrentIndex = -1;
        m_InProgress = false;

        HideTextbox();
        HideCursor();
        HideSpeaker();
    }

    private void CheckPointerTarget(OVRInput.Controller c, GameObject go) {
        // If we don't have any grabbers that we're using with this dialogue option, we gotta go default to null
        if (m_Grabbers.Count == 0) {
            m_BeingPointedBy = null;
            return;
        }

        // We check if any of the grabbers we associated with the Dialogue system match the receiving controller
        CustomGrabber g = null;
        foreach(CustomGrabber cg in m_Grabbers) {
            if (cg.OVRController == c) {
                g = cg;
                break;
            }
        }
        // If not, we return early
        if (g == null) {
            m_BeingPointedBy = null;
            return;
        }

        /*
        // If our received game object matches our textbox background, we assign being pointed by to this.
        if (go != null && go.GetInstanceID() == m_MainTextbox.textboxBackground.GetInstanceID()) {
            m_BeingPointedBy = g.pointer;
        } else {
            m_BeingPointedBy = null;
        }
        // If our received game object as a DialogueTarget object, then we need to also assign that.
        if (go != null && go.GetComponent<DialogueTarget>() != null) {
            m_DialogueTarget = go.GetComponent<DialogueTarget>();
        } else {
            m_DialogueTarget = null;
        }
        */

        return;
    }

    /*
    private void CheckIfPointedAt() {
        if (m_Grabbers.Count == 0) {
            m_BeingPointedBy = null;
            return;
        }
        foreach (CustomGrabber g in m_Grabbers) {
            if (
                g.pointer != null &&    // check that our pointer isn't NULL
                g.pointer.GetPointerType() == "Target" &&   // Check that our pointer type is neither locomotion-based or set_target-based
                g.pointer.raycastTarget != null &&  // check that we do have a raycast target
                g.pointer.raycastTarget.GetInstanceID() == m_DialogueBackground.gameObject.GetInstanceID()  // that we're actually hitting the dialogue background
            ) {
                m_BeingPointedBy = g.pointer;
                break;
            }
            else {
                m_BeingPointedBy = null;
            }
        }
        return;
    }
    */

    private bool CheckGrabber(OVRInput.Controller c) {
        bool found = false;
        foreach(CustomGrabber cg in m_Grabbers) {
            if (cg.OVRController == c) {
                found = true;
                break;
            }
        }
        return found;
    }

    private void NextAction() {
        if (m_InProgress) return;
        if (m_CurrentDialogueItems != null) StartCoroutine(ActionEvent());
        else if (m_DialogueTarget != null) GetDialogueFromTarget();
    }

    private void NextAction(OVRInput.Controller c) {
        // We have to return early if we encounter any of the following situations:
        // - our received controller doesnt correspond with any of our grabbers
        // - if we're in the middle of typing out onto the checkbox
        if (!CheckGrabber(c) || m_InProgress ) return;
        
        // We're in a particular situation:
        // - if our Current Dialogue Items is null (aka we're not typing anything atm), then we might be clicking our interaction button
        //          on a DialogueTarget
        // - If our Current Dialogue Items is NOT null, then we're in the middle of a dialogue stream. In that case, we have to advance the next action item
        if (m_CurrentDialogueItems != null) StartCoroutine(ActionEvent());
        else if (m_DialogueTarget != null) GetDialogueFromTarget();
    }

    private void GetDialogueFromTarget() {
        if (m_DialogueTarget == null || m_DialogueTarget.dialogueKey == null) return;
        string targetKey = m_DialogueTarget.dialogueKey;
        DialogueItem[] d;
        if (dialogue.TryGetValue(targetKey, out d)) {
            ShowTextbox();
            InitializeDialogue(targetKey, d);
        }
    }

    private IEnumerator ActionEvent() {
        // Set the action status so that we're in the middle of typing
        m_InProgress = true;

        // Set our next index
        m_CurrentIndex += 1;

        // If our current index is greater than the list of dialogue items, we initialize the end of the coroutine;
        if (m_CurrentIndex >= m_CurrentDialogueItems.Length) {
            EndDialogue();
            yield break;
        }

        // Get our current dialogue item based on the index
        DialogueItem di = m_CurrentDialogueItems[m_CurrentIndex];

        // Set our speaker text to speaker from our DialogueItem
        if (di.speaker != null) {
            ShowSpeaker();
            m_Speaker.textbox.text = di.speaker;
        } else {
            HideSpeaker();
        }

        // Make sure that our textbox is visible
        ShowTextbox();
        HideCursor();

        string currentString = "";
        // Parse our dialogue item's text
        foreach(char c in di.text) {
            currentString += c;
            m_MainTextbox.textbox.text = currentString;
            yield return new WaitForSeconds(0.01f);
        }

        // Reset action status so that we can continue to the next action
        m_InProgress = false;
        ShowCursor();
    }

    #endregion

}
