using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ------------
- The purpose of this component is to control the locomotion function of our custom EasierVRAssets package.
- In virtual reality, locomotion refers to the ability to teleport from one location to another in the VR space without having to physically move your body.
- Simply put, when the user starts pointing (or holding down the trigger button on the Oculus controller),
    this script renders the appropriate prefabs, assets, and visualizations that indicate where the user is targeting at as their teleportation destination.
    When the user releases the trigger, the script teleports (either instantly or via transition) the player to the target destination of that pointer.
- This script replaces Oculus's teleportation script due to Oculus' implementation containing several issues with rigidbodies,
    instant teleportation causing collisions during the fade-in and fade-out transition, alongside other bugs that were present at the time this component was written.
------------ */
public class CustomLocomotion : MonoBehaviour
{   
    #region Variables and References

    /// We need a reference to Oculus' OVRPlayer component that controls and dictates behavior of the player's avatar in-game.
    /// This is needed because this component overwrites certain values and functions that we typically try to modify in Unity such as the player's Vector3 position.
    [SerializeField] [Tooltip("Reference to OVRPlayer")]
    private CharacterController m_characterController;

    /// We need a reference to Oculus' OVRPlayerController component that dictates input functions such as the ability to rotate and move using controller joysticks
    [SerializeField] [Tooltip("Reference to OVRPlayerController")]
    private OVRPlayerController m_playerController;
    
    /// This is Oculus' screen fader component that we need to access to control screen fade-ins and outs for locomotion.
    [SerializeField] [Tooltip("Reference to Fade script")]
    private OVRScreenFade m_screenFader;
    public OVRScreenFade screenFader {
        get {   return m_screenFader;   }
        set {}
    }

    /// We reference our own custom grabber component that replaces Oculus' OVRGrabber. This is for detecting inputs from the controller.
    [SerializeField] [Tooltip("Controller for locomotion")]
    private CustomGrabber m_customGrabber;
    public CustomGrabber customGrabber {
        get {   return m_customGrabber;     }
        set {   m_customGrabber = value;    }
    }
    
    /// We create a storage variable that is used to store the destination of our locomotion function
    [Tooltip("Storage variable to destination of locomotion")]
    private Vector3 m_destination = Vector3.zero;
    public Vector3 destination {
        get {   return m_destination;   }
        set {   m_destination = value;  }
    }

    /// We create a boolean variable that determines if our custom pointer, used as a targeting line to determine destination of locomotion, is hitting the ground or not.
    [Tooltip("Storage variable for storing whether the raycast downward from pointer is detecting something")]
    private bool m_floorBelow = false;

    /// We store a reference to the prefab that is the cursor for our locomotion function. When we start pointing at where we want to teleport to, this prefab is instantiated and rendered.
    [SerializeField] [Tooltip("Reference to prefab for locomotion cursor")]
    private ExternalCollider m_locomotionCursorPrefab;

    /// Our instantiated cursor is stored in this variable
    [Tooltip("Instantiated locomotion cursor")]
    private ExternalCollider m_instantiatedCursor = null;

    /// These are two booleans that store whether we're in the middle of holding our trigger and selecting our destination or if we're in the middle of teleporting, respectively.
    [Tooltip("Boolean to detecting if we're getting ready to teleport")]
    private bool m_isPreparing = false, m_isTeleporting = false;
    public bool isPreparing {
        get {   return m_isPreparing;   }
        set {   m_isPreparing = value;  }
    }
    public bool isTeleporting {
        get {   return m_isTeleporting;     }
        set {   m_isTeleporting = value;    }
    }

    /// The reference to the index finger trigger that controls the locomotion function.
    [SerializeField] [Tooltip("Locomotion Trigger Button - default = index trigger")]
    private OVRInput.Button m_locomotionTrigger = OVRInput.Button.PrimaryIndexTrigger;

    /// What color should the rendered reticle be when it is okay for the user to teleport?
    [SerializeField] [Tooltip("Indicator color for okay locomotion")]
    private Material m_validLocomotionColor;

    /// Same as `m_validLocomotionColor`, but for when the target destination is not suitable for teleporting to.
    [SerializeField] [Tooltip("Indicator color for invalid locomotion")]
    private Material m_invalidLocomotionColor;

