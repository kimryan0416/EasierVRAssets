using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class CustomPointer : MonoBehaviour
{

    private LineRenderer m_LineRenderer;
    private IEnumerator m_CustomUpdate;
    private GameObject m_raycastTarget = null;
    public GameObject raycastTarget {
        get {   return m_raycastTarget;     }
        set {   m_raycastTarget = value;    }
    }
    [SerializeField]
    private Transform m_raycastOrigin;
    public Transform raycastOrigin {
        get {   return m_raycastOrigin;     }
        set {   m_raycastOrigin = value;    }
    }
    [SerializeField]
    private float m_raycastDistance = 100f;
    public float raycastDistance {
        get {   return m_raycastDistance;   }
        set {   m_raycastDistance = value;  }
    }
    private Vector3[] linePoints = new Vector3[2];
    [SerializeField]
    private Color m_lineColor = new Color32(255, 255, 0, 255);
    public Color lineColor {
        get {   return m_lineColor;     }
        set {   m_lineColor = value;    }
    }

    [SerializeField]
    private bool m_debugMode = false;
    public bool debugMode {
        get {   return m_debugMode;     }
        set {   m_debugMode = value;    }
    }

    [SerializeField]
    private bool m_isGraphicRaycaster = false;
    public bool isGraphicRaycaster {
        get {   return m_isGraphicRaycaster;    }
        set {   m_isGraphicRaycaster = value;   }
    }

    [SerializeField]
    private GameObject m_XYZ;

    private void Awake()
    {
        m_LineRenderer = this.GetComponent<LineRenderer>();
        m_CustomUpdate = CustomUpdate();
        if (m_raycastOrigin == null) {  m_raycastOrigin = this.transform;   }
        linePoints[0] = Vector3.zero;
        linePoints[1] = Vector3.zero;
    }

    private IEnumerator CustomUpdate() {
        while(true) {
            // (if in debug mode, turn on XYZ)
            m_XYZ.SetActive(m_debugMode);
            // Update start position
            linePoints[0] = m_raycastOrigin.position;
            linePoints[1] = CheckRaycast();
            if (m_LineRenderer.enabled) {   
                m_LineRenderer.material.SetColor("_Color", m_lineColor);
                m_LineRenderer.SetPositions(linePoints); 
            }
            yield return null;
        }
    }
    private Vector3 CheckRaycast() {
        RaycastHit rayHit;
        Vector3 returnPoint = m_raycastOrigin.position + m_raycastOrigin.forward * m_raycastDistance;
        if (Physics.Raycast(m_raycastOrigin.position, m_raycastOrigin.TransformDirection(Vector3.forward), out rayHit, m_raycastDistance)) {
            // Something in front of it
            m_raycastTarget = rayHit.transform.gameObject;
            returnPoint = rayHit.point;
        } else {
            m_raycastTarget = null;
        }
        return returnPoint;
    }
    
    public void Activate() {
        StartCoroutine(m_CustomUpdate);
    }
    public void Deactivate() {
        StartCoroutine(m_CustomUpdate);
    }
    public void LineSet(bool s) {
        m_LineRenderer.enabled = (!m_debugMode) ? s : true;
    }
    public void LineToggle() {
        m_LineRenderer.enabled = (!m_debugMode) ? !m_LineRenderer.enabled : true;
    }
    public void LineOff() {
        m_LineRenderer.enabled = (!m_debugMode) ? false : true;
    }
    public void LineOn() {
        m_LineRenderer.enabled = true;
    }
}
