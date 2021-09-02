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
public class EVRA_Locomotion : MonoBehaviour
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

    [SerializeField] [Tooltip("Reference to the EVRA_Hand that will be controlling the locomotion")]
    private EVRA_Hand m_Hand = null;

    [SerializeField] [Tooltip("Reference to the pointer we'll be using as our destination pointer")]
    private EVRA_Pointer m_Pointer;

    /// We store a reference to the prefab that is the cursor for our locomotion function. When we start pointing at where we want to teleport to, this prefab is instantiated and rendered.
    [SerializeField] [Tooltip("The locomotion cursor to indicate where the locomotion destination will be")]
    private GameObject m_locomotionCursor = null;
    [Tooltip("Ref to the locomotion cursor's `LocomotionCursor` component, if it has one")] // NOT SERIALIZED
    private EVRA_LocCursor m_cursorComponent = null;
    
    /// We create a storage variable that is used to store the destination of our locomotion function
    [Tooltip("The target object of the destination of locomotion")] // NOT SERIALIZED
    private Transform m_destinationTarget = null;
    [Tooltip("The point position for the destination of locomotion")] // NOT SERIALIZED
    private Vector3 m_destinationPoint = Vector3.zero;

    /// Our check for height while moving or at the teleportation destination
    public enum CheckHeight {
        DoNotCheck,
        AtDestination
    }
    [SerializeField] [Tooltip("Should our pointer be concerned about whether our person can physically fit at the designated teleportation location?")]
    private CheckHeight m_checkHeight = CheckHeight.AtDestination;

    [Tooltip("What's the minimum height tolerance (in % of original height), if checking for height during movement")]
    public float heightTolerancePercent = 0.5f;

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

    [SerializeField] [Tooltip("Should the cursor, if it contains the `LocomotionCursor` component, change the color to indicate if it's a valid teleportation or not?")]
    private bool m_showIfInvalid = true;
    [SerializeField] [Tooltip("What color should show up on the locomotion cursor if it's a valid teleporation location?")]
    private Color m_validColor = new Color(0f, 0.07f, 1f, 0.25f);
    [SerializeField] [Tooltip("What color should show up on the locomotion cursor if it's an invalid teleporation location?")]
    private Color m_invalidColor = new Color(1f, 0.19f, 0f, 0.25f);

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

    /*
    /// The thumbstick's orientation on the hand we are holding the trigger on. We need this to determine the direction our avatar is facing when we finish locomoting.
    /// The player has the ability to orient their avatar to a desired rotation upon teleporting. The thumbstick controls that orientation in the targeting reticle.
    [Tooltip("Stores the thumbstick's position")]
    private Vector2 m_thumbstickPosition;
    */

    /*
    /// The method of locomotion is customizable between instant teleportation, fade-in fade-out (instant with screen transitions), and Smoothly damping from the start position to the destination.
    private enum LocomotionType {
        Instant,
        Fade,
        SmoothDamp,
    }
    [SerializeField] [Tooltip("Determine the type of locomotion")]
    private LocomotionType m_locomotionType = LocomotionType.Instant;

    /// How long does it take for any transitional type of locomotion (Fade, SmoothDamp) to transition?
    [SerializeField] [Tooltip("Time to transition for non-Instant locomotion types. If the Locomotion Type is set to 'Instant', then this is ignored.")]
    private float m_transitionTime = 0.25f;
    */

    [Tooltip("Is our teleportation possible?")] // NOT SERIALIZED
    private bool isPossible = true;

    /// This controls the fade-to-black of the "Fade" locomotion type. We set keyframes such that the fade level is 0, then 1, then 0 within the time frame determined by m_transitionTime.
    [SerializeField] [Tooltip("Fade to black over the duration of the transition")]
	private AnimationCurve FadeLevels = new AnimationCurve(new Keyframe[3] { new Keyframe(0,0), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.0f) });

    /// We are storing a reference to `m_characterController`'s transform component. Overkill, but easier to handle.
    [Tooltip("Reference to character transform")]
    private Transform m_charTransform;

    [Tooltip("Saved memory of the character height, based on `height` from OVRCharcterController")] // NOT SERIALIZED
    private float m_charHeight;

    /// When we do the locomote, we are essentially overwriting some values in the OVRPlayerController.
    /// To ensure that we're not messing up anything, we store the OVRPlayerController's initially set linear movement and rotational movement booleans here, then re-apply them when we finish the locomote.
    [Tooltip("Saved memory of the initial conditions for movement with the OVRPlayerController")]
    private bool m_linMov, m_rotMov;

    [Tooltip("The max speed of the player, if locomoting via translation, in MPS")]
    public float maxSpeed = 0.75f;

    [Tooltip("The modifier that determines how much of the max speed the player should move. 0f = stop, 1f = full speed")] // NOT SERIALIZED
    private float speedModifier = 1f;

    [Tooltip("Reference to the trigger that is activating the locomotion, if locomoting via translation")]
    private string m_translationInputName = null;

    [Tooltip("Used as a check for if the player is locomoting using translation.")] // NOT SERIALIZED
    private bool m_isTranslating = false;

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

    /// We call this function whenever we want to initialize the locomotion of our VR game.
    private void Awake() {
        m_cursorComponent = (m_locomotionCursor) ? m_locomotionCursor.GetComponent<EVRA_LocCursor>() : null;

        m_charTransform = m_characterController.transform;          // set the references
        m_charHeight = m_characterController.height;           // set the height of 
        m_linMov = m_playerController.EnableLinearMovement;         // Save the linear and rotational movement settings
        m_rotMov = m_playerController.EnableRotation;
        
        // We save the rigidbody's initial kinematic settings inside of `m_originalKinStat`
        foreach(Rigidbody rb in m_rbs) {
            m_originalKinStat.Add(rb.gameObject.GetInstanceID(), rb.isKinematic);
        }
    }

    /// The Update loop occurs every frame. It is imperative that we do not block the loop, meaning we need to have checks such as early returns upon conditionals.
    private void Update() {
        // We don't even run this if our pointer is null
        if (m_Pointer == null) return;

        // Update doesn't run if we're currently not finding a location to teleport to.
        if (!m_isPreparing) return;
        if (m_isTeleporting) {
            if (m_isTranslating) {
                Vector3 direction = -m_Pointer.transform.forward;
                if (m_Hand != null && m_translationInputName != null) {
                    switch(m_translationInputName) {
                        case "index":
                            speedModifier = m_Hand.IndexTriggerValue;
                            break;
                        case "grip":
                            speedModifier = m_Hand.GripTriggerValue;
                            break;
                        case "thumbstickPress":
                            speedModifier = (m_Hand.ThumbstickPress) ? 1f : 0f;
                            break;
                        case "one":
                            speedModifier = (m_Hand.ButtonOneValue) ? 1f : 0f;
                            break;
                        case "two":
                            speedModifier = (m_Hand.ButtonTwoValue) ? 1f : 0f;
                            break;
                        case "start":
                            speedModifier = (m_Hand.StartButtonValue) ? 1f : 0f;
                            break;
                        default:
                            speedModifier = 1f;
                            break;
                    }
                }
                else speedModifier = 1f;
                float timePassed = Time.deltaTime;
                TranslateTeleport(direction, maxSpeed * speedModifier, timePassed);
            }
            return;
        }
        
        // We grab both the targeted Transform alongside the specific point where our raycast is pointing to.
        // No concern needs to be held for whether our line is straight or a Bezier curve downwards; the pointer accounts for that by giving us the appropriate target regardless of the line type
        m_destinationTarget = m_Pointer.downwardRaycastTarget;
        m_destinationPoint = m_Pointer.downwardRaycastHitPosition;
        // If our target isn't null (aka we're aren't pointing to space), we show our cursor
        // Otherwise, we need to hide our cursor
        if (m_destinationTarget != null) {
            CheckIfValid(m_destinationPoint);
            ShowCursor(m_destinationPoint);
        }
        else HideCursor();
    }

    #endregion

    #region Preparing Teleportation

    public void StartDestinationSelection() {
        if (m_Pointer == null || m_isTeleporting) return;
        m_isPreparing = true;
    }

    #endregion
    #region Teleportation

    /* ---
    - This is called when we invoke the event meant to start the teleportion. The flow of functions: 
        1. StartTeleport(...) - stops preparation phase, initializes locomotion if pointer is hitting a floor. Calls `ActivateLocomotion()`
        2. ActivateLocomotion(...) - Coroutine that adjust CharacterController Settings and Locomotes. Calls `LocomoteFromFloor()`
        3. LocomoteFromFloor(...) - moves the player to the destination with respect to the floor's location. Calls `DoTeleport()`
        4. DoTeleport(...) - Based on whichever locomotion setting we set up with <code>m_ocomotionType</code>, we call either `InstantTeleport()`, `FadeTeleport()`, or `SoothDampTeleport()`
        5. InstantTeleport(...), FadeTeleport(...), or SmoothDampTeleport(...) - does the locomoting.
        6. We return to `ActivateLocomotion()`, which resets CharacterController Settings, re-activates the pointer, and resets this script for locomotion once more.
    --- */
    /*
    //public void Teleport(OVRInput.Controller c) {
    public void StartTeleport() {
        HideCursor();
        m_isPreparing = false;
        if (m_isTeleporting || !isPossible || m_destinationTarget == null) return;
        // We use a coroutine to allow locomotion to run over multiple frames, thereby preventing it from being a blocker in Unity's Update loops
        StartCoroutine(ActivateLocomotion(m_destinationPoint));
        return;
    }
    /// The function that starts the locomotion process. This is a coroutine to prevent blocking and allow it to work over multiple frames.
    private IEnumerator ActivateLocomotion(Vector3 pos) {
        // Deactivate thumbstick movement and rotation
        ChangePlayerMovement(false, false);
        // Deactivate the pointer
        m_Pointer.TrulyDeactivate();
        // Perform the teleportation
        LocomoteFromFloor(pos);
        // Wait until we stop teleporting
        while (m_isTeleporting) {
            yield return null;
        }
        // Reset movement upon movement completion
        ResetPlayerMovement();
    }
    */

    /*
    /// The main locomotion function used by this component. The function will auto-adjust the destination to account for the player's height, which is set inside of Oculus' <code>CharacterController</code>.
    public void LocomoteFromFloor(Vector3 floorDestination) {
        if (!m_isTeleporting) {
            m_isTeleporting = true;
            Vector3 destination = new Vector3(floorDestination.x, floorDestination.y + m_charHeight * 0.5f, floorDestination.z);
            DoTeleport(destination);
        }
    }
    */
    public Vector3 LocomoteDestinationFromFloor(Vector3 floorDestination) {
        return new Vector3(floorDestination.x, floorDestination.y + m_charHeight * 0.505f, floorDestination.z);
    }
    /*
    /// If, in the advent that we want to teleport the player through script rather than player action, we can do so here.
    public void LocomoteDirectly(Vector3 destination) {
        if (!m_isTeleporting) {
            m_isTeleporting = true;
            DoTeleport(destination);
        }
    }
    */

    /*
    /// The big kahuna, this is used in the `Teleport()` flow to actually initialize movement based on locomotion settings from `m_locomotionType`
    public void DoTeleport(Vector3 destination) {
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        switch(m_locomotionType) {
            case(LocomotionType.Instant):
                StartCoroutine(InstantTeleport(destination));
                break;
            case(LocomotionType.Fade):
                StartCoroutine(FadeTeleport(destination));
                break;
            case(LocomotionType.SmoothDamp):
                StartCoroutine(SmoothDampTeleport(destination));
                break;;
        }
    }
    */

    /*
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
    */

    public void StartTranslate(string inputKey = null) {
        m_isPreparing = true;
        m_isTeleporting = true;
        m_isTranslating = true;
        m_translationInputName = inputKey;
    }
    public void EndTranslate() {
        m_isPreparing = false;
        m_isTeleporting = false;
        m_isTranslating = false;
        m_translationInputName = null;
    }
    public void TranslateTeleport(Vector3 forward, float speed, float deltaTime) {
        m_charTransform.position = m_charTransform.position + forward * speed * deltaTime;
    }


    public void Instant() {
        HideCursor();
        m_isPreparing = false;
        if (m_isTeleporting || !isPossible || m_destinationTarget == null) return;
        // We use a coroutine to allow locomotion to run over multiple frames, thereby preventing it from being a blocker in Unity's Update loops
        StartCoroutine(ActivateInstant(m_destinationPoint));
        return;
    }
    private IEnumerator ActivateInstant(Vector3 pos) {
        m_isTeleporting = true;
        // Deactivate thumbstick movement and rotation
        ChangePlayerMovement(false, false);
        // Deactivate the pointer
        m_Pointer.TrulyDeactivate();
        // Perform the teleportation
        Vector3 destination = LocomoteDestinationFromFloor(pos);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        StartCoroutine(InstantTeleport(destination));
        // Wait until we stop teleporting
        while (m_isTeleporting) {
            yield return null;
        }
        // Reset movement and rigidbodies upon movement completion
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
        m_Pointer.TrulyActivate();
        m_destinationTarget = null;
        ResetPlayerMovement();
    }
    /// Instant teleportation - set the player's avatar at the destination
    public IEnumerator InstantTeleport(Vector3 destination) {
        m_characterController.enabled = false;
        m_charTransform.position = destination;
        m_characterController.enabled = true;
        m_isTeleporting = false;
        yield return null;
    }



    public void Fade(float timeToFade = 0.25f) {
        HideCursor();
        m_isPreparing = false;
        if (m_isTeleporting || !isPossible || m_destinationTarget == null) return;
        // We use a coroutine to allow locomotion to run over multiple frames, thereby preventing it from being a blocker in Unity's Update loops
        StartCoroutine(ActivateFade(m_destinationPoint, timeToFade));
        return;
    }
    private IEnumerator ActivateFade(Vector3 pos, float timeToFade) {
        m_isTeleporting = true;
        // Deactivate thumbstick movement and rotation
        ChangePlayerMovement(false, false);
        // Deactivate the pointer
        m_Pointer.TrulyDeactivate();
        // Perform the teleportation
        Vector3 destination = LocomoteDestinationFromFloor(pos);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        if (timeToFade > 0f) StartCoroutine(FadeTeleport(destination, timeToFade));
        else StartCoroutine(InstantTeleport(destination));
        // Wait until we stop teleporting
        while (m_isTeleporting) {
            yield return null;
        }
        // Reset movement and rigidbodies upon movement completion
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
        m_Pointer.TrulyActivate();
        m_destinationTarget = null;
        ResetPlayerMovement();
    }
    /// Similar to `InstantTeleport()`, except with a fade-in and fade-out effect on the user's visuals.
    public IEnumerator FadeTeleport(Vector3 destination, float timeToFade) {
		float elapsedTime = 0;
        bool teleported = false;
		while (elapsedTime < timeToFade)
		{
            yield return null;
			elapsedTime += Time.deltaTime;
			if (!teleported && elapsedTime >= timeToFade*0.5f)
			{
				teleported = true;
                m_characterController.enabled = false;
				m_charTransform.position = destination;
                m_characterController.enabled = true;
			}
			float fadeLevel = FadeLevels.Evaluate(elapsedTime / timeToFade);
			m_screenFader.SetFadeLevel(fadeLevel);
		}

		m_screenFader.SetFadeLevel(0);
        m_isTeleporting = false;
        yield return null;
    }

    public void SmoothDamp(float timeToDamp = 0.25f) {
        HideCursor();
        m_isPreparing = false;
        if (m_isTeleporting || !isPossible || m_destinationTarget == null) return;
        // We use a coroutine to allow locomotion to run over multiple frames, thereby preventing it from being a blocker in Unity's Update loops
        StartCoroutine(ActivateSmoothDamp(m_destinationPoint, timeToDamp));
        return;
    }
    private IEnumerator ActivateSmoothDamp(Vector3 pos, float timeToDamp) {
        m_isTeleporting = true;
        // Deactivate thumbstick movement and rotation
        ChangePlayerMovement(false, false);
        // Deactivate the pointer
        m_Pointer.TrulyDeactivate();
        // Perform the teleportation
        Vector3 destination = LocomoteDestinationFromFloor(pos);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        if (timeToDamp > 0f) StartCoroutine(SmoothDampTeleport(destination, timeToDamp));
        else StartCoroutine(InstantTeleport(destination));
        // Wait until we stop teleporting
        while (m_isTeleporting) {
            yield return null;
        }
        // Reset movement and rigidbodies upon movement completion
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
        m_Pointer.TrulyActivate();
        m_destinationTarget = null;
        ResetPlayerMovement();
    }
    /// Smoothly move the player's avatar from their current position to the destination
    public IEnumerator SmoothDampTeleport(Vector3 destination, float timeToDamp) {
        Vector3 velocity = Vector3.zero;
        m_characterController.enabled = false;
        while(Vector3.Distance(m_charTransform.position, destination) > 0.01f) {
            yield return null;
            m_charTransform.position = Vector3.SmoothDamp(m_charTransform.position, destination, ref velocity, timeToDamp);
        }
        m_characterController.enabled = true;
        m_isTeleporting = false;
        yield return null;
    }




    #endregion
    #region Setting Modifiers

    public void CheckIfValid(Vector3 to) {
        // We need to indicate if the teleportation is possible, depending on m_checkHeight and m_checkGaps
        isPossible = (m_checkHeight == CheckHeight.AtDestination) ? isPossible = CheckHeightAtPoint(to) : true;
    }

    /// This hides the cursor for locomotion.
    public void ShowCursor(Vector3 to) {
        if (m_locomotionCursor == null) return;
        if (!m_locomotionCursor.activeSelf) m_locomotionCursor.SetActive(true);
        m_locomotionCursor.transform.position = to;

        if (m_showIfInvalid && m_cursorComponent) {
            if (isPossible) {
                foreach(Renderer r in m_cursorComponent.renderers) {
                    if (r.material.HasProperty("_SpecColor")) r.material.SetColor("_SpecColor", m_validColor);
                    if (r.material.HasProperty("_TintColor")) r.material.SetColor("_TintColor", m_validColor);
                    if (r.material.HasProperty("_Color")) r.material.SetColor("_Color", m_validColor);
                }
            } else {
                foreach(Renderer r in m_cursorComponent.renderers) {
                    if (r.material.HasProperty("_SpecColor")) r.material.SetColor("_SpecColor", m_invalidColor);
                    if (r.material.HasProperty("_TintColor")) r.material.SetColor("_TintColor", m_invalidColor);
                    if (r.material.HasProperty("_Color")) r.material.SetColor("_Color", m_invalidColor);
                }
            }
        }
    }
    /// This shows the cursor for locomotion.
    public void HideCursor() {
        if (m_locomotionCursor == null) return;
        if (m_locomotionCursor.activeSelf) m_locomotionCursor.SetActive(false);
    }

    // At a particular point, can our character fit in there? Returns TRUE if yes, otherwise false
    private bool CheckHeightAtPoint(Vector3 pos) {
        RaycastHit hit;
        bool possible = true;
        if (Physics.Raycast(pos, Vector3.up, out hit, (m_charHeight*heightTolerancePercent))) {
            // Our raycast hit something within the scope of the minimum height tolerance... that ain't good
            possible = false;
        }
        return possible;
    }

    /*
    /// This function allows the switching between different locomotion types, if needed.
    public void ChangeLocomotionType(string type) {
        switch(type) {
            case("Instant"):
                m_locomotionType = LocomotionType.Instant;
                break;
            case("Fade"):
                m_locomotionType = LocomotionType.Fade;
                break;
            case("SmoothDamp"):
                m_locomotionType = LocomotionType.SmoothDamp;
                break;
        }
        return;
    }
    */
    /// This function alters the movement and rotation of the player using the joystick.
    /// This is necessary because we don't want the player to be able to move or rotate using the joystick in the middle of teleporting.
    public void ChangePlayerMovement(bool mov, bool rot) {
        m_playerController.EnableLinearMovement = mov;
        m_playerController.EnableRotation = rot;
    }
    /// A function that defaults the player movement to allow them to move and point again. A simplified version of `ChangePlayerMovement()`
    public void ResetPlayerMovement() {
        m_playerController.EnableLinearMovement = m_linMov;
        m_playerController.EnableRotation = m_rotMov;
    }

    #endregion
}