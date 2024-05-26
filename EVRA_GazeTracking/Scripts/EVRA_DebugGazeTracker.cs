using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_DebugGazeTracker : MonoBehaviour
{
    public Transform headRef;
    public List<OVREyeGaze> eyes;

    public Transform reticle;
    private Renderer reticleRenderer;
    public float reticleSize = 0.25f;
    public float trackingRange = 10f;
    [Range(1,90)] public int trackingFrequency = 90;

    private IEnumerator updateCoroutine;
    private Vector3 previousPosition, previousDirection, previousRelativeDirection, previousHeadOrientation;
    private Vector3 currentPosition, currentDirection, currentRelativeDirection, currentHeadOrientation;
    private float angularVelocity;

    private void Awake() {
        if (headRef == null) headRef = this.transform;
        reticleRenderer = reticle.GetComponent<Renderer>();
    }

    // Start is called before the first frame update
    private void Start() {
        // First, save previous as a pre-emptive measure
        SaveCurrent();
        SavePrev();
        // Start the coroutine update
        updateCoroutine = UpdateTracking();
        StartCoroutine(updateCoroutine);
    }

    private IEnumerator UpdateTracking() {
        while(true) {
            // Get the current properties of the eye
            SaveCurrent();
            // Based on the current and previous properties, measure the angular velocity of the eye
            // Done by getting the angle between the local relative directions of the previous and current eye, and then dividing by the amount of time passed
            float prevAngle = Vector3.SignedAngle(previousHeadOrientation, previousDirection, Vector3.up);
            float currentAngle = Vector3.SignedAngle(currentHeadOrientation, currentDirection, Vector3.up);
            angularVelocity = (currentAngle - prevAngle) / (1f/(float)trackingFrequency);

            // Reposition the reticle
            if (reticle != null) {
                reticle.position = currentPosition + currentDirection*trackingRange;
                reticle.localScale = Vector3.one * reticleSize * trackingRange;
                reticle.LookAt(headRef);
                if (reticleRenderer != null) reticleRenderer.enabled = (angularVelocity >= 90f) ? false : true;
            }
            // Save the previous with the current
            SavePrev();
            
            // Yield
            yield return new WaitForSeconds(1f/(float)trackingFrequency);
        }
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

    private void OnDestroy() {
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
    }
}
