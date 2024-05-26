using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EVRA_MoveUI : MonoBehaviour
{
    public enum DisplayType { Off, Constant, Fade_In_Out, Fade_Out, Fade_In }

    public Transform positionTargetRef, lookAtTargetRef;
    private CanvasGroup canvasGroup;
    
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private AnimationCurve movementMultiplier;
    [SerializeField] private DisplayType displayType = DisplayType.Fade_In_Out;
    private DisplayType _displayType;   // Clone, for keeping cache
    [SerializeField] private float distanceThreshold = 2f;
    [SerializeField] private float fadeTimeThreshold = 2f;
    [SerializeField] private float fadeTimeRate = 0.5f;

    private float startTime = 0f;
    private float distanceToTarget = 0f;
    private float gradientValue = 0f;
    private bool isClose = true;

    void Start() {
        startTime = Time.time;                      // Time since the start of the application
        canvasGroup = GetComponent<CanvasGroup>();  // Get reference to canvas group
        _displayType = displayType;                 // Cache the display type
    }

    // Update is called once per frame
    void Update() {
        // Calculate the necessary changes
        CalculateUpdate();
        // Update!
        UpdatePosition();
        UpdateRotation();
        UpdateOpacity();
    }

    private void CalculateUpdate() {
        // Calculate the distance between our current position and the target position
        distanceToTarget = (positionTargetRef != null) ? Vector3.Distance(positionTargetRef.position, transform.position) : 0f;
        // Calculate if we're close enough to our target position or not
        isClose = distanceToTarget <  0.05f;
        // Calculate the gradient value based on distanceToTarget
        gradientValue = Mathf.Clamp(distanceToTarget/distanceThreshold, 0f, 1f);
        
    }

    private void UpdatePosition() {
        // If we're close enough, just set the position...
        // This also accounts for the issue of if positionTargetRef is not set.
        if (positionTargetRef == null) return;
        if (isClose) {
            transform.position = positionTargetRef.position;
            return;
        }
        // Calculate the step needed to be taken for translatio0n
        float step = movementSpeed * Time.deltaTime * movementMultiplier.Evaluate(gradientValue);
        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, positionTargetRef.position, step);
    }

    private void UpdateRotation() {
        // Only update rotation if lookAtTargetRef is not null
        if (lookAtTargetRef != null) 
            transform.rotation = Quaternion.LookRotation(transform.position - lookAtTargetRef.position);
    }

    private void UpdateOpacity(float toSetAlpha=1f) {
        float newAlpha = toSetAlpha;
        switch(displayType) {
            case DisplayType.Fade_In_Out:
                newAlpha = (!isClose) ? 1f - gradientValue : 1f;
                break;
            case DisplayType.Constant:
                newAlpha = 1f;
                break;
            case DisplayType.Fade_Out:
                newAlpha = (Time.time - startTime < fadeTimeThreshold) 
                    ? 1f 
                    : 1f - Mathf.Clamp((Time.time - startTime+fadeTimeThreshold)/fadeTimeRate, 0f, 1f);
                break;
            case DisplayType.Fade_In:
                newAlpha = (Time.time - startTime < fadeTimeThreshold) 
                    ? 0f 
                    : Mathf.Clamp((Time.time - startTime+fadeTimeThreshold)/fadeTimeRate, 0f, 1f);
                break;
            default:
                // Off is the default
                newAlpha = 0f;
                break;
        }
        canvasGroup.alpha = newAlpha;
    }
}