    /* ---
    - A problem with Oculus' original implementation of VR teleportation is that "instant" teleportation is actually a 3D transition, supersped to appear as if it is instant.
    - This is a problem when objects with rigidbodies are in the pathway between the player's current position and the destination.
    - Because the player's body transitions rather than simply sets the player's position, the avatar's collider can hit objects with colliders and rigidbodies.
    - To remedy this, we have a list of RigidBodies attached to our player avatar. When we teleport, we disable physics on all these RigidBodies by setting their kinematic settings to true.
    - With this new system, regardless of whether we're instantly teleporting or transitioning, we ensure that we are not accidentally hitting any objects that happen to be in the way.
    --- */
    [SerializeField] [Tooltip("All rigidbodies that must be set to kinematic")]
    private List<Rigidbody> m_rbs = new List<Rigidbody>();
 
    /// When we start locomotion, we store the original Kinematic statuses of each of the RigidBodies inside of `m_rbs`.
    /// When we finish locomoting, we set the kinematic values back to what they originally were.
    [Tooltip("private reference for all initial rigidbody statuses")]
    private Dictionary<int, bool> m_originalKinStat = new Dictionary<int, bool>();

    /// The thumbstick's orientation on the hand we are holding the trigger on. We need this to determine the direction our avatar is facing when we finish locomoting.
    /// The player has the ability to orient their avatar to a desired rotation upon teleporting. The thumbstick controls that orientation in the targeting reticle.
    [Tooltip("Stores the thumbstick's position")]
    private Vector2 m_thumbstickPosition;

    /// This is legacy code, but the version of the code prior to this current version had our custom Grabber component and this script combined into one class.
    /// Now, since the Grabber code is separated from this code, this code is out of date, but it would have determined whether this was a simple striaght pointer or a locomotion reticule.
    private enum pointerType {
        Target,
        Teleport
    }

    /// This was planned for later, but the method to initialize the locomotion function was planned to be modifyable.
    /// For now, the locomotion is triggered by holding the index trigger of the Oculus controller to initialize the pointing reticule, then releasing to start the locomotion.
    /*
    private enum activationType {
        Hold_Release,
        Double_Press,
        Two_Buttons
    }
    [SerializeField] [Tooltip("The method to activate and initialize locomotion")]
    private activationType m_activationType;
    */

    /// The method of locomotion is customizable between instant teleportation, fade-in fade-out (instant with screen transitions), and Smoothly damping from the start position to the destination.
    private enum locomotionType {
        Instant,
        Fade,
        SmoothDamp,
    }
    [SerializeField] [Tooltip("Determine the type of locomotion")]
    private locomotionType m_locomotionType;

    /// What's the maximum distance the pointer can point towards?
    [SerializeField] [Tooltip("Total distance the pointer is allowed to travel")]
    private float m_maxDistance = 5f;

    /// How long does it take for any transitional type of locomotion (Fade, SmoothDamp) to transition?
    [SerializeField] [Tooltip("Time to transition for non-Instant locomotion types")]
    private float m_transitionTime;

