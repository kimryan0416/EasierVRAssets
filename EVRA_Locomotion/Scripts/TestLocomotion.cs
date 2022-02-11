using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocomotionType {
    None,
    Instant,
    Fade,
    SmoothDamp,
    TranslateTarget,
    Translate,
    TranslateLikeJoystick,
}

public class TestLocomotion : MonoBehaviour
{
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

    [SerializeField] [Tooltip("Reference to the hand we want the rotation to base off of")]
    private EVRA_Hand m_Hand;

    [SerializeField] [Tooltip("Reference to the pointer we'll be using as our destination pointer")]
    private EVRA_Pointer m_Pointer;

    /// This controls the fade-to-black of the "Fade" locomotion type. We set keyframes such that the fade level is 0, then 1, then 0 within the time frame determined by m_transitionTime.
    [SerializeField] [Tooltip("Fade to black over the duration of the transition")]
	private AnimationCurve FadeLevels = new AnimationCurve(new Keyframe[3] { new Keyframe(0,0), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.0f) });

     /// We store a reference to the prefab that is the cursor for our locomotion function. When we start pointing at where we want to teleport to, this prefab is instantiated and rendered.
    [SerializeField] [Tooltip("The locomotion cursor to indicate where the locomotion destination will be")]
    private GameObject m_locomotionCursor = null;
    [Tooltip("Ref to the locomotion cursor's `LocomotionCursor` component, if it has one")] // NOT SERIALIZED
    private EVRA_LocCursor m_cursorComponent = null;

    /// We are storing a reference to `m_characterController`'s transform component. Overkill, but easier to handle.
    [Tooltip("Reference to character transform")]
    private Transform m_charTransform;

    [Tooltip("Saved memory of the character height, based on `height` from OVRCharcterController")] // NOT SERIALIZED
    private float m_charHeight;

    /// When we do the locomote, we are essentially overwriting some values in the OVRPlayerController.
    /// To ensure that we're not messing up anything, we store the OVRPlayerController's initially set linear movement and rotational movement booleans here, then re-apply them when we finish the locomote.
    [Tooltip("Saved memory of the initial conditions for movement with the OVRPlayerController")] // NOT SERIALIZED
    private bool m_linMov, m_rotMov;

    // ------
    // A problem with Oculus' original implementation of VR teleportation is that "instant" teleportation is actually a 3D transition, supersped to appear as if it is instant.
    // This is a problem when objects with rigidbodies are in the pathway between the player's current position and the destination.
    // Because the player's body transitions rather than simply sets the player's position, the avatar's collider can hit objects with colliders and rigidbodies.
    // To remedy this, we have a list of RigidBodies attached to our player avatar. When we teleport, we disable physics on all these RigidBodies by setting their kinematic settings to true.
    // With this new system, regardless of whether we're instantly teleporting or transitioning, we ensure that we are not accidentally hitting any objects that happen to be in the way.
    // ------
    [SerializeField] [Tooltip("All rigidbodies that must be set to kinematic")]
    private List<Rigidbody> m_rbs = new List<Rigidbody>();

    /// When we start locomotion, we store the original Kinematic statuses of each of the RigidBodies inside of `m_rbs`.
    /// When we finish locomoting, we set the kinematic values back to what they originally were.
    [Tooltip("private reference for all initial rigidbody statuses")] // NOT SERIALIZED
    private Dictionary<int, bool> m_originalKinStat = new Dictionary<int, bool>();

    /// When using a pointer, we need to know two things: 1) the pointer's target object, and 2) the pointer's downward target position
    /// We need both because, we use this to ensure that we have a legit target destination. If you are pointing into empty space, for example, you wouldn't want to locomote into a floorless space.
    /// So we use destinationTarget to make sure our pointer is actually hitting a legit locomotion area, then we can use destinationPosition safely as a final target.
    private Transform destinationTarget = null;
    private Vector3 destinationPosition = Vector3.zero;
    private Quaternion destinationRotation = Quaternion.identity;
    private float lastJoystickAngle = 0f;
    /// finalDestination is the actual destination that the player will locomote to
    public Vector3 finalDestination = Vector3.zero;
    public Quaternion finalRotation = Quaternion.identity;
    public Quaternion deriv = Quaternion.identity;
    public float totalDistanceToDestination = 0f;

    /// Internal boolean for Fade, SmoothDamp, and TranslateTarget to determine if the transition is performed yet
    private bool teleported = true;
    /// Internal boolean for SmoothDamp, TranslateTarget, and Translate as a reference velocity when damping around
    private Vector3 velocity = Vector3.zero;
    /// internal time measurement for how long the locomotion has been going for.
    private float elapsedTime = 0f;
    [Tooltip("The total time the locomotion should take for Fade, SmoothDamp, and TranslateTarget.")]
    public float transitionTime = 0.5f;

    [Tooltip("The max speed of the player, if locomoting via translation, in MPS")]
    public float maxSpeed = 2f;

    [Tooltip("The type of locomotion used in the current moment. When performing a locomotion, this will change.")]
    public LocomotionType locomotionType = LocomotionType.None;

    [Tooltip("Should any transition-based locomotion (i.e. SmoothDamp, Translate, TranslateTarget move directly to the target, or should it move across the floor?")]
    public bool transitionAcrossFloor = false;

    /// Internal variables used to track if the user is 1) currently selecting a destination, or 2) is currently teleporting around.
    private bool isSelecting = false;
    private bool isTeleporting = false;

    [Tooltip("When using Instant, Fade, SmoothDamp, or TranslateTarget, should the player use the joystick to also select a rotation destination?")]
    public bool rotatePlayerWhenLocomoting = false;

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
        finalDestination = transform.position;
    }

    private void Update() {
        if (isSelecting) { 
            DisableLinearMovement();
            DisableCharacterController();
            destinationTarget = m_Pointer.downwardRaycastTarget;
            destinationPosition = m_Pointer.downwardRaycastHitPosition;
            lastJoystickAngle = (m_Hand.ThumbstickAngle.y > 0.1f) ? m_Hand.ThumbstickAngle.x : lastJoystickAngle;
            destinationRotation = Quaternion.Euler(0f, m_Pointer.transform.rotation.eulerAngles.y, 0f) * Quaternion.Euler(0f, lastJoystickAngle, 0f);
            ShowCursor(destinationPosition, destinationRotation);
        } else {
            HideCursor();
        }

        if (!isTeleporting) {
            finalDestination = m_charTransform.position;
            finalRotation = m_charTransform.rotation;
        } else {
            switch(locomotionType) {
                case (LocomotionType.Instant):
                    InstantCalculation();
                    break;
                case (LocomotionType.Fade):
                    FadeCalculation();
                    break;
                case (LocomotionType.SmoothDamp):
                    SmoothDampCalculation();
                    break;
                case (LocomotionType.TranslateTarget):
                    TranslateTargetCalculation();
                    break;
                case (LocomotionType.Translate):
                    finalDestination = LocomoteDestinationFromFloor(destinationPosition);
                    TranslateCalculation();
                    break;
                case (LocomotionType.TranslateLikeJoystick):
                    finalDestination = LocomoteDestinationFromFloor(new Vector3(m_Pointer.transform.position.x, RaycastDownward(m_Pointer.transform.position).y, m_Pointer.transform.position.z));
                    TranslateCalculation();
                    break;
                default:
                    break;
            }
        }
    }

    public void SetPointer(EVRA_Pointer newPointer) {
        m_Pointer = newPointer;
    }

    public void SelectDestination() {
        isSelecting = true;
    }
    public void StopSelectDestination() {
        isSelecting = false;
    }

    public void Instant(bool stopDestSelect = true) {
        // If we're currently selecting, we'll grab the target position from destinationPosition
        if (isSelecting && destinationTarget == null) return;
        finalDestination = LocomoteDestinationFromFloor(destinationPosition);
        finalRotation = destinationRotation;
        totalDistanceToDestination = Vector3.Distance(m_charTransform.position, finalDestination);
        locomotionType = LocomotionType.Instant;

        lastJoystickAngle = 0f;
        ChangePlayerMovement(false, false);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }

        teleported = false;
        isTeleporting = true;
        if (stopDestSelect) StopSelectDestination();
    }
    public void ManualInstant(Vector3 dest, Quaternion rot) {
        finalDestination = LocomoteDestinationFromFloor(dest);
        finalRotation = rot;
        totalDistanceToDestination = Vector3.Distance(m_charTransform.position, finalDestination);

        lastJoystickAngle = 0f;
        locomotionType = LocomotionType.Instant;
        ChangePlayerMovement(false, false);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        teleported = false;
        isTeleporting = true;
    }
    private void InstantCalculation() {
        m_characterController.enabled = false;
        m_charTransform.position = finalDestination;
        if (rotatePlayerWhenLocomoting) m_charTransform.rotation = finalRotation;
        m_characterController.enabled = true;

        StopLocomotion();
    }

    public void Fade(bool stopDestSelect = true) {
        if (isSelecting && destinationTarget == null) return;
        finalDestination = LocomoteDestinationFromFloor(destinationPosition);
        finalRotation = destinationRotation;
        totalDistanceToDestination = Vector3.Distance(m_charTransform.position, finalDestination);
        elapsedTime = 0f;
        locomotionType = LocomotionType.Fade;

        lastJoystickAngle = 0f;
        ChangePlayerMovement(false, false);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }

        teleported = false;
        isTeleporting = true;
        if (stopDestSelect) StopSelectDestination();
    }
    public void ManualFade(Vector3 dest, Quaternion rot) {
        finalDestination = LocomoteDestinationFromFloor(dest);
        finalRotation = rot;
        totalDistanceToDestination = Vector3.Distance(m_charTransform.position, finalDestination);
        lastJoystickAngle = 0f;
        elapsedTime = 0f;
        locomotionType = LocomotionType.Fade;
        ChangePlayerMovement(false, false);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        teleported = false;
        isTeleporting = true;
    }
    private void FadeCalculation() {
        lastJoystickAngle = 0f;
        elapsedTime += Time.deltaTime;
        if (elapsedTime < transitionTime) {
            if (!teleported && elapsedTime >= transitionTime*0.5f) {
                teleported = true;
                m_characterController.enabled = false;
                m_charTransform.position = finalDestination;
                if (rotatePlayerWhenLocomoting) m_charTransform.rotation = finalRotation;
                m_characterController.enabled = true;
            }
            float fadeLevel = FadeLevels.Evaluate(elapsedTime / transitionTime);
            m_screenFader.SetFadeLevel(fadeLevel);
        } else {
            StopLocomotion();
        }
    }

    public void SmoothDamp(bool stopDestSelect = true) {
        if (isSelecting && destinationTarget == null) return;
        finalDestination = LocomoteDestinationFromFloor(destinationPosition);
        finalRotation = destinationRotation;
        totalDistanceToDestination = Vector3.Distance(m_charTransform.position, finalDestination);
        locomotionType = LocomotionType.SmoothDamp;
        lastJoystickAngle = 0f;
        ChangePlayerMovement(false, false);
        m_characterController.enabled = false;
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }

        teleported = false;
        isTeleporting = true;
        if (stopDestSelect) StopSelectDestination();
    }
    public void ManualSmoothDamp(Vector3 dest, Quaternion rot) {
        finalDestination = LocomoteDestinationFromFloor(dest);
        finalRotation = rot;
        totalDistanceToDestination = Vector3.Distance(m_charTransform.position, finalDestination);
        locomotionType = LocomotionType.SmoothDamp;
        lastJoystickAngle = 0f;
        ChangePlayerMovement(false, false);
        m_characterController.enabled = false;
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        teleported = false;
        isTeleporting = true;
    }
    private void SmoothDampCalculation() {
        float distanceTo = Vector3.Distance(m_charTransform.position, finalDestination);
        bool distanceReached = distanceTo <= 0.01f;
        if (!distanceReached) {
            Vector3 goTo = Vector3.SmoothDamp(m_charTransform.position, finalDestination, ref velocity, transitionTime);
            if (transitionAcrossFloor) goTo = LocomoteDestinationFromFloor(RaycastDownward(new Vector3(goTo.x, goTo.y + m_charHeight * 0.505f, goTo.z)));
            m_charTransform.position = goTo;
        } 
        if (rotatePlayerWhenLocomoting) m_charTransform.rotation = QuaternionSmoothDamp(m_charTransform.rotation, finalRotation, ref deriv, transitionTime);
        if (distanceReached) {
            StopLocomotion();
        }
    }

    public void TranslateTarget(bool stopDestSelect = true) {
        if (isSelecting && destinationTarget == null) return;
        finalDestination = LocomoteDestinationFromFloor(destinationPosition);
        finalRotation = destinationRotation;
        locomotionType = LocomotionType.TranslateTarget;
        lastJoystickAngle = 0f;
        ChangePlayerMovement(false, false);
        m_characterController.enabled = false;
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }

        teleported = false;
        isTeleporting = true;
        if (stopDestSelect) StopSelectDestination();
    }
    public void ManualTranslateTarget(Vector3 dest, Quaternion rot) {
        finalDestination = LocomoteDestinationFromFloor(dest);
        finalRotation = rot;
        locomotionType = LocomotionType.TranslateTarget;
        lastJoystickAngle = 0f;
        ChangePlayerMovement(false, false);
        m_characterController.enabled = false;
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }
        teleported = false;
        isTeleporting = true;
    }
    private void TranslateTargetCalculation() {
        if (Vector3.Distance(m_charTransform.position, finalDestination) > 0.01f) {
            Vector3 goTo = Vector3.SmoothDamp(m_charTransform.position, finalDestination, ref velocity, transitionTime, maxSpeed);
            if (transitionAcrossFloor) goTo = LocomoteDestinationFromFloor(RaycastDownward(new Vector3(goTo.x, goTo.y + m_charHeight * 0.505f, goTo.z)));
            m_charTransform.position = goTo;
            if (rotatePlayerWhenLocomoting) m_charTransform.rotation = QuaternionSmoothDamp(m_charTransform.rotation, finalRotation, ref deriv, transitionTime);
        } else {
            StopLocomotion();
        }
    }

    public void Translate() {
        locomotionType = LocomotionType.Translate;
        lastJoystickAngle = 0f;
        ChangePlayerMovement(false, true);
        m_characterController.enabled = false;
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }

        teleported = false;
        isTeleporting = true;
    }
    public void TranslateLikeJoystick() {
        locomotionType = LocomotionType.TranslateLikeJoystick;
        lastJoystickAngle = 0f;
        ChangePlayerMovement(false, true);
        m_characterController.enabled = false;
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = false;
        }

        teleported = false;
        isTeleporting = true;
    }
    private void TranslateCalculation() {
        elapsedTime = Time.deltaTime;

        if (Vector3.Distance(m_charTransform.position, finalDestination) > 0.01f) {
            //m_charTransform.position = m_charTransform.position + direction * maxSpeed * elapsedTime;
            Vector3 goTo = Vector3.SmoothDamp(m_charTransform.position, finalDestination, ref velocity, transitionTime, maxSpeed);
            if (transitionAcrossFloor) goTo = LocomoteDestinationFromFloor(RaycastDownward(new Vector3(goTo.x, goTo.y + m_charHeight * 0.505f, goTo.z)));
            m_charTransform.position = goTo;
        } else {
            StopLocomotion();
        }
    }

    public void StopLocomotion() {
        m_screenFader.SetFadeLevel(0);
        velocity = Vector3.zero;
        teleported = true;
        isTeleporting = false;
        m_characterController.enabled = true;
        ResetPlayerMovement();
        locomotionType = LocomotionType.None;
    }
    public void StopLocomotion(EVRA_Pointer signalPointer) {
        if (m_Pointer == signalPointer) StopLocomotion();
    }

    public void ShowCursor() {
        if (m_locomotionCursor) m_locomotionCursor.SetActive(true);
    } 
    public void ShowCursor(Vector3 targetPos, Quaternion targetRot) {
        if (!m_locomotionCursor) return;
        m_locomotionCursor.SetActive(true); 
        m_locomotionCursor.transform.position = targetPos;
        m_locomotionCursor.transform.rotation = targetRot;
    }
    public void HideCursor() {
        if (m_locomotionCursor) m_locomotionCursor.SetActive(false);
    }
    // At a particular point, can our character fit in there? Returns TRUE if yes, otherwise false
    private bool CheckHeightAtPoint(Vector3 pos) {
        RaycastHit hit;
        bool possible = true;
        if (Physics.Raycast(pos, Vector3.up, out hit, m_charHeight)) {
            // Our raycast hit something within the scope of the minimum height tolerance... that ain't good
            possible = false;
        }
        return possible;
    }
    private Vector3 RaycastDownward(Vector3 initial) {

        int layerMaskDownward = 1 << LayerMask.NameToLayer("EVRA_LocDestIgnore");
        layerMaskDownward = ~layerMaskDownward;

        RaycastHit hit;
        Vector3 target = initial;
        bool raycastResult = Physics.Raycast(initial, -Vector3.up, out hit, Mathf.Infinity, layerMaskDownward);
        if (raycastResult) target = hit.point;
        return target;
    }


    public Vector3 LocomoteDestinationFromFloor(Vector3 floorDestination) {
        return new Vector3(floorDestination.x, floorDestination.y + m_charHeight * 0.505f, floorDestination.z);
    }
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
    public void DisableLinearMovement() {
        ChangePlayerMovement(false,true);
    }
    public void DisableCharacterController() {
        m_characterController.enabled = false;
    }

    // https://gist.github.com/maxattack/4c7b4de00f5c1b95a33b
    public static Quaternion QuaternionSmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time) {
		if (Time.deltaTime < Mathf.Epsilon) return rot;
		// account for double-cover
		var Dot = Quaternion.Dot(rot, target);
		var Multi = Dot > 0f ? 1f : -1f;
		target.x *= Multi;
		target.y *= Multi;
		target.z *= Multi;
		target.w *= Multi;
		// smooth damp (nlerp approx)
		var Result = new Vector4(
			Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
			Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
			Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
			Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
		).normalized;
		
		// ensure deriv is tangent
		var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
		deriv.x -= derivError.x;
		deriv.y -= derivError.y;
		deriv.z -= derivError.z;
		deriv.w -= derivError.w;		
		
		return new Quaternion(Result.x, Result.y, Result.z, Result.w);
	}
}
