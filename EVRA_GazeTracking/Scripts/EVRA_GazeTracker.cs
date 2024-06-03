using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EVRA_GazeTracker : MonoBehaviour
{   
    public enum TrackingType { Update, FixedUpdate, Coroutine }
    public enum TrackingEyes { Left, Right, Both }

    [Header("Head and Eyes")]
    public Transform headRef;
    //public TrackingEyes _eyes = TrackingEyes.Both;
    public List<OVREyeGaze> eyes;

    [Header("Reticle Settings")]
    public Transform reticle;
    private Renderer reticleRenderer;
    public float reticleSize = 0.25f;

    [Header("Tracking Settings")]
    public float trackingRange = 10f;
    [Range(1,200)] public int trackingFrequency = 200;
    public TrackingType trackType = TrackingType.FixedUpdate;
    private IEnumerator updateCoroutine = null;

    [Header("Other References and Settings")]
    public TextMeshProUGUI textbox;

    /// CACHED DATA AND VARAIBLES
    // Perms for eye tracking
    //private const OVRPermissionsRequester.Permission EyeTrackingPermission = OVRPermissionsRequester.Permission.EyeTracking;
    // Reference to our own OnPermissionGranted(string permissionId), which we use in certain cases.
    //private Action<string> _onPermissionGranted;
    //
    private OVRPlugin.EyeGazesState _currentEyeGazesState;
    //
    private Vector3 previousPosition, previousDirection, previousRelativeDirection, previousHeadOrientation;
    private Vector3 currentPosition, currentDirection, currentRelativeDirection, currentHeadOrientation;
    //
    private float angularVelocity;
    private List<float> timestamps = new List<float>();
    private float startingTime;
    private float endingTime;
    private int hzCounter = 0;
    
    private void Awake() {
        // Check references to the head reference (this transform) and the renderer for the reticle.
        if (headRef == null) headRef = this.transform;
        reticleRenderer = reticle.GetComponent<Renderer>();
    }



    // Start is called before the first frame update
    private void Start() {
        // Initialize the readout for analyzing frequency hz in a cave
        InitializeReadout();
        // Save the current, then the previous eye positions.
        SaveCurrent();
        SavePrev();
    }

    
    // During the actual in-game performance, we need to run Update/FixedUpdate/Coroutine for our functionality.
    // The Update 
    private void Update() {
        if (trackType != TrackingType.Update) return;
        UpdateGaze(Time.deltaTime);
    }

    // The FixedUpdate() DOES run separately from the update cycle. Caveat: it runs off of the fixed timestep
    // that the Physics engine also uses.
    // Make sure you're really sure about this before depending on this function.
    private void FixedUpdate() {
        if (trackType != TrackingType.FixedUpdate) return;
        UpdateGaze(Time.fixedDeltaTime);
    }

    // Coroutines are NOT separate from the Update cycle. Whenever you call `yield return`, you are merely
    // waiting for the next available frame. This means that Coroutines, despite offering the most
    // flexible option, are NOT necessarily going to be deterministic because
    // they are dependent on the next available frame.
    private IEnumerator UpdateTracking() {
        if (trackType == TrackingType.Coroutine) {
            float deltaTime = 1f/(float)trackingFrequency;
            while(true) {
                UpdateGaze(deltaTime);
                yield return new WaitForSeconds(deltaTime);
            }
        }
    }


    public void UpdateGaze(float deltaTime) {
        // Update the counter for the number of times this function was called.
        UpdateReadout();

        // Get the current properties of the eye
        SaveCurrent();
        
        // Based on the current and previous properties, measure the angular velocity of the eye
        // Done by getting the angle between the local relative directions of the previous and current eye, and then dividing by the amount of time passed
        float prevAngle = Vector3.SignedAngle(previousHeadOrientation, previousDirection, Vector3.up);
        float currentAngle = Vector3.SignedAngle(currentHeadOrientation, currentDirection, Vector3.up);
        angularVelocity = (currentAngle - prevAngle) / deltaTime;

        // Reposition the reticle
        if (reticle != null) {
            reticle.position = currentPosition + currentDirection*trackingRange;
            reticle.localScale = Vector3.one * reticleSize * trackingRange;
            reticle.LookAt(headRef);
            if (reticleRenderer != null) reticleRenderer.enabled = (angularVelocity >= 90f) ? false : true;
        }
        // Save the previous with the current
        
        SavePrev();
    }

    private void SavePrev() {
        previousPosition = currentPosition;
        previousDirection = currentDirection;
        previousRelativeDirection = currentRelativeDirection;
        previousHeadOrientation = currentHeadOrientation;
    }

    private void SaveCurrent() {
        // Initialize Vector3's for average eye position and eye direction
        currentPosition = Vector3.zero;
        currentDirection  = Vector3.zero;
        currentRelativeDirection = Vector3.zero;
        // Aggregate
        foreach(OVREyeGaze eye in eyes) {
            currentPosition += eye.transform.position;
            currentDirection += eye.transform.forward;
            currentRelativeDirection += headRef.InverseTransformVector(eye.transform.forward).normalized;
        }
        // Average, and finalize
        currentPosition /= eyes.Count;
        currentDirection = Vector3.Normalize(currentDirection);
        currentRelativeDirection = Vector3.Normalize(currentRelativeDirection);
        currentHeadOrientation = headRef.forward;
    }

    private void InitializeReadout() {
        startingTime = Time.time;
        endingTime = startingTime+1.0f;
        hzCounter = 0;
    }

    private void UpdateReadout() {
        // update timestamps
        float curTime = Time.time;
        if (curTime-startingTime > endingTime) {
            textbox.text = hzCounter.ToString();
            hzCounter = 0;
            startingTime = endingTime;
            endingTime += 1.0f;
        }
        hzCounter += 1;
    }

    private void OnDestroy() {
        // This OnDestroy() runs whenever we end the application. This effectively ties loose ends.
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
    }
}
