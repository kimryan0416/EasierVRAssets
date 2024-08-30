using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EVRA_GazeTracker : MonoBehaviour
{   
    public enum TrackingType { Update, FixedUpdate, Coroutine }
    public enum UpVector { World, Head }

    [Header("Head and Eyes")]
    public Transform headRef;
    public List<OVREyeGaze> eyes;

    [Header("Tracking Settings")]
    public float trackingRange = 10f;
    public TrackingType trackType = TrackingType.FixedUpdate;
    [Range(1,200)] public int trackingFrequency = 200;
    private IEnumerator updateCoroutine = null;
    public UpVector upVector = UpVector.World;

    [Header("Reticle Settings")]
    public Transform reticle;
    public float reticleSize = 0.25f;

    [Header("Recording References and Settings")]
    public bool log_results = true;
    public TextMeshProUGUI leftTextbox, rightTextbox, angularVelocityTextbox;
    public LineRenderer lr;
    public List<Vector3> lr_positions = new List<Vector3>();

    /// CACHED DATA AND VARAIBLES
    private Vector3 prevGazeWorld, prevGazeLocal;
    private Vector3 curGazeWorld, curGazeLocal;

    // OUTPUTS
    private float _angularVelocity = 0f;
    public float angularVelocity { get => _angularVelocity; set{} }
    private float _angleDiff = 0f;
    public float angleDiff { get => _angleDiff; set{} }
    private float _signedAngleDiff = 0f;
    public float signedAngleDiff { get => _signedAngleDiff; set{} }
    private string _saccade_status = "";
    public string saccade_status => _saccade_status;
    public Vector3 gazePoint => headRef.position + curGazeWorld * trackingRange;
    public Vector3 gazePoint2D => Camera.main.WorldToScreenPoint(gazePoint);

    
    private void Awake() {
        // Check references to the head reference (this transform) and the renderer for the reticle.
        if (headRef == null) headRef = this.transform;
        if (lr != null) lr.positionCount = 0;
    }

    private void Start() {
        SaveCurrent();          // Save the current, then the previous eye positions.
        SavePrev();
    }

    
    // During the actual in-game performance, we need to run Update/FixedUpdate/Coroutine for our functionality.
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
        // Get the current properties of the eye
        SaveCurrent();
        
        // Based on the current and previous properties, measure the angular velocity of the eye
        // Done by getting the angle between the local relative directions of the previous and current eye, and then dividing by the amount of time passed
        //float prevAngle = Vector3.SignedAngle(previousHeadOrientation, previousDirection, Vector3.up);
        //float currentAngle = Vector3.SignedAngle(currentHeadOrientation, currentDirection, Vector3.up);
        Vector3 orientationUp = (upVector == UpVector.Head) ? headRef.up : Vector3.up;
        _angleDiff = Vector3.Angle(prevGazeLocal, curGazeLocal);
        _signedAngleDiff = Vector3.SignedAngle( prevGazeLocal, curGazeLocal, orientationUp );
        _angularVelocity = _angleDiff / deltaTime;
        _saccade_status = (_angularVelocity > 180f) ? (_signedAngleDiff < 0f) ? "Left" : "Right" : "-";

        // Save the previous with the current
        SavePrev();

        // Reposition the reticle
        if (reticle != null) {
            reticle.position = gazePoint;
            reticle.localScale = Vector3.one * reticleSize * trackingRange;
            reticle.LookAt(headRef);
        }

        // Update the counter for the number of times this function was called.
        UpdateReadout();
    }

    private void SavePrev() {
        prevGazeWorld = curGazeWorld;
        prevGazeLocal = curGazeLocal;
    }

    private void SaveCurrent() {
        // Initialize Vector3's for average eye position and eye direction
        curGazeWorld = Vector3.zero;
        // Aggregate
        foreach(OVREyeGaze eye in eyes) {
            curGazeWorld += eye.transform.forward;
        }
        // Average, and finalize
        curGazeWorld = Vector3.Normalize(curGazeWorld);
        curGazeLocal = headRef.InverseTransformDirection(curGazeWorld);
    }

    private void UpdateReadout() {
        if (log_results && angularVelocityTextbox != null) angularVelocityTextbox.text = _saccade_status;

        if (lr != null && lr.enabled) {
            lr_positions.Add(gazePoint);
            lr.positionCount = lr_positions.Count;
            lr.SetPositions(lr_positions.ToArray());
        }
    }

    public void ToggleLineRenderer() {
        if (lr == null) return;
        lr_positions = new List<Vector3>();
        lr.positionCount = 0;
        lr.enabled = !lr.enabled;
    }

    public void ResetLineRenderer() {
        if (lr == null) return;
        lr_positions = new List<Vector3>();
        lr.positionCount = 0;
    }

    private void OnDestroy() {
        // This OnDestroy() runs whenever we end the application. This effectively ties loose ends.
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
    }
}
