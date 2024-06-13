using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class EVRA_SaccadeCalibrator : MonoBehaviour
{

    /// <summary>
    /// This script is meant to be added to `EVRA_GazeTracker` and is a core component in helping to 
    /// identify saccades from fixations. We do so using I-VT, which uses a velocity threshold to 
    /// differentiate between saccades and fixations. We do NOT consider smooth pursuit at the moment.
    ///
    /// In order to implement I-VT, we need to determine the necessary thresholds. For the angular velocity
    /// threshold, we default to 180 degrees/sec, as this was used in Qi Sun's Infinite Walking paper.
    /// However, we also want to consider angular acceleration in order to really differentiate a moving saccade
    /// and the initialization of a saccade. This is due to the note that saccadic suppression only
    /// occurs within 100ms of the initialization of a saccade. So we only care about the onset of a saccade.
    ///
    /// Do keep these in mind when implementing this component:
    /// - Attach this component to the main head of the user. This is because all calibration targets must be 
    ///     relative to the head.
    /// - All calibration targets must be spatially oriented relative to the center of the head and must be children
    ///     of this component.
    /// </summary>

    public enum TrackingType { Update, FixedUpdate, Coroutine }
    public enum TargetOrder { Sequential, Randomized }

    public bool initializeOnStart = true;
    public EVRA_GazeTracker gaze_tracker = null;
    public GameObject dummy_target = null;
    public List<GameObject> calibration_targets = new List<GameObject>();
    public TargetOrder target_order = TargetOrder.Randomized;
    public float targetDuration = 2f;

    public CSVWriter writer;
    public bool write_results = true;
    public TextMeshProUGUI status_textbox, velocity_textbox, acceleration_textbox;

    [SerializeField] private TrackingType _trackingType = TrackingType.FixedUpdate;
    [Range(1,200), SerializeField] private int _trackingFrequency = 200;

    private List<int> targetIndexOrder;
    private int curTargetIndex = -1;
    private IEnumerator updateCoroutineRef;
    private float curTargetStart = 0f;
    private float prevAngularVel = 0f;

    private List<float> angularVels = new List<float>();
    private List<float> angularAccels = new List<float>();

    [SerializeField, ReadOnly] private bool _initialized = false;
    public UnityEvent onEndCallback;

    // Start is called before the first frame update
    private void Awake() {
        // Set all calibration targets to be inactive
        foreach(GameObject obj in calibration_targets) obj.SetActive(false);
        // Determine the randomized order of calibration target appearance
        targetIndexOrder = new List<int>();
        int i = 0;
        while (i < calibration_targets.Count) targetIndexOrder.Add(i++); 
    }

    public void Start() {
        if (initializeOnStart) Initialize();
    }

    public void Initialize() {
        // Reset some variables
        curTargetIndex = -1;
        curTargetStart = 0f;
        prevAngularVel = 0f;
        angularVels = new List<float>();
        angularAccels = new List<float>();
        gaze_tracker.log_results = false;

        // Set all calibration targets to be inactive
        foreach(GameObject obj in calibration_targets) obj.SetActive(false);
        targetIndexOrder = new List<int>();
        int i = 0;
        while (i < calibration_targets.Count) targetIndexOrder.Add(i++); 

        // Only initialize the writer if we want to.
        if (write_results) {
            // Set the columns in the following order:
            writer.columns = new List<string> {
                "Timestamp",
                "Description",
                "ScreenPoint_X",
                "ScreenPoint_Y",
                "ScreenPoint_Z",
                "AngularVelocity",
                "AngularAcceleration"
            };
            // Initialize the writer
            writer.Initialize();
        }
        
        // Depending on tracking type, we need to initialize the coroutine if we want to use it
        if (_trackingType == TrackingType.Coroutine) {
            updateCoroutineRef = CoroutineUpdate();
            StartCoroutine(updateCoroutineRef);
        }
        
        // Declare that we've initialized
        _initialized = true;

        // If we have a status textbox, we need to output the results
        if (status_textbox != null) status_textbox.text = "Callibrating Saccades...";
        
        // Initialize the next target. If we have a dummy target, we don't call next_target
        if (dummy_target != null) {
            dummy_target.SetActive(true);
            curTargetStart = Time.time;
            if (writer.is_active) {
                writer.AddPayload(Time.time.ToString());
                writer.AddPayload($"Dummy Target");
                writer.AddPayload(Camera.main.WorldToScreenPoint(dummy_target.transform.position));
                writer.WriteLine(writer.writeUnixTime);
            }
        }
        else NextTarget();
    }

    private void Update() {
        if (_trackingType != TrackingType.Update) return;
        UpdateCallibrator(Time.deltaTime);
    }
    private void FixedUpdate() {
        if (_trackingType != TrackingType.FixedUpdate) return;
        UpdateCallibrator(Time.fixedDeltaTime);
    }
    private IEnumerator CoroutineUpdate() {
        // This only runs if the tracking type is set to TrackingType.Coroutine and thus doesn't need a check
        float deltaTime = 1f / (float)_trackingFrequency;
        while(true) {
            UpdateCallibrator(deltaTime);
            yield return new WaitForSeconds(deltaTime);
        }
    }

    private void UpdateCallibrator(float deltaTime) {
        // Don't continue if not initialized
        if (!_initialized) return;
        
        // Assign next target if our elapsed time surpasses the target duration
        float curTime = Time.time;
        if (curTime - curTargetStart >= targetDuration) NextTarget();

        // Get the necessary calculations!
        Vector3 screenPoint = gaze_tracker.gazePoint2D;
        float curAngularVel = gaze_tracker.angularVelocity;
        float curAngularAccel = (curAngularVel - prevAngularVel) / deltaTime;

        // Write to the writer
        if (writer.is_active) {
            writer.AddPayload(curTime.ToString());
            writer.AddPayload("Eye Gaze");
            writer.AddPayload(screenPoint);
            writer.AddPayload(curAngularVel);
            writer.AddPayload(curAngularAccel);
            writer.WriteLine(writer.writeUnixTime);
        }

        // Store the current angular velocity and angular acceleration into our list
        angularVels.Add(curAngularVel);
        angularAccels.Add(curAngularAccel);

        // Store the previous angular velocity, for the next angular acceleration calculation
        prevAngularVel = curAngularVel;
    }


    private void NextTarget() {
        // Avoid edge case where NextTarget() is called when this is actually not initializd yet.
        if (!_initialized) return;

        // set the current active target to disppear
        if (curTargetIndex != -1) calibration_targets[curTargetIndex].SetActive(false);
        else if (dummy_target != null) dummy_target.SetActive(false);

        // If targetIndexOrder's count == 0, then there will be no more indices.
        // In this case, just stop.
        if (targetIndexOrder.Count == 0) {
            foreach(GameObject obj in calibration_targets) obj.SetActive(false);
            EndCalibration();
            return;
        }

        // Choose the next target index
        int index = Random.Range(0, targetIndexOrder.Count);
        curTargetIndex = targetIndexOrder[index];
        targetIndexOrder.RemoveAt(index);

        // Enable the current active target
        calibration_targets[curTargetIndex].SetActive(true);
        
        // Reset the time counter
        curTargetStart = Time.time;
        
        // Add the start of the current target
        if (writer.is_active) {
            writer.AddPayload(curTargetStart.ToString());
            writer.AddPayload($"Calibration Target {calibration_targets.Count - targetIndexOrder.Count}");
            writer.AddPayload(Camera.main.WorldToScreenPoint(calibration_targets[curTargetIndex].transform.position));
            writer.WriteLine(writer.writeUnixTime);
        }
    } 


    public void EndCalibration() {
        // Can't do anything if we're not initialized
        if (!_initialized) return;

        // Tell ourselves that we want to terminate
        _initialized = false;

        // If the writer was set to be active, that means we were writing our results.
        // Let's stop the writer in this case.
        if (writer.is_active) writer.Disable();
        
        // If we were running via a coroutine, then we need to stop it
        if (updateCoroutineRef != null) StopCoroutine(updateCoroutineRef);

        // Disable any targets
        foreach(GameObject obj in calibration_targets) obj.SetActive(false);
        if (dummy_target != null) dummy_target.SetActive(false);

        // If we have the required textboxes, then we can print out the results
        if (status_textbox != null) status_textbox.text = "Calibration Ended!";
        
        // Unlike Disable, we WANT to invoke the callback as well as calculate the threshold(s)
        // The velocity threshold is... just a static value for now.
        // The angular threshold is 2x the standard deviation of the distribution formed by the angular accelerations
        float accel_sd = StandardDeviation(angularAccels);
        if (velocity_textbox != null) velocity_textbox.text = "180.0";
        if (acceleration_textbox != null) acceleration_textbox.text = (accel_sd*2f).ToString();
        gaze_tracker.log_results = true;
        onEndCallback?.Invoke();
    }

    public void Disable() {
        // Can't do anything if we're not initialized
        if (!_initialized) return;

        // Tell ourselves that we want to terminate
        _initialized = false;

        // If the writer was set to be active, that means we were writing our results.
        // Let's stop the writer in this case.
        if (writer.is_active) writer.Disable();
        
        // If we were running via a coroutine, then we need to stop it
        if (updateCoroutineRef != null) StopCoroutine(updateCoroutineRef);
        
        // Disable any targets
        foreach(GameObject obj in calibration_targets) obj.SetActive(false);
        if (dummy_target != null) dummy_target.SetActive(false);

        // If we have the required textboxes, then we can print out the results
        if (status_textbox != null) status_textbox.text = "Calibration Disabled...";
    }

    private void OnDestroy() {
        Disable(); 
    }

    public static float StandardDeviation(List<float> values) {
        float avg = values.Average();
        return Mathf.Sqrt(values.Average(v=>Mathf.Pow(v-avg,2)));
    }
}