    /// This controls the fade-to-black of the "Fade" locomotion type. We set keyframes such that the fade level is 0, then 1, then 0 within the time frame determined by m_transitionTime.
    [SerializeField] [Tooltip("Fade to black over the duration of the transition")]
	private AnimationCurve FadeLevels = new AnimationCurve(new Keyframe[3] { new Keyframe(0,0), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.0f) });

    /// We are storing a reference to `m_characterController`'s transform component. Overkill, but easier to handle.
    [Tooltip("Reference to character transform")]
    private Transform m_charTransform;

    /// When we do the locomote, we are essentially overwriting some values in the OVRPlayerController.
    /// To ensure that we're not messing up anything, we store the OVRPlayerController's initially set linear movement and rotational movement booleans here, then re-apply them when we finish the locomote.
    [Tooltip("Saved memory of the initial conditions for movement with the OVRPlayerController")]
    private bool m_linMov, m_rotMov;

    /// Should this component be active upon initialization of the program? There may be times when we disable this component until necessary, such as when we're waiting for a loading screen for example.
    [SerializeField] [Tooltip("Bool to determine if the locomotion should initialize ")]
    private bool m_shouldStartOnRun = true;

    #endregion
    /* ------------
    - When the user pressed DOWN on the trigger, locomotion is PREPARING. During this prep period, a cursor is meant to appear. There can only be one cursor in the world, which is what we store in `m_instantiatedCursor`.
    - When the user lets go of the trigger, locomotion is TELEPORTING. In this situation, the press-down can still be registered - that is fine. However, we cannot ignore the possibility that the preparing state can be re-instated multiple times.
    - So, here's how we're gonna do it:
        1) IS PREPARING is first initiated
            - Check if the grabbers correspond with each other, as a precaution.
            - Check if isPreparing is true - if it is, then we return early.
            - Assuming that we haven't started prepping yet, we instantiate a new cursor that's automatically hidden from the world view.
        2) During the Update() sequence:
            - To save time in the system, we want to return early if the player is not prepping for locomotion.
            - We update the destination and whether we're hitting a floor or not
            - If we're hitting a floor, we alter the location of the cursor to the point of contact between the pointer and the floor. This tells us where we'll be teleporting to.
    ------------ */
    #region Unity callbacks

    /// If we're set to start upon program initialization, do so.
    private void Start() {
        if (m_shouldStartOnRun) Init();
    }

    /// The Update loop occurs every frame. It is imperative that we do not block the loop, meaning we need to have checks such as early returns upon conditionals.
    private void Update() {

        // Update doesn't run if we're currently not finding a location to teleport to.
        if (!m_isPreparing) {
            return;
        }

        // If we're currently teleporting, we hide our cursor and end early.
        if (m_isTeleporting) {
            HideCursor();
            return;
        }
        
        // Get the current pointer's destination.
        // The pointer attached to our Custom Grabber has two public variables: <code>locomotionPosition</code>, and <code>raycastDownwardHit</code>.
        //      - <code>locomotionPosition</code> = Vector3: where the pointer is hitting the floor (if it is hitting a floor to begin with)
        //      - <code>raycastDownwardHit</code> = Boolean: are we hitting the floor with our pointer?
        m_destination = m_customGrabber.pointer.locomotionPosition;
        m_floorBelow = m_customGrabber.pointer.raycastDownwardHit;

        // Move the instantiated locomotion cursor to the destination only if a downward raycast is detected (aka if <code>m_floorBelow</code> is TRUE)
        if (m_floorBelow) {
            ShowCursor();
            m_instantiatedCursor.transform.position = m_destination;
        } else {
            HideCursor();
        }
    }

    #endregion
    #region Initializers

    /// We call this function whenever we want to initialize the locomotion of our VR game.
    public void Init() {
        m_charTransform = m_characterController.transform;          // set the reference
        m_linMov = m_playerController.EnableLinearMovement;         // Save the linear and rotational movement settings
        m_rotMov = m_playerController.EnableRotation;
        
        // We save the rigidbody's initial kinematic settings inside of `m_originalKinStat`
        foreach(Rigidbody rb in m_rbs) {
            m_originalKinStat.Add(rb.gameObject.GetInstanceID(), rb.isKinematic);
        }
        
        // `m_customGrabber.pointer` == our custom grabber component's Line Renderer component.
        // The pointer is not attached to this component - rather, our Custom Grabber already has the pointer installed with it.
        // All we need to do is adjust the pointer's settings from HERE, and we're ready to locomote. But we need to make sure that we're not referencing a null object
        if (m_customGrabber != null) {
            
            m_customGrabber.pointer.SetPointerType("Teleport");         // set the pointer type of our chosen hand to "Teleport"
            m_customGrabber.pointer.SetLaserType("Parabolic");          // set the pointer's laser appearance to "Parabolic" (aka it drops down to the floor and uses the point of contact between the floor and the line as our target destination)
            m_customGrabber.pointer.raycastDistance = m_maxDistance;    // We set the hand's raycast distance to our max distance.

            // This is a bit confusing conceptually, so I'll explain here.
            // Oculus' OVR Implementation package comes with the "OVRController" class. This class is attached to the left and right hand objects in the game and can be classified into different types: "LTouch" == the "Left hand" controller, and "RTouch" == the "Right hand" controller
            // In other words, OVRController == OVRInput.Controller.LTouch (to indicate this is the controller for the left hand), or OVRController == OVRInput.Controller.RTouch (this is the controller for the right hand).
            // However, we aren't using OVRController to control our player - we are using a custom component I wrote in actuality. We are keeping it though because without it, the Oculus Implementation package will not work properly in-game. So we're forced to keep it despite it not being used.

            // We have a singleton class that acts as an event handler related to button presses (ex. "button A press down", "button trigger lifted"). We need to attribute our locomotion functions to these events.
            // Based on whether we're controlling locomotion with the left or right hand, we attach this class's functions to the matching button events.
            switch(m_customGrabber.OVRController) {
                case(OVRInput.Controller.LTouch):
                    CustomEvents.current.onLeftTriggerDown += PrepareTeleport;  // Teleport Preparations listening for trigger down
                    CustomEvents.current.onLeftTriggerUp += Teleport;           // Teleport listening for trigger up
                    break;
                case(OVRInput.Controller.RTouch):
                    CustomEvents.current.onRightTriggerDown += PrepareTeleport; // Teleport Preparations listening for trigger down
                    CustomEvents.current.onRightTriggerUp += Teleport;          // Teleport listening for trigger up
                    break;
            }
        }
    }

    #endregion
    #region Preparing Teleportation

    /// This is called when we first initially hold down on the index trigger button.
    public void PrepareTeleport(OVRInput.Controller c) {
        if (c != m_customGrabber.OVRController || m_isPreparing) return;
        m_instantiatedCursor = InstantiateCursor();
        m_isPreparing = true;
        return;
    }   
    /// This is called when we need to instantiate a new cursor for our locomotion - called from `PrepareTeleport()` above.
    private ExternalCollider InstantiateCursor() {
        if (m_instantiatedCursor != null) return m_instantiatedCursor;
        ExternalCollider newCursor =  Instantiate(m_locomotionCursorPrefab, m_customGrabber.pointer.locomotionPosition, Quaternion.identity) as ExternalCollider;
        HideCursor(newCursor);
        return newCursor;
    }
    /// This hides the cursor for locomotion. This is useful for when the user is in the middle of teleporting and shouldn't be able to readjust their destination mid-way.
    private void HideCursor(ExternalCollider ec = null) {
        ec = (ec == null) ? m_instantiatedCursor : ec;
        if (ec == null) return;
        foreach(Renderer r in ec.GetComponentsInChildren<Renderer>()) {
            r.enabled = false;
        }
        return;
    }
    /// This shows the cursor for locomotion.
    private void ShowCursor(ExternalCollider ec = null) {
        ec = (ec == null) ? m_instantiatedCursor : ec;
        if (ec == null) return;
        foreach(Renderer r in ec.GetComponentsInChildren<Renderer>()) {
            r.enabled = true;
        }
    }
    /// We destroy the cursor when we don't need it anymore.
    private void DestroyCursor() {
        Destroy(m_instantiatedCursor.gameObject);
        m_instantiatedCursor = null;
    }
    /// Legacy code - initially, the code to initialize a new cursor was written here. However, it has since been re-written elsewhere.
    private IEnumerator QueueNewCursor() {
        while(m_isTeleporting) {
            yield return null;
        }
    }

    #endregion
    #region Teleportation

    /* ---
    - This is called when we let go of the index trigger's button. The flow of functions: 
        1. Teleport(...) - stops preparation phase, initializes locomotion if pointer is hitting a floor. Calls `ActivateLocomotion()`
        2. ActivateLocomotion(...) - Coroutine that adjust CharacterController Settings and Locomotes. Calls `LocomoteFromFloor()`
        3. LocomoteFromFloor(...) - moves the player to the destination with respect to the floor's location. Calls `DoTeleport()`
        4. DoTeleport(...) - Based on whichever locomotion setting we set up with <code>m_ocomotionType</code>, we call either `InstantTeleport()`, `FadeTeleport()`, or `SoothDampTeleport()`
        5. InstantTeleport(...), FadeTeleport(...), or SmoothDampTeleport(...) - does the locomoting.
        6. We return to `ActivateLocomotion()`, which resets CharacterController Settings, re-activates the pointer, and resets this script for locomotion once more.
    --- */
    public void Teleport(OVRInput.Controller c) {
        if (c != m_customGrabber.OVRController || !m_isPreparing || m_isTeleporting) return;
        if (m_instantiatedCursor != null) {
            Destroy(m_instantiatedCursor.gameObject);
            m_instantiatedCursor = null;
        }
        m_isPreparing = false;
        // We only teleport if we have a floor below us. We use a coroutine to allow locomotion to run over multiple frames, thereby preventing it from being a blocker in Unity's Update loops
        if (m_floorBelow)   StartCoroutine(ActivateLocomotion(m_destination));
        return;
    }
    /// The function that starts the locomotion process. This is a coroutine to prevent blocking and allow it to work over multiple frames.
    private IEnumerator ActivateLocomotion(Vector3 pos) {
        // Deactivate thumbstick movement and rotation
        ChangePlayerMovement(false, false);
        // Deactivate custom grabber's custom pointer
        m_customGrabber.pointer.Deactivate();
        LocomoteFromFloor(pos);
        while (m_isTeleporting) {
            yield return null;
        }
        // Reset movement
        ResetPlayerMovement();
        m_customGrabber.pointer.Activate();
    }

    /// The main locomotion function used by this component. The function will auto-adjust the destination to account for the player's height, which is set inside of Oculus' <code>CharacterController</code>.
    public void LocomoteFromFloor(Vector3 floorDestination) {
        if (!m_isTeleporting) {
            m_isTeleporting = true;
            Vector3 destination = new Vector3(floorDestination.x, floorDestination.y + m_characterController.height*0.5f, floorDestination.z);
            DoTeleport(destination);
        }
    }
    /// If, in the advent that we want to teleport the player through script rather than player action, we can do so here.
    public void LocomoteDirectly(Vector3 destination) {
        if (!m_isTeleporting) {
            m_isTeleporting = true;
            DoTeleport(destination);
        }
    }

    /// The big kahuna, this is used in the `Teleport()` flow to actually initialize movement based on locomotion settings from `m_locomotionType`
    public void DoTeleport(Vector3 destination) {
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        switch(m_locomotionType) {
            case(locomotionType.Instant):
                StartCoroutine(InstantTeleport(destination));
                break;
            case(locomotionType.Fade):
                StartCoroutine(FadeTeleport(destination));
                break;
            case(locomotionType.SmoothDamp):
                StartCoroutine(SmoothDampTeleport(destination));
                break;;
        }
    }
    /// Similar to DoTeleport(...), except that we can determine the type of locomotion we want to execute if needed.
    public void DoTeleportWithType(string type, Vector3 destination) {
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        switch(type) {
            case("Instant"):
                StartCoroutine(InstantTeleport(destination));
                break;
            case("Fade"):
                StartCoroutine(FadeTeleport(destination));
                break;
            case("SmoothDamp"):
                StartCoroutine(SmoothDampTeleport(destination));
                break;
        }
    }

    /// Instant teleportation - set the player's avatar at the destination
    public IEnumerator InstantTeleport(Vector3 destination) {
        m_characterController.enabled = false;
        m_charTransform.position = destination;
        m_characterController.enabled = true;
        //m_locomotionIndicator.ResetStatus();
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
        m_isTeleporting = false;
        yield return null;
    }
    /// Similar to `InstantTeleport()`, except with a fade-in and fade-out effect on the user's visuals.
    public IEnumerator FadeTeleport(Vector3 destination) {
		float elapsedTime = 0;
        bool teleported = false;
		while (elapsedTime < m_transitionTime)
		{
            yield return null;
			elapsedTime += Time.deltaTime;
			if (!teleported && elapsedTime >= m_transitionTime*0.5f)
			{
				teleported = true;
                m_characterController.enabled = false;
				m_charTransform.position = destination;
                m_characterController.enabled = true;
			}
			float fadeLevel = FadeLevels.Evaluate(elapsedTime / m_transitionTime);
			m_screenFader.SetFadeLevel(fadeLevel);
		}

		m_screenFader.SetFadeLevel(0);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
        m_isTeleporting = false;
    }
    /// Smoothly move the player's avatar from their current position to the destination
    public IEnumerator SmoothDampTeleport(Vector3 destination) {
        Vector3 velocity = Vector3.zero;
        m_characterController.enabled = false;
        while(Vector3.Distance(m_charTransform.position, destination) > 0.01f) {
            yield return null;
            m_charTransform.position = Vector3.SmoothDamp(m_charTransform.position, destination, ref velocity, m_transitionTime);
        }
        m_characterController.enabled = true;
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
        m_isTeleporting = false;
    }

    #endregion
    #region Setting Modifiers

    /// This function allows the switching between different locomotion types, if needed.
    public void ChangeLocomotionType(string type) {
        switch(type) {
            case("Instant"):
                m_locomotionType = locomotionType.Instant;
                break;
            case("Fade"):
                m_locomotionType = locomotionType.Fade;
                break;
        }
        return;
    }
    /// This function alters the movement and rotation of the player using the joystick.
    /// This is necessary because we don't want the player to be able to move or rotate using the joystick in the middle of teleporting.
    public void ChangePlayerMovement(bool mov, bool rot) {
        m_playerController.EnableLinearMovement = mov;
        m_playerController.EnableRotation = rot;
        if (!mov) {
            m_customGrabber.pointer.Deactivate();
        } else {
            m_customGrabber.pointer.Activate();
        }
    }
    /// A function that defaults the player movement to allow them to move and point again. A simplified version of `ChangePlayerMovement()`
    public void ResetPlayerMovement() {
        m_playerController.EnableLinearMovement = m_linMov;
        m_playerController.EnableRotation = m_rotMov;
        m_customGrabber.pointer.Activate();
    }

    #endregion
}