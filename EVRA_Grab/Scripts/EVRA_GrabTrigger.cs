using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_GrabTrigger : MonoBehaviour
{
    #region Grabbable Refs
        [Tooltip("To which custom grabbable script am I going to refer to?")] // NO SERIALIZATION
        private EVRA_Grabbable m_GrabbableRef;
        public EVRA_Grabbable GrabbableRef {
            get {   return m_GrabbableRef;  }
            set {}
        }
    #endregion

    // All we need to do is ensure that, when this triggers the grabber, we can tell it that "You gotta look at THIS guy".
    public void Init(EVRA_Grabbable parent) {
        m_GrabbableRef = parent;
    }
}
