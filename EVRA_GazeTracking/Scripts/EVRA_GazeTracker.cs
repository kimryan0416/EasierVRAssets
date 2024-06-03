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
    private Renderer reticleRenderer;
    public float reticleSize = 0.25f;

    [Header("Recording References and Settings")]
    public TextMeshProUGUI leftTextbox, rightTextbox, angularVelocityTextbox;
    public LineRenderer lr;
    public List<Vector3> lr_positions = new List<Vector3>();

    /// CACHED DATA AND VARAIBLES
    private Vector3 previousGazePosition, previousGazeOrientation, previousHeadOrientation;
    private Vector3 currentGazePosition, currentGazeOrientation, currentHeadOrientation;

    // OUTPUTS
    public float angularVelocity = 0f;
    public Vector3 gazePoint => currentGazePosition + currentGazeOrientation * trackingRange;

    
    private void Awake() {
        // Check references to the head reference (this transform) and the renderer for the reticle.
        if (headRef == null) headRef = this.transform;
        if (reticle != null) reticleRenderer = reticle.GetComponent<Renderer>();
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
        float signedAngle = Vector3.SignedAngle(
            headRef.InverseTransformDirection(previousGazeOrientation), 
            headRef.InverseTransformDirection(currentGazeOrientation), 
            orientationUp
        );
        angularVelocity = signedAngle / deltaTime;

        // Save the previous with the current
        SavePrev();

        // Reposition the reticle
        if (reticle != null) {
            reticle.position = gazePoint;
            reticle.localScale = Vector3.one * reticleSize * trackingRange;
            reticle.LookAt(headRef);
            if (reticleRenderer != null) reticleRenderer.enabled = (angularVelocity >= 90f) ? false : true;
        }

        // Update the counter for the number of times this function was called.
        UpdateReadout();
    }

    private void SavePrev() {
        previousGazePosition = currentGazePosition;
        previousGazeOrientation = currentGazeOrientation;
        previousHeadOrientation = currentHeadOrientation;
    }

    private void SaveCurrent() {
        // Initialize Vector3's for average eye position and eye direction
        currentGazePosition = Vector3.zero;
        currentGazeOrientation = Vector3.zero;
        // Aggregate
        foreach(OVREyeGaze eye in eyes) {
            currentGazePosition += eye.transform.position;
            currentGazeOrientation += eye.transform.forward;
        }
        // Average, and finalize
        currentGazePosition /= eyes.Count;
        currentGazeOrientation = Vector3.Normalize(currentGazeOrientation);
        currentHeadOrientation = headRef.forward;
    }

    private void UpdateReadout() {
        foreach(OVREyeGaze eye in eyes) {
            if (eye.Eye == OVREyeGaze.EyeId.Left) {
                if (leftTextbox != null) leftTextbox.text = eye.hzCounter.ToString();
            }
            else {
                if (rightTextbox != null) rightTextbox.text = eye.hzCounter.ToString();
            }

            if (lr != null) {
                lr_positions.Add(reticle.position);
                lr.SetPositions(lr_positions.ToArray());
                lr.positionCount = lr_positions.Count;
            }

            if (angularVelocityTextbox != null) angularVelocityTextbox.text = ((int)(angularVelocity*100.0f)/100.0f).ToString();
        }
    }

    private void OnDestroy() {
        // This OnDestroy() runs whenever we end the application. This effectively ties loose ends.
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
    }
}
