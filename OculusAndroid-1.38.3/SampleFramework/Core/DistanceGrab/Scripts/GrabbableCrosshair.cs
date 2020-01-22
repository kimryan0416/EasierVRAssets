/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided �AS IS� WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace OculusSampleFramework
{
    public class GrabbableCrosshair : MonoBehaviour
    {
        public enum CrosshairState { Disabled, Enabled, Targeted }

        CrosshairState m_state = CrosshairState.Disabled;
        Transform m_centerEyeAnchor;

        [SerializeField]
        GameObject m_targetedCrosshair;
        [SerializeField]
        GameObject m_enabledCrosshair;
        [SerializeField]
        GameObject m_ActualCrosshair;

        [SerializeField]
        float m_TargetedScale;    // default = 2
        float m_CurrentTargetScale;   // somewhere between m_OriginalScale and m_TargetedScale;
        float m_OriginalScale = 0;    // default = 1;

        Vector3 m_LocalScale;

        [SerializeField]
        float m_TransformScale;

        private void Start()
        {
            m_centerEyeAnchor = GameObject.Find("CenterEyeAnchor").transform;

            m_CurrentTargetScale = m_OriginalScale;
            m_LocalScale = m_ActualCrosshair.transform.localScale;

            StartCoroutine(ChangeCrosshair());
        }

        public void SetState(CrosshairState cs)
        {
            m_state = cs;
            switch(cs) {
                case (CrosshairState.Enabled):
                    //m_targetedCrosshair.SetActive(false);
                    //m_enabledCrosshair.SetActive(true);
                    m_CurrentTargetScale = m_OriginalScale;
                    break;
                case (CrosshairState.Targeted):
                    //m_targetedCrosshair.SetActive(true);
                    //m_enabledCrosshair.SetActive(false);
                    m_CurrentTargetScale = m_TargetedScale;
                    break;
                default:
                    //m_targetedCrosshair.SetActive(false);
                    //m_enabledCrosshair.SetActive(false);
                    m_CurrentTargetScale = m_OriginalScale;
                    break;
            }
            /*
            if (cs == CrosshairState.Disabled)
            {
                m_targetedCrosshair.SetActive(false);
                m_enabledCrosshair.SetActive(false);
            }
            else if (cs == CrosshairState.Enabled)
            {
                m_targetedCrosshair.SetActive(false);
                m_enabledCrosshair.SetActive(true);
            }
            else if (cs == CrosshairState.Targeted)
            {
                m_targetedCrosshair.SetActive(true);
                m_enabledCrosshair.SetActive(false);
            }
            */
        }

        private void Update()
        {
            if (m_state != CrosshairState.Disabled)
            {
                transform.LookAt(m_centerEyeAnchor);
            }
        }

        private IEnumerator ChangeCrosshair() {
            while(true) {
                m_ActualCrosshair.transform.localScale = Vector3.Lerp(m_ActualCrosshair.transform.localScale, m_LocalScale * m_CurrentTargetScale, Time.deltaTime * m_TransformScale);
                yield return null;
            }
        }
    }
}
