using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_EyeTracker : MonoBehaviour
{
    [Tooltip("List of OVREyeGaze. A single OVREyeGaze will track only that eye; two OVREyeGaze's will track based on the average of the two trajectories.")]
    public List<OVREyeGaze> eyes;

    [Tooltip("How far will the eye tracking travel?")]
    public float gazeDistance = 10f;
    [Tooltip("What do we limit the eye tracking to hit?")]
    public LayerMask layersToInclude;
    [Tooltip("The tracking frequency (Hz)."), Range(1,90)]
    public int trackFrequency = 90;

    public Transform reticle = null;
    public float reticleSize = 0.025f;

    private Transform m_eyeTarget = null;
    public Transform eyeTarget { get { return m_eyeTarget; } set {} }

    private IEnumerator updateCoroutine;
    private Vector3 trackPosition, trackDir;
    private Vector3 prevTrackposition, prevTrackDir;

    // Start is called before the first frame update
    private void Start() {
        // Initialize our previous tracked positions
        CalculateAverages();



        updateCoroutine = UpdateTracking();
        StartCoroutine(updateCoroutine);
    }

    private IEnumerator UpdateTracking() {
        while(true) {
            // Calculate the average eye positions and directional vectors of all OVREyeGaze's
            CalculateAverages();
            // Calculate the angular velocity of 
            yield return new WaitForSeconds(1f/((float)trackFrequency));
        }
    }



    private void OnDestroy() {
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
    }

    private void CalculateAverages() {
        // Initialize
        trackPosition = Vector3.zero;
        trackDir = Vector3.zero;
        // Calculate totals
        foreach(OVREyeGaze gaze in eyes) {
            trackPosition += gaze.transform.position;
            trackDir += gaze.transform.forward;
        }
        // Get average position and normalized direction
        trackPosition = trackPosition / eyes.Count;
        trackDir = Vector3.Normalize(trackDir);
    }
}
