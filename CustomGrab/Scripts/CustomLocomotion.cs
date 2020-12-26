﻿using System.Collections;
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

    [SerializeField]
    [Tooltip("Locomotion Trigger Button - default = index trigger")]
    private OVRInput.Button m_locomotionTrigger = OVRInput.Button.PrimaryIndexTrigger;

    [SerializeField]
    [Tooltip("Indicator for locomotion")]
    private ExternalCollider m_locomotionIndicator;

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
        Fade
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
    [Tooltip("Boolean to detecting if we're getting ready to teleport")]
    private bool m_isPreparing = false;
    public bool isPreparing {
        get {   return m_isPreparing;   }
        set {   m_isPreparing = value;  }
    }

    // NOT SERIALIZED
    [Tooltip("Boolean to detect if we're currently teleporting")]
    private bool m_isTeleporting = false;
    public bool isTeleporting {
        get {   return m_isTeleporting;     }
        set {   m_isTeleporting = value;    }
    }

    // NOT SERIALIZED
    [Tooltip("Saved memory of the initial conditions for movement with the OVRPlayerController")]
    private bool m_linMov, m_rotMov;

    private void Start() {
        m_charTransform = m_characterController.transform;
        m_linMov = m_playerController.EnableLinearMovement;
        m_rotMov = m_playerController.EnableRotation;
        m_customGrabber.pointer.SetPointerType("Teleport");
        m_customGrabber.pointer.SetLaserType("Parabolic");
        m_customGrabber.pointer.raycastDistance = m_maxDistance;
        if (m_locomotionIndicator != null) {
            //m_locomotionIndicator.ActivateColliders();
            //m_locomotionIndicator.transform.localScale = new Vector3(m_locomotionIndicator.transform.localScale.x, m_characterController.height / 2f, m_locomotionIndicator.transform.localScale.z);
        }
        foreach(Rigidbody rb in m_rbs) {
            m_originalKinStat.Add(rb.gameObject.GetInstanceID(), rb.isKinematic);
        }
    }

    private void Update() {
        // We don't do anything if the customPointer assigned to our customGrabber is NOT a locomotion type
        if (!m_customGrabber.pointer.isTeleportType()) {
            if (m_locomotionIndicator != null) m_locomotionIndicator.gameObject.SetActive(false);
            return;
        }

        // We don't do anything if we're currently in the teleportation process!
        if (m_isTeleporting) return;

        // If the locomotion indicator is not null, we 1) update its position, and 2) change its color depending on its status
        if (m_locomotionIndicator == null) return;

        if (!m_customGrabber.pointer.LineIsEnabled()) {
            m_locomotionIndicator.gameObject.SetActive(false);
            return;
        } else {
            m_locomotionIndicator.gameObject.SetActive(true);
            m_locomotionIndicator.transform.position = new Vector3(m_customGrabber.pointer.locomotionPosition.x, m_customGrabber.pointer.locomotionPosition.y + m_characterController.height, m_customGrabber.pointer.locomotionPosition.z);
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
    /*
    public void ChangeActivationType(string type) {
        switch(type) {
            case("Hold_Release"):
                m_activationType = activationType.Hold_Release;
                break;
            case("Double_Press"):
                m_activationType = activationType.Double_Press;
                break;
            case("Two_Buttons"):
                m_activationType = activationType.Two_Buttons;
                break;
        }
        return;
    }
    public void ChangePrimaryTrigger(OVRInput.Button b) {
        m_primaryTrigger = b;
        return;
    }
    public void ChangeSecondaryTrigger(OVRInput.Button b) {
        m_secondaryTrigger = b;
        return;
    }
    */

    public void LocomoteDirectly(Vector3 destination) {
        if (!m_isTeleporting) {

            // Check if the locomotion is viable
            if (m_locomotionIndicator != null && m_locomotionIndicator.colliding) {
                return;
            }

            m_isTeleporting = true;
            DoTeleport(destination);
        }
    }

    public void LocomoteFromFloor(Vector3 floorDestination) {
        if (!m_isTeleporting) {

            // Check if the locomotion is viable
            if (m_locomotionIndicator != null && m_locomotionIndicator.colliding) {
                return;
            }

            m_isTeleporting = true;
            Vector3 destination = new Vector3(floorDestination.x, floorDestination.y + m_characterController.height, floorDestination.z);
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
        m_isTeleporting = false;
        m_locomotionIndicator.ResetStatus();
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
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
        m_locomotionIndicator.ResetStatus();

		m_screenFader.SetFadeLevel(0);
        foreach(Rigidbody rb in m_rbs) {
            rb.isKinematic = m_originalKinStat[rb.gameObject.GetInstanceID()];
        }
        m_isTeleporting = false;
    }

    private IEnumerator ActivateLocomotion() {
        /*
        if (!m_isMoving) {
            m_isMoving = true;
            if (m_screenFader != null) {

            } else {
                m_playerController.gameObject.transform.position = 
            }
        }
        yield return null;
        */
        // Deactivate thumbstick movement and rotation
        ChangePlayerMovement(false, false);
        // Deactivate custom grabber's custom pointer
        m_customGrabber.pointer.Deactivate();
        LocomoteFromFloor(m_customGrabber.pointer.locomotionPosition);
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
