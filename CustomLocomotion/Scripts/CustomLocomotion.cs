using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLocomotion : MonoBehaviour
{   
    [SerializeField]
    [Tooltip("Reference to OVRPlayer")]
    private CharacterController m_characterController;
    [SerializeField]
    [Tooltip("Reference to OVRPlayerController")]
    private OVRPlayerController m_playerController;
    [SerializeField]
    [Tooltip("Reference to Fade script")]
    private OVRScreenFade m_screenFader;
    public OVRScreenFade screenFader {
        get {   return m_screenFader;   }
    }

    [SerializeField]
    [Tooltip("Controller for locomotion")]
    private CustomGrabber m_customGrabber;
    public CustomGrabber customGrabber {
        get {   return m_customGrabber;     }
        set {   m_customGrabber = value;    }
    }
    // NOT SERIALIZED
    [Tooltip("Reference to destination of locomotion")]
    private Vector3 m_destination = Vector3.zero;
    public Vector3 destination {
        get {   return m_destination;   }
        set {   m_destination = value;  }
    }
    // NOT SERIALIZED
    [Tooltip("Reference to whether the raycast downward from pointer is detecting something")]
    private bool m_floorBelow = false;

    [SerializeField]
    [Tooltip("Reference to prefab for locomotion cursor")]
    private ExternalCollider m_locomotionCursorPrefab;
    // NOT SERIALIZED
    [Tooltip("Instantiated locomotion cursor")]
    private ExternalCollider m_instantiatedCursor = null;
    // NOT SERIALIZED
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





    [SerializeField]
    [Tooltip("Locomotion Trigger Button - default = index trigger")]
    private OVRInput.Button m_locomotionTrigger = OVRInput.Button.PrimaryIndexTrigger;

    [SerializeField]
    [Tooltip("Indicator color for okay locomotion")]
    private Material m_validLocomotionColor;

    [SerializeField]
    [Tooltip("Indicator colro for invalid locomotion")]
    private Material m_invalidLocomotionColor;

    [SerializeField]
    [Tooltip("All rigidbodies htat must be set to kinematic")]
    private List<Rigidbody> m_rbs = new List<Rigidbody>();
    // NOT SERIALIZED
    [Tooltip("private reference for all initial rigidbody statuses")]
    private Dictionary<int, bool> m_originalKinStat = new Dictionary<int, bool>();

    // NOT SERIALIZED
    [Tooltip("Stores the thumbstick's position")]
    private Vector2 m_thumbstickPosition;

    // NOT SERIALIZED
    private enum pointerType {
        Target,
        Teleport
    }

    /*
    private enum activationType {
        Hold_Release,
        Double_Press,
        Two_Buttons
    }
    [SerializeField]
    [Tooltip("The method to activate and initialize locomotion")]
    private activationType m_activationType;
    */

    private enum locomotionType {
        Instant,
        Fade,
        SmoothDamp,
    }
    [SerializeField]
    [Tooltip("Determine the type of locomotion")]
    private locomotionType m_locomotionType;

    [SerializeField]
    [Tooltip("Total distance the pointer is allowed to travel")]
    private float m_maxDistance = 5f;
    
    [SerializeField]
    [Tooltip("Time to transition for non-Instant locomotion types")]
    private float m_transitionTime;

    [SerializeField]
    [Tooltip("Fade to black over the duration of the transition")]
	private AnimationCurve FadeLevels = new AnimationCurve(new Keyframe[3] { new Keyframe(0,0), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.0f) });

    // NOT SERIALIZED
    [Tooltip("Reference to character transform")]
    private Transform m_charTransform;

    // NOT SERIALIZED
    [Tooltip("Saved memory of the initial conditions for movement with the OVRPlayerController")]
    private bool m_linMov, m_rotMov;

    [SerializeField]
    [Tooltip("Bool to determine if the locomotion should initialize ")]
    private bool m_shouldStartOnRun = true;

    private void Start() {
        if (m_shouldStartOnRun) Init();
    }

    public void Init() {
        m_charTransform = m_characterController.transform;
        m_linMov = m_playerController.EnableLinearMovement;
        m_rotMov = m_playerController.EnableRotation;
        m_customGrabber.pointer.SetPointerType("Teleport");
        m_customGrabber.pointer.SetLaserType("Parabolic");
        m_customGrabber.pointer.raycastDistance = m_maxDistance;
        //if (m_locomotionIndicator != null) {
            //m_locomotionIndicator.ActivateColliders();
            //m_locomotionIndicator.transform.localScale = new Vector3(m_locomotionIndicator.transform.localScale.x, m_characterController.height / 2f, m_locomotionIndicator.transform.localScale.z);
        //}
        foreach(Rigidbody rb in m_rbs) {
            m_originalKinStat.Add(rb.gameObject.GetInstanceID(), rb.isKinematic);
        }
        
        if (m_customGrabber != null) {
            switch(m_customGrabber.OVRController) {
                case(OVRInput.Controller.LTouch):
                    CustomEvents.current.onLeftTriggerDown += PrepareTeleport;
                    CustomEvents.current.onLeftTriggerUp += Teleport;
                    break;
                case(OVRInput.Controller.RTouch):
                    CustomEvents.current.onRightTriggerDown += PrepareTeleport;
                    CustomEvents.current.onRightTriggerUp += Teleport;
                    break;
            }
        }
    }

    // Basic flow:
    // When the user pressed DOWN on the trigger, locomotion is PREPARING
    // During this prep period, a cursor is meant to appear
    // There can only be one cursor in the world. That is no question
    // When the user lets GO of the trigger, locomotion is TELEPORTING
    // In this situation, the press-down can still be registered - that is fine. However, we cannot ignore the possibility that the preparing state can be re-instated multiple times
    // So, here's how we're gonna do it:
    //  1) IS PREPARING is first initiated
    //      - Check if the grabbers correspond with each other, as a precaution
    //      - Check if isPreparing is true - if it is, then we return early
    //      - Assuming that we haven't started prepping yet, we instantiate a new cursor that's automatically hidden from the world view
    //  2) During the Update() sequence:
    //      - To save time in the system, we want to return early if the player is not prepping for locomotion
    //      - We update the destination and whether we're hitting a floor or not
    //      - If we're hitting a floor, we alter the location of the 

    public void PrepareTeleport(OVRInput.Controller c) {
        if (c != m_customGrabber.OVRController || m_isPreparing) return;
        m_instantiatedCursor = InstantiateCursor();
        m_isPreparing = true;
        return;
    }
    public void Teleport(OVRInput.Controller c) {
        if (c != m_customGrabber.OVRController || !m_isPreparing || m_isTeleporting) return;
        if (m_instantiatedCursor != null) {
            Destroy(m_instantiatedCursor.gameObject);
            m_instantiatedCursor = null;
        }
        m_isPreparing = false;
        if (m_floorBelow)   StartCoroutine(ActivateLocomotion(m_destination));
        return;
    }

    private IEnumerator QueueNewCursor() {
        while(m_isTeleporting) {
            yield return null;
        }

    }
    private ExternalCollider InstantiateCursor() {
        if (m_instantiatedCursor != null) return m_instantiatedCursor;
        ExternalCollider newCursor =  Instantiate(m_locomotionCursorPrefab, m_customGrabber.pointer.locomotionPosition, Quaternion.identity) as ExternalCollider;
        HideCursor(newCursor);
        return newCursor;
    }
    private void HideCursor(ExternalCollider ec = null) {
        ec = (ec == null) ? m_instantiatedCursor : ec;
        if (ec == null) return;
        foreach(Renderer r in ec.GetComponentsInChildren<Renderer>()) {
            r.enabled = false;
        }
        return;
    }
    private void ShowCursor(ExternalCollider ec = null) {
        ec = (ec == null) ? m_instantiatedCursor : ec;
        if (ec == null) return;
        foreach(Renderer r in ec.GetComponentsInChildren<Renderer>()) {
            r.enabled = true;
        }
    }
    private void DestroyCursor() {
        Destroy(m_instantiatedCursor.gameObject);
        m_instantiatedCursor = null;
    }

    private void Update() {

        // Update doesn't run if we're currently not finding a location to teleport to
        if (!m_isPreparing) {
            return;
        }

        // If we're currently teleporting, we hide our cursor
        if (m_isTeleporting) {
            HideCursor();
            return;
        }
        ShowCursor();
        
        // Get the current pointer's destination
        m_destination = m_customGrabber.pointer.locomotionPosition;
        m_floorBelow = m_customGrabber.pointer.raycastDownwardHit;

        // Move the instantiated locomotion teleporter to the destination only if a downward raycast is detected
        if (m_floorBelow) {
            ShowCursor();
            m_instantiatedCursor.transform.position = m_destination;
        } else {
            HideCursor();
        }

        /*
        if (!m_customGrabber.pointer.LineIsEnabled()) {
            m_locomotionIndicator.gameObject.SetActive(false);
            return;
        } else {
            m_locomotionIndicator.gameObject.SetActive(true);
            m_locomotionIndicator.transform.position = new Vector3(
                m_customGrabber.pointer.locomotionPosition.x, 
                // m_customGrabber.pointer.locomotionPosition.y + m_characterController.height,
                m_customGrabber.pointer.locomotionPosition.y,
                m_customGrabber.pointer.locomotionPosition.z
            );
            if (m_locomotionIndicator.colliding) {
                m_locomotionIndicator.GetComponent<Renderer>().material = m_invalidLocomotionColor;
                m_customGrabber.pointer.lineColor = m_invalidLocomotionColor.GetColor("_Color");
            } else {
                m_locomotionIndicator.GetComponent<Renderer>().material = m_validLocomotionColor;
                m_customGrabber.pointer.lineColor = m_validLocomotionColor.GetColor("_Color");
            }
        }

        // we need to detect when the player presses or releases the trigger
        if (OVRInput.GetUp(m_locomotionTrigger, m_customGrabber.OVRController)) {
            StartCoroutine(ActivateLocomotion());
        }

        /*
        switch(m_customGrabber.m_controller) {
            case(OVRInput.Controller.RTouch):
                m_thumbstickPosition = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
                break;
            case(OVRInput.Controller.LTouch):
                m_thumbstickPosition = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
                break;
            default:
                m_thumbstickPosition = new Vector2(0f,1f);
                break;
        }

        // Convert thumbstick position to angles, if the distance between the current thumbstick position and the center is greater than a threshold
        if ()
        */
        /*
        // Double-checking to make sure that our custom pointer is actually active
        if (OVRInput.GetDown(m_primaryTrigger,m_controller.m_controller) || OVRInput.Get(m_primaryTrigger,m_controller.m_controller)) {
            // primary trigger has been pressed down - preparing for locomotion OR locomoting, if we need to
            switch(m_activationType) {
                case(activationType.Double_Press) {

                }
            }
        }
        */
    }


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

    public void LocomoteDirectly(Vector3 destination) {
        if (!m_isTeleporting) {

            // Check if the locomotion is viable
            //if (m_locomotionIndicator != null && m_locomotionIndicator.colliding) {
            //    return;
            //}

            m_isTeleporting = true;
            DoTeleport(destination);
        }
    }

    public void LocomoteFromFloor(Vector3 floorDestination) {
        if (!m_isTeleporting) {

            // Check if the locomotion is viable
            //if (m_locomotionIndicator != null && m_locomotionIndicator.colliding) {
            //    return;
            //}

            m_isTeleporting = true;
            Vector3 destination = new Vector3(floorDestination.x, floorDestination.y + m_characterController.height*0.5f, floorDestination.z);
            DoTeleport(destination);
        }
    }

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

    // Largely adapted from LocomotionTeleport script from OVR package
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




        /*
        var character = LocomotionController.CharacterController;
		var characterTransform = character.transform;
		var destTransform = _teleportDestination.OrientationIndicator;

		Vector3 destPosition = destTransform.position;
		destPosition.y += character.height * 0.5f;
		Quaternion destRotation = _teleportDestination.LandingRotation;// destTransform.rotation;
#if false
		Quaternion destRotation = destTransform.rotation;

		//Debug.Log("Rots: " + destRotation + " " + destTransform.rotation * Quaternion.Euler(0, -LocomotionController.CameraRig.trackingSpace.localEulerAngles.y, 0));

		destRotation = destRotation * Quaternion.Euler(0, -LocomotionController.CameraRig.trackingSpace.localEulerAngles.y, 0);
#endif
		if (Teleported != null)
		{
			Teleported(characterTransform, destPosition, destRotation);
		}

		characterTransform.position = destPosition;
		characterTransform.rotation = destRotation;

		LocomotionController.PlayerController.Teleported = true;
        */
    }

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
        //m_locomotionIndicator.ResetStatus();

		m_screenFader.SetFadeLevel(0);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
        m_isTeleporting = false;
    }
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

    public void ChangePlayerMovement(bool mov, bool rot) {
        m_playerController.EnableLinearMovement = mov;
        m_playerController.EnableRotation = rot;
        if (!mov) {
            m_customGrabber.pointer.Deactivate();
        } else {
            m_customGrabber.pointer.Activate();
        }
    }
    public void ResetPlayerMovement() {
        m_playerController.EnableLinearMovement = m_linMov;
        m_playerController.EnableRotation = m_rotMov;
        m_customGrabber.pointer.Activate();
    }
}
