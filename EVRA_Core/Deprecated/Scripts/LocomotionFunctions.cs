using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocomotionFunctions : MonoBehaviour {
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