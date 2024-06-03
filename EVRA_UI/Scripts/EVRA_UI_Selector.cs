using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EVRA.Inputs;

public class EVRA_UI_Selector : MonoBehaviour
{

    /// <summary>
    /// This component has a very simple job. Its role is to ensure:
    /// 1. The selector is always oriented so that its z-forward matches its parent gameobject
    /// 2. The selector maintains within the area defined by its parent UI (toggleable)
    /// 3. The selector has the capacity to move depending on either public function call or by some external entity.
    /// </summary>

    public EVRA_UI ui_parent;
    public float translation_speed = 0.1f;

    // Update is called once per frame
    void LateUpdate() {
        // Double-check its position so that it lays flat on the XY plane definedby its parent.
        Update2DPosition();
    }

    public void TranslateXY(InputEventDataPackage package) {
        // We get the joystick direction (which is a 2D vector)
        Vector2 dir = package.inputs[ InputType.Thumbstick ].response.direction;
        // Don't move if the magnitude of the dir doesn't exceed a threshold
        if (dir.magnitude < 0.45f) return;
        // Based on the direction, we move in accordance to the translation speed
        float newX = transform.localPosition.x + dir.x * translation_speed;
        float newY = transform.localPosition.y + dir.y * translation_speed;
        transform.localPosition = new Vector3(newX, newY, 0f);
    }

    public void Update2DPosition() {
        if (ui_parent == null) return;
        transform.localPosition = new Vector3(
            Mathf.Clamp(transform.localPosition.x, -ui_parent.ui_area.x*0.5f, ui_parent.ui_area.x*0.5f), 
            Mathf.Clamp(transform.localPosition.y, -ui_parent.ui_area.y*0.5f, ui_parent.ui_area.y*0.5f), 
            0f
        );
    }
}
