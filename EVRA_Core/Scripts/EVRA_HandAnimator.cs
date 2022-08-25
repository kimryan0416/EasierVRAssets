using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EVRA;
using EVRA.Inputs;
using TMPro;

[RequireComponent(typeof(Animator))]
public class EVRA_HandAnimator : MonoBehaviour
{
    [SerializeField]
    private float m_animationSpeed = 20f;

    private Animator m_animator;
    private EVRA_Hand m_hand;

    // We want to track the Trigger and Grip
    private string indexTriggerTouchEventName = "Touch Index Trigger Animation";
    private float touchIndexCurrent, touchIndexTarget;
    private string indexTriggerEventName = "Index Trigger Animation";
    private float indexCurrent, indexTarget;
    private string indexAnimatorParam = "IndexTrigger";
    private float indexTo;
    private bool shouldUpdateIndexTrigger = false;

    private string gripTriggerEventName = "Grip Trigger Animation";
    private float gripCurrent, gripTarget;
    private string gripTriggerAnimatorParam = "GripTrigger";

    private string touchThumbstickName = "Touch Thumbstick Animation";
    private float touchThumbstickCurrent, touchThumbstickTarget;
    private string touchThumbstickAnimatorParam = "ThumbstickTouch";

    private string thumbstickEventName = "Thumbstick Animation";
    private float thumbstickXCurrent, thumbstickXTarget;
    private float thumbstickYCurrent, thumbstickYTarget;
    private string thumbstickXAnimatorParam = "ThumbstickX";
    private string thumbstickYAnimatorParam = "ThumbstickY";

    private string touchButtonOneEventName = "Touch Button One Animation";
    private float touchButtonOneCurrent, touchButtonOneTarget;
    private string buttonOneEventName = "Button One Animation";
    private float buttonOneCurrent, buttonOneTarget;
    private float buttonOneTo;
    private bool shouldUpdateButtonOne = false;
    private string buttonOneAnimatorParam = "ButtonOne";

    private string touchButtonTwoEventName = "Touch Button Two Animation";
    private float touchButtonTwoCurrent, touchButtonTwoTarget;
    private string buttonTwoEventName = "Button Two Animation";
    private float buttonTwoCurrent, buttonTwoTarget;
    private float buttonTwoTo;
    private bool shouldUpdateButtonTwo = false;
    private string buttonTwoAnimatorParam = "ButtonTwo";

    private string gripEventName = "Grip Animation";
    // We use GripCurrent and GripTarget, so we don't need variables for this one
    private string gripAnimatorParam = "Grip";
    
    void Awake() {
        m_animator = GetComponent<Animator>();
    }
    
    public void Init(EVRA_Hand hand) {
        m_hand = hand;
        if (m_hand.ovrControllerHelper == null) {
            // There's no controller - we need to delete ourselves
            gameObject.SetActive(false);
            return;
        }
        // if our hand is the RIGHT hand, we modify the object to match the right hand direction (it's defaulted to left)
        if (m_hand.OVRController == OVRInput.Controller.RTouch) {
            transform.position = new Vector3(
                transform.position.x * -1f,
                transform.position.y, 
                transform.position.z
            );
            transform.localScale = new Vector3(
                transform.localScale.x,
                transform.localScale.y * -1f,
                transform.localScale.z
            );
        }
        AddControllerAnimations();
    }

    void AddControllerAnimations() {
         // Index Trigger
        m_hand.AddNewEvent(
            indexTriggerEventName,
            new InputEventMap(InputType.IndexTrigger, InputEvent.Any),
            this.UpdateIndex
        );

        // Grip Triger
        m_hand.AddNewEvent(
            gripTriggerEventName,
            new InputEventMap(InputType.GripTrigger, InputEvent.Any),
            this.UpdateGrip
        );

        // Thumbstick
        m_hand.AddNewEvent(
            touchThumbstickName,
            new InputEventMap(InputType.TouchThumbstick, InputEvent.Any),
            this.UpdateTouchThumbstick
        );
        m_hand.AddNewEvent(
            thumbstickEventName,
            new InputEventMap(InputType.Thumbstick, InputEvent.Any),
            this.UpdateThumbstick
        );

        // Button One
        m_hand.AddNewEvent(
            touchButtonOneEventName,
            new InputEventMap(InputType.TouchButtonOne, InputEvent.Any),
            this.UpdateTouchButtonOne
        );
        m_hand.AddNewEvent(
            buttonOneEventName,
            new InputEventMap(InputType.ButtonOne, InputEvent.Any),
            this.UpdateButtonOne
        );
            
        // Button Two
        m_hand.AddNewEvent(
            touchButtonTwoEventName,
            new InputEventMap(InputType.TouchButtonTwo, InputEvent.Any),
            this.UpdateTouchButtonTwo
        );
        m_hand.AddNewEvent(
            buttonTwoEventName,
            new InputEventMap(InputType.ButtonTwo, InputEvent.Any),
            this.UpdateButtonTwo
        );
    }

    void Update() {
        if (m_hand == null) return;
        AnimateHand();
    }

    void UpdateIndex(InputEventDataPackage package) {
        indexTarget = package.inputs[InputType.IndexTrigger].response.direction.x;
    }

    void UpdateGrip(InputEventDataPackage package) {
        gripTarget = package.inputs[InputType.GripTrigger].response.direction.x;
    }

