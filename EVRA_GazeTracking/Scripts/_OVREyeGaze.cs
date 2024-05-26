/// <summary>
/// This class is an UPDATED version of the original `OVREyeGaze.cs`.
/// The only thing that can be done is replace the original with the instructions spliced throughout this file.
/// </summary>

using System;
using UnityEngine;
/// <remarks>
/// Simply add System.Collections like below.
/// </remarks>
using System.Collections;


[HelpURL("https://developer.oculus.com/reference/unity/latest/class_o_v_r_eye_gaze")]
public class _OVREyeGaze : MonoBehaviour
{
    /// <remarks>
    /// Range of frequency tracking. How frequently should we record the eye tracking?
    /// Simply add this as a variable.
    /// </remarks>
    [Range(1,90)] public int trackFrequency = 90;

    /// <remarks>
    /// A reference to the coroutine that runs the new update look (below)
    /// Simply add this as a variable
    /// </remarks>
    private IEnumerator updateCoroutine;

    /// <remarks>
    /// These variables are already in the original OVREyeGaze. They haven't been modified in any way.
    /// Therefore, these are just here to prevent the console from freaking out due to missing variables
    /// </remarks>
    public EyeId Eye;
    private Quaternion _initialRotationOffset;
    private Transform _viewTransform;
    public bool ApplyPosition = true;
    public bool ApplyRotation = true;
    public EyeTrackingMode TrackingMode;
    public float ConfidenceThreshold = 0.5f;
    public float Confidence { get; private set; }
    private OVRPlugin.EyeGazesState _currentEyeGazesState;
    public enum EyeId
    {
        Left = OVRPlugin.Eye.Left,
        Right = OVRPlugin.Eye.Right
    }
    public enum EyeTrackingMode
    {
        HeadSpace,
        WorldSpace,
        TrackingSpace
    }

    private void Awake()
    {
        /// <remarks>
        /// DO NOT REPLACE THE Awake() FUNCTION. But you need to add the line below to it.
        /// </remarks>
        updateCoroutine = CustomUpdate();
    }

    private void Start()
    {
        /// <remarks>
        /// DO NOT REPLACE THE Start() FUNCTION. But you need to add the line below to it.
        /// </remarks>
        StartCoroutine(updateCoroutine);
    }

    private void OnDestroy()
    {
        /// <remarks>
        /// DO NOT REPLACE THE OnDestroy() FUNCTION. But you need to add the line below to it.
        /// </remarks>
        if (updateCoroutine != null) StopCoroutine(updateCoroutine);
    }

    /// <remarks>
    /// There will be an Update() function in the original file. Feel free to comment it out.
    /// Instead, replace the Update() function with this 
    /// </remarks>
    private IEnumerator CustomUpdate() {
        while(true) {
            if (!OVRPlugin.GetEyeGazesState(OVRPlugin.Step.Render, -1, ref _currentEyeGazesState)) {
                yield return null;
                continue;
            }

            var eyeGaze = _currentEyeGazesState.EyeGazes[(int)Eye];

            if (!eyeGaze.IsValid) {
                yield return null;
                continue;
            }

            Confidence = eyeGaze.Confidence;
            if (Confidence < ConfidenceThreshold) {
                yield return null;
                continue;
            }

            var pose = eyeGaze.Pose.ToOVRPose();
            switch (TrackingMode) {
                case EyeTrackingMode.HeadSpace:
                    pose = pose.ToHeadSpacePose();
                    break;
                case EyeTrackingMode.WorldSpace:
                    pose = pose.ToWorldSpacePose(Camera.main);
                    break;
            }

            if (ApplyPosition) {
                transform.position = pose.position;
            }

            if (ApplyRotation) {
                transform.rotation = CalculateEyeRotation(pose.orientation);
            }

            yield return new WaitForSeconds(1f/(float)trackFrequency);
        }
    }

    /// <remarks>
    /// This is simply here to prevent the console from freaking out due to a missing function
    /// However, there isn't anything to change here.
    /// </remarks>
    private Quaternion CalculateEyeRotation(Quaternion eyeRotation)
    {
        var eyeRotationWorldSpace = _viewTransform.rotation * eyeRotation;
        var lookDirection = eyeRotationWorldSpace * Vector3.forward;
        var targetRotation = Quaternion.LookRotation(lookDirection, _viewTransform.up);

        return targetRotation * _initialRotationOffset;
    }
}
