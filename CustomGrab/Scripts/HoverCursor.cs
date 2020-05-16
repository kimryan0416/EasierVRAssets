using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCursor : MonoBehaviour
{
    private Transform m_target;
    public Transform target {
        get {   return m_target;    }
        set {   m_target = value;   }
    }

    [SerializeField]
    private GameObject m_highlightParent;

    // NOT SERIALIZED
    [Tooltip("Stores renderers of all cubes in highlightparent")]
    private Renderer[] m_childrenRenderers;

    // NOT SERIALIZED
    [Tooltip("Refernece to customupdate")]
    private IEnumerator m_customUpdate;

    // NOT SERIALIZED
    [Tooltip("bool for storing if coroutine is still running")]
    private bool m_updateRunning = false;

    public void Init(Transform t) {
        m_customUpdate = CustomUpdate();
        m_childrenRenderers = m_highlightParent.GetComponentsInChildren<Renderer>();
        m_target = t;
        StartCoroutine(m_customUpdate);
        return;
    }
    public void Init(Transform t, Color setColor) {
        m_customUpdate = CustomUpdate();
        m_childrenRenderers = m_highlightParent.GetComponentsInChildren<Renderer>();
        ChangeColor(setColor);
        m_target = t;
        StartCoroutine(m_customUpdate);
        return;
    }

    private IEnumerator CustomUpdate() {
        m_updateRunning = true;
        while (m_target != null) {
            m_highlightParent.transform.localScale = m_target.localScale;
            m_highlightParent.transform.position = m_target.position;
            m_highlightParent.transform.rotation = m_target.rotation;
            yield return null;
        }
        m_updateRunning = false;
        Relieve();
    }

    /*
    private void Update() {
        if ( m_target == null || !m_target.GetComponent<CustomGrabbable>()) {
            m_highlightParent.transform.localScale = Vector3.zero;
            m_highlightParent.transform.position = Vector3.zero;
            m_highlightParent.transform.rotation = Quaternion.identity;
        }
        else {
            m_highlightParent.transform.localScale = m_target.localScale;
            m_highlightParent.transform.position = m_target.position;
            m_highlightParent.transform.rotation = m_target.rotation;
        }
    }
    */

    public void ChangeColor(Color newColor) {
        if (newColor == null) return;
        foreach(Renderer r in m_childrenRenderers) {
            r.material.SetColor("_Color", newColor);
        }
        return;
    }

    public void Relieve() {
        if (m_updateRunning) StopCoroutine(m_customUpdate);
        Destroy(this.gameObject);
    }
}
