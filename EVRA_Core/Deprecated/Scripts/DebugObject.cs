using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugObject : MonoBehaviour
{

    [System.Serializable]
    public class ButtonMap {
        public OVRInput.Controller OVRController;
        public OVRInput.Button buttonToggle;
        public List<GameObject> objects = new List<GameObject>();
        public void hideObjects(OVRInput.Controller c) {
            if (c != OVRController) return;
            foreach(GameObject o in objects) {
                o.SetActive(false);
            }
        }
        public void showObjects(OVRInput.Controller c) {
            if (c != OVRController) return;
            foreach(GameObject o in objects) {
                o.SetActive(true);
            }
        }
    }

    public List<ButtonMap> m_maps = new List<ButtonMap>();

    private void Start() {
        foreach(ButtonMap m in m_maps) {
            switch(m.buttonToggle) {
                case(OVRInput.Button.PrimaryIndexTrigger):
                    if (m.OVRController == OVRInput.Controller.LTouch) {
                        CustomEvents.current.onLeftTriggerDown += m.hideObjects;
                        CustomEvents.current.onLeftTriggerUp += m.showObjects;
                    }
                    else {
                        CustomEvents.current.onRightTriggerDown += m.hideObjects;
                        CustomEvents.current.onRightTriggerUp += m.showObjects;
                    }
                    break;
            }
        }
    }

    
}
