using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace EVRA {

    namespace Inputs {
        public enum InputType {
            ButtonOne,
            ButtonTwo,
            ButtonStart,
            ButtonThumbstick,
            IndexTrigger,
            GripTrigger,
            Thumbstick,

            TouchButtonOne,
            TouchButtonTwo,
            TouchIndexTrigger,
            TouchThumbstick
        }
        public enum InputEvent {
            Rest,       // not down on multiple frames (button), at rest on multiple frames (thumbstick)
            Down,       // down on this update frame (button), not at rest on this frame (thumbstick)
            Stay,       // down on multiple frames (button), not at rest on multiple frames (thumbstick)
            Up,         // not down on this frame (button), at rest on this frame (thumbstick)
            Changed,    // As long as it wasn't the same as the previous frame
            Any,        // Whichever it happens to be
        }

        [Serializable]
        public class InputData {
            public Vector2 direction, angle;
            public InputData() {
                this.direction = Vector2.zero;
                this.angle = Vector2.zero;
            }
            public InputData(Vector2 direction) {
                this.direction = direction;
                this.angle = Vector2.zero;
            }
            public InputData(Vector2 direction, Vector2 angle) {
                this.direction = direction;
                this.angle = angle;
            }
        }

        public class InputMethods {
            public static Dictionary<InputType, System.Func<OVRInput.Controller, InputData>> InputMap = new Dictionary<InputType, System.Func<OVRInput.Controller, InputData>>() {
                {InputType.ButtonOne, (controller) => GetButtonOneInput(controller)},
                {InputType.ButtonTwo, (controller) => GetButtonTwoInput(controller)},
                {InputType.ButtonStart, (controller) => GetButtonStartInput(controller)},
                {InputType.ButtonThumbstick, (controller) => GetButtonThumbstickInput(controller)},
                {InputType.IndexTrigger, (controller) => GetIndexTriggerInput(controller)},
                {InputType.GripTrigger, (controller) => GetGripTriggerInput(controller)},
                {InputType.Thumbstick, (controller) => GetThumbstickInput(controller)},

                {InputType.TouchButtonOne, (controller) => GetTouchInput(OVRInput.Touch.One, controller)},
                {InputType.TouchButtonTwo, (controller) => GetTouchInput(OVRInput.Touch.Two, controller)},
                {InputType.TouchIndexTrigger, (controller) => GetTouchInput(OVRInput.Touch.PrimaryIndexTrigger, controller)},
                {InputType.TouchThumbstick, (controller) => GetTouchInput(OVRInput.Touch.PrimaryThumbstick, controller)}
            };
            public static InputData GetButtonInput(OVRInput.Button input, OVRInput.Controller controller) {
                float value = (OVRInput.Get(input, controller)) ? 1f : 0f;
                return new InputData(new Vector2(value, 0f));
            }
            public static InputData GetAxis1DInput(OVRInput.Axis1D input, OVRInput.Controller controller) {
                float value = OVRInput.Get(input, controller);
                return new InputData(new Vector2(value, 0f));
            }
            public static InputData GetAxis2DInput(OVRInput.Axis2D input, OVRInput.Controller controller) {
                Vector2 direction = Vector2.ClampMagnitude(OVRInput.Get(input, controller), 1f);
                Vector2 angle = new Vector2(CommonFunctions.GetAngleFromVector2(direction, controller), Vector2.Distance(Vector2.zero, direction));
                return new InputData(direction, angle);
            }
            public static InputData GetTouchInput(OVRInput.Touch input, OVRInput.Controller controller) {
                float value = (OVRInput.Get(input, controller)) ? 1f : 0f;
                return new InputData(new Vector2(value, 0f));
            }
            public static InputData GetButtonOneInput(OVRInput.Controller controller) {
                return GetButtonInput(OVRInput.Button.One, controller);
            }
            public static InputData GetButtonTwoInput(OVRInput.Controller controller) {
                return GetButtonInput(OVRInput.Button.Two, controller);
            }
            public static InputData GetButtonStartInput(OVRInput.Controller controller) {
                return GetButtonInput(OVRInput.Button.Start, controller);
            }
            public static InputData GetButtonThumbstickInput(OVRInput.Controller controller) {
                return GetButtonInput(OVRInput.Button.PrimaryThumbstick, controller);
            }
            public static InputData GetIndexTriggerInput(OVRInput.Controller controller) {
                return GetAxis1DInput(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
            }
            public static InputData GetGripTriggerInput(OVRInput.Controller controller) {
                return GetAxis1DInput(OVRInput.Axis1D.PrimaryHandTrigger, controller);
            }
            public static InputData GetThumbstickInput(OVRInput.Controller controller) {
                return GetAxis2DInput(OVRInput.Axis2D.PrimaryThumbstick, controller);
            }
            public static InputEvent GetCurrentState(bool isDown, bool prevDown) {
                if (!isDown && !prevDown) return InputEvent.Rest;
                if (isDown && prevDown) return InputEvent.Stay;
                if (isDown && !prevDown) return InputEvent.Down;
                if (!isDown && prevDown) return InputEvent.Up;
                if (isDown != prevDown) return InputEvent.Changed;
                return InputEvent.Rest;
            }
        }

        [Serializable]
        public class InputResult {
            public InputData response;
            public InputEvent currentState = InputEvent.Rest;
            //private System.Func<OVRInput.Controller, InputData> GetMethod;
            private bool isDown = false, prevDown = false;
            public void CheckInput(InputType input, OVRInput.Controller controller = OVRInput.Controller.None) {
                this.response = InputMethods.InputMap[input](controller);
                this.isDown = this.response.direction.magnitude > 0.1f;
                this.currentState = InputMethods.GetCurrentState(this.isDown, this.prevDown);
                this.prevDown = this.isDown;
            }
        }

        [Serializable]
        public class InputEventMap {
            public InputType input;
            public InputEvent state;
            public InputEventMap() {}
            public InputEventMap(InputType input, InputEvent state) {
                this.input = input;
                this.state = state;
            }
        }

        [Serializable]
        public class InputEventDataPackage {
            public EVRA_Hand hand;
            public Dictionary<InputType, InputResult> inputs;
            public InputEventDataPackage(EVRA_Hand hand, Dictionary<InputType, InputResult> inputs) {
                this.hand = hand;
                this.inputs = inputs;
            }
        }

    }

    namespace Events {
        [Serializable]
        public class EVRA_Event : UnityEvent<Inputs.InputEventDataPackage> {}

        [Serializable]
        public class EVRA_OldEvent : UnityEvent {}

        [Serializable]
        public class EVRA_OldThumbstick : UnityEvent<Vector2> {}
    }

    namespace Locomotion {
        [Serializable]
        public class LocomotionFunctions {
            // XY movement relative to a potential parent?
            public static Vector3 InputRelativeToParent(Vector2 input, Transform parent = null) {
                Vector2 direction;
                if (parent) {
                    Vector3 dir = parent.TransformDirection(new Vector3(input.x, 0f, input.y));
                    direction = new Vector2(dir.x, dir.z);
                } else {
                    direction = input;
                }
                return direction;
            }

            public static Vector3 InputRelativeToParent(Vector3 input, Transform parent = null) {
                Vector3 direction = (parent) ? parent.TransformDirection(input) : input;
                return direction;
            }
        }
    }

    [Serializable]
    public class CommonFunctions {

        // idsToIgnore = gameobject IDs
        public static List<Out> GetInRange<Out, Check>(Transform origin, float rad, int idToIgnore) {
            if (origin == null) return null;
            Dictionary<Out, float> possible = new Dictionary<Out, float>();
            Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
            foreach (Collider c in hitColliders) {
                if (c.GetComponent<Check>() != null && idToIgnore!=c.gameObject.GetInstanceID()) {
                    possible.Add(c.GetComponent<Out>(), Vector3.Distance(origin.position, c.transform.position));
                }
            }
            List<Out> inRange = possible.OrderBy(x => x.Value).Select(pair => pair.Key).ToList();
            
            //inRange = inRange.OrderBy(x => Vector3.Distance(origin.position, x.transform.position)).ToList();
            return inRange;
        }

        public static List<Out> GetInRange<Out, Check, Exclude>(Transform origin, float rad) {
            if (origin == null) return null;
            Dictionary<Out, float> possible = new Dictionary<Out, float>();
            Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
            foreach (Collider c in hitColliders) {
                if (c.GetComponent<Check>() != null && c.GetComponent<Exclude>() == null) {
                    //inRange.Add(c.GetComponent<Out>());
                    possible.Add(c.GetComponent<Out>(), Vector3.Distance(origin.position, c.transform.position));
                }
            }
            List<Out> inRange = possible.OrderBy(x => x.Value).Select(pair => pair.Key).ToList();
            
            //inRange = inRange.OrderBy(x => Vector3.Distance(origin.position, x.transform.position)).ToList();
            return inRange;
        }
        public static List<Out> GetInRange<Out, Check, Exclude>(Transform origin, float rad, int layerToAvoid) {
            if (origin == null) return null;
            Dictionary<Out, float> possible = new Dictionary<Out, float>();
            Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
            foreach (Collider c in hitColliders) {
                if (c.GetComponent<Check>() != null && c.GetComponent<Exclude>() == null && layerToAvoid != c.gameObject.layer) {
                    //inRange.Add(c.GetComponent<Out>());
                    possible.Add(c.GetComponent<Out>(), Vector3.Distance(origin.position, c.transform.position));
                }
            }
            List<Out> inRange = possible.OrderBy(x => x.Value).Select(pair => pair.Key).ToList();
            
            //inRange = inRange.OrderBy(x => Vector3.Distance(origin.position, x.transform.position)).ToList();
            return inRange;
        }
        public static List<Out> GetInRange<Out, Check, Exclude>(Transform origin, float rad, List<int> layersToAvoid) {
            if (origin == null) return null;
            Dictionary<Out, float> possible = new Dictionary<Out, float>();
            Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
            foreach (Collider c in hitColliders) {
                if (c.GetComponent<Check>() != null && c.GetComponent<Exclude>() == null && !layersToAvoid.Contains(c.gameObject.layer)) {
                    //inRange.Add(c.GetComponent<Out>());
                    possible.Add(c.GetComponent<Out>(), Vector3.Distance(origin.position, c.transform.position));
                }
            }
            List<Out> inRange = possible.OrderBy(x => x.Value).Select(pair => pair.Key).ToList();
            
            //inRange = inRange.OrderBy(x => Vector3.Distance(origin.position, x.transform.position)).ToList();
            return inRange;
        }

        public static float GetAngleFromVector2(Vector2 original, OVRInput.Controller source = OVRInput.Controller.None) {
            // Derive angle from y and x
            float angle = (original.x < 0) ? 360f - (Mathf.Atan2(original.x, original.y) * Mathf.Rad2Deg * -1) : Mathf.Atan2(original.x, original.y) * Mathf.Rad2Deg;
            // We need to recenter the angle so that it's between 0 and 360, not 5 and 365
            angle = (angle > 360f) ? angle - 360 : angle;
            // Return
            return angle;
        }

        public static bool HasComponent<T> (GameObject obj) {
            return (obj.GetComponent<T>() as Component) != null;
        }

        public static float RemapFloat(float input, float oldLow, float oldHigh, float newLow, float newHigh) {
            float t = Mathf.InverseLerp(oldLow, oldHigh, input);
            return Mathf.Lerp(newLow, newHigh, t);
        }
    }

    namespace PseudoParenting {
        [Serializable]
        public class FakeChild {
            public Transform parent, child;
            public Vector3  pos = Vector3.zero,
                            fw = Vector3.forward, 
                            up = Vector3.up;
            
            public void CalculateOffsets(Transform parent, Transform child) {
                this.parent = parent;
                this.child = child;
                this.CalculateOffsets();
            }

            public void CalculateOffsets() {
                this.pos = this.parent.InverseTransformPoint(this.child.position);
                this.fw = this.parent.InverseTransformDirection(this.child.forward);
                this.up = this.parent.InverseTransformDirection(this.child.up);
            }

            public void ResetOffsets() {
                this.pos = Vector3.zero;
                this.fw = Vector3.forward;
                this.up = Vector3.up;
            }

            public void Move() {
                Vector3 newpos = this.parent.TransformPoint(this.pos);
                Vector3 newfw = this.parent.TransformDirection(this.fw);
                Vector3 newup = this.parent.TransformDirection(this.up);
                Quaternion newrot = Quaternion.LookRotation(newfw, newup);
                child.position = newpos;
                child.rotation = newrot;
            }

        }
        public class FakeChildTwoParents : FakeChild{
            //public Transform parent, child;
            public Transform otherParentToLookAt;
            public Quaternion otherParentRotationalRelation = Quaternion.identity;
            [Range(0f,1f)]
            public float otherParentInfluence = 0f;
            //public Vector3  pos = Vector3.zero,
            //                fw = Vector3.forward, 
            //                up = Vector3.up;

            /*
            public void CalculateOffsets(Transform parent, Transform child) {
                this.parent = parent;
                this.child = child;
                this.CalculateOffsets();
            }

            public void CalculateOffsets() {
                this.pos = this.parent.InverseTransformPoint(this.child.position);
                this.fw = this.parent.InverseTransformDirection(this.child.forward);
                this.up = this.parent.InverseTransformDirection(this.child.up);
            }
            */

            public void SetOtherParent(Transform otherParent) {
                this.otherParentToLookAt = otherParent;
                this.CalculateOtherParentRotationalRelationship();
            }
            public void SetWeight(float otherParentInfluence) {
                this.otherParentInfluence = otherParentInfluence;
            }

            public void ResetOtherParent() {
                this.otherParentToLookAt = null;
                this.otherParentRotationalRelation = Quaternion.identity;
                base.CalculateOffsets();
            }

            public void CalculateOtherParentRotationalRelationship() {
                if (this.otherParentToLookAt == null) return;
                Quaternion rot = Quaternion.LookRotation(this.otherParentToLookAt.position - parent.position);
                this.otherParentRotationalRelation = Quaternion.Inverse(rot) * parent.rotation;
                this.RotateParentToOtherParent();
                base.CalculateOffsets();
            }

            public void RotateParentToOtherParent() {
                if (this.otherParentToLookAt == null) return;
                Vector3 forward = otherParentToLookAt.position - parent.position;
                Quaternion amountToRotate = Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(forward), otherParentInfluence);
                //parent.rotation = amountToRotate * otherParentRotationalRelation;
                parent.rotation = parent.rotation;
                //parent.rotation = Quaternion.Euler(Vector3.Scale(amountToRotate.eulerAngles, Vector3.ClampMagnitude(new Vector3(1f,1f,1f) - forward,1f)));
            }

            public void ResetOffsets() {
                base.ResetOffsets();
                this.otherParentRotationalRelation = Quaternion.identity;
                this.parent = null;
                this.otherParentToLookAt = null;
            }

            public void Move() {
                this.RotateParentToOtherParent();
                base.Move();
            }
        }
    }
}