    void UpdateTouchThumbstick(InputEventDataPackage package) {
        touchThumbstickTarget = Mathf.Max(touchThumbstickTarget, package.inputs[InputType.TouchThumbstick].response.direction.x);
    }
    void UpdateThumbstick(InputEventDataPackage package) {
        Vector2 dir = package.inputs[InputType.Thumbstick].response.direction;
        thumbstickXTarget = CommonFunctions.RemapFloat(dir.x, -1f, 1f, 0f, 1f);
        thumbstickYTarget = CommonFunctions.RemapFloat(dir.y, -1f, 1f, 0f, 1f);
        //touchThumbstickTarget = Remap(Vector2.ClampMagnitude(dir, 0.1f).magnitude, 0f,0.1f, 0f,1f);
        touchThumbstickTarget = (dir != Vector2.zero) ? 1f : package.inputs[InputType.TouchThumbstick].response.direction.x;
    }

    void UpdateTouchButtonOne(InputEventDataPackage package) {
        touchButtonOneTarget = package.inputs[InputType.TouchButtonOne].response.direction.x;
    }
    void UpdateButtonOne(InputEventDataPackage package) {
        buttonOneTarget = package.inputs[InputType.ButtonOne].response.direction.x;
    }

    void UpdateTouchButtonTwo(InputEventDataPackage package) {
        touchButtonTwoTarget = package.inputs[InputType.TouchButtonTwo].response.direction.x;
    }
    void UpdateButtonTwo(InputEventDataPackage package) {
        buttonTwoTarget = package.inputs[InputType.ButtonTwo].response.direction.x;
    }

    void AnimateHand() {
        // Animate Index Trigger
        AnimateIndexTrigger();
        // Animate Grip Trigger
        AnimateGripTrigger();
        // Animate Thumbstick
        AnimateThumbstick();
        // Animate Button Two
        AnimateButtonTwo();
        // Animate Button One
        AnimateButtonOne();
    }

    void AnimateIndexTrigger() {
        // There's two phases: we first need to consider touch index, then we need to consider actual press
        if (indexCurrent != indexTarget) {
            indexCurrent = Mathf.MoveTowards(indexCurrent, indexTarget, Time.deltaTime * m_animationSpeed);
            m_animator.SetFloat(indexAnimatorParam, indexCurrent);
        }
    }

    void AnimateGripTrigger() {
        if (gripCurrent != gripTarget) {
            gripCurrent = Mathf.MoveTowards(gripCurrent, gripTarget, Time.deltaTime * m_animationSpeed);
            m_animator.SetFloat(gripTriggerAnimatorParam, gripCurrent);
        }
    }

    void AnimateThumbstick() {
        if (touchThumbstickCurrent != touchThumbstickTarget) {
            touchThumbstickCurrent = Mathf.MoveTowards(touchThumbstickCurrent, touchThumbstickTarget, Time.deltaTime * m_animationSpeed);
            m_animator.SetFloat(touchThumbstickAnimatorParam, touchThumbstickCurrent);
        }
        /*
        if (thumbstickXCurrent != thumbstickXTarget) {
            thumbstickXCurrent = Mathf.MoveTowards(thumbstickXCurrent, thumbstickXTarget, Time.deltaTime * m_animationSpeed);
            m_animator.SetFloat(thumbstickXAnimatorParam, thumbstickXCurrent);
        }
        if (thumbstickYCurrent != thumbstickYTarget) {
            thumbstickYCurrent = Mathf.MoveTowards(thumbstickYCurrent, thumbstickYTarget, Time.deltaTime * m_animationSpeed);
            m_animator.SetFloat(thumbstickYAnimatorParam, thumbstickYCurrent);
        }
        */
    }

    void AnimateButtonOne() {
        if (buttonTwoTo > 0f) {
            m_animator.SetFloat(buttonOneAnimatorParam, 0f);
            return;
        }
        shouldUpdateButtonOne = false;
        if (touchButtonOneCurrent != touchButtonOneTarget) {
            touchButtonOneCurrent = Mathf.MoveTowards(touchButtonOneCurrent, touchButtonOneTarget, Time.deltaTime * m_animationSpeed);
            shouldUpdateButtonOne = true;
        }
        if (touchButtonOneCurrent > 0f && buttonOneCurrent != buttonOneTarget) {
            buttonOneCurrent = Mathf.MoveTowards(buttonOneCurrent, buttonOneTarget, Time.deltaTime * m_animationSpeed);
            shouldUpdateButtonOne = true;
        }
        if (shouldUpdateButtonOne) {
            buttonOneTo = (touchButtonOneCurrent > 0.5f) ? 0.90f + (buttonOneCurrent * 0.1f) : touchButtonOneCurrent * 0.90f;
            m_animator.SetFloat(buttonOneAnimatorParam, buttonOneTo);
        }
    }

    void AnimateButtonTwo() {
        shouldUpdateButtonTwo = false;
        if (touchButtonTwoCurrent != touchButtonTwoTarget) {
            touchButtonTwoCurrent = Mathf.MoveTowards(touchButtonTwoCurrent, touchButtonTwoTarget, Time.deltaTime * m_animationSpeed);
            shouldUpdateButtonTwo = true;
        }
        if (touchButtonTwoCurrent > 0f && buttonTwoCurrent != buttonTwoTarget) {
            buttonTwoCurrent = Mathf.MoveTowards(buttonTwoCurrent, buttonTwoTarget, Time.deltaTime * m_animationSpeed);
            shouldUpdateButtonTwo = true;
        }
        if (shouldUpdateButtonTwo) {
            buttonTwoTo = (touchButtonTwoCurrent > 0.5f) ? 0.9f + (buttonTwoCurrent * 0.1f) : touchButtonTwoCurrent * 0.9f;
            m_animator.SetFloat(buttonTwoAnimatorParam, buttonTwoTo);
        }
    }
}
