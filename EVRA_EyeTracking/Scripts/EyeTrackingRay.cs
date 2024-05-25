using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OVREyeGaze))]
public class EyeTrackingRay : MonoBehaviour
{

    [SerializeField]
    private float   rayDistance = 1.0f,
                    rayWidth = 0.01f;

    [SerializeField]
    private LayerMask layersToInclude;

    [SerializeField]
    private Color   rayColorDefaultState = Color.white,
                    rayColorHoverState = Color.red,
                    rayNoTargetState = Color.red,
                    rayTargetHitState = Color.blue;

    [SerializeField]
    private LineRenderer lr = null;
    private Transform eyeTarget = null;

    private bool _rayHit = false;
    public bool rayHit => _rayHit;
    private Vector3 _rayTargetPosition = Vector3.zero;
    public Vector3 rayTargetPosition => _rayTargetPosition;
    private Vector3 _rayTargetRelPosition = Vector3.zero;
    public Vector3 rayTargetRelPosition => _rayTargetRelPosition;
    private string _rayTargetName = "";
    public string rayTargetName => _rayTargetName;

    [SerializeField]
    private Transform targetReticle = null;
    [SerializeField]
    private float targetReticleSize = 0.025f;
    private Material reticleMaterial = null;

    [SerializeField]
    private bool m_debugMode = false;

    private void Awake() {
        lr = GetComponent<LineRenderer>();
        if (targetReticle.GetComponent<Renderer>() != null) reticleMaterial = targetReticle.GetComponent<Renderer>().materials[0];
        SetupRay();
    }

    private void SetupRay() {
        if (m_debugMode) {
            if (lr == null) {
                lr = gameObject.AddComponent<LineRenderer>();
            }
            lr.enabled = true;
            lr.useWorldSpace = true;
            lr.positionCount = 2;
            lr.startWidth = rayWidth;
            lr.endWidth = rayWidth;
            lr.startColor = rayColorDefaultState;
            lr.endColor = rayColorDefaultState;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, new Vector3(transform.position.x, transform.position.y, transform.position.z + rayDistance));
        } else {
            if (lr != null) lr.enabled = false;
        }
    }

    private void Update() {

        // Get positions
        Vector3 rayOriginPosition = transform.position,
                raycastDirection = transform.TransformDirection(Vector3.forward);
        _rayTargetPosition = transform.position + raycastDirection*rayDistance;
        _rayTargetRelPosition = Vector3.zero;
        _rayTargetName = "";
        _rayHit = false;
        RaycastHit hit;
        Color reticleColor = rayNoTargetState;
        float distanceToTarget = rayDistance;
        if (Physics.Raycast(transform.position, raycastDirection, out hit, 100f, layersToInclude)) {
            lr.startColor = rayColorHoverState;
            lr.endColor = rayColorHoverState;
            reticleColor = rayTargetHitState;
            SetTarget(hit.transform);
            _rayHit = true;
            _rayTargetPosition = hit.point;
            _rayTargetRelPosition = hit.transform.InverseTransformPoint(hit.point);
            _rayTargetName = hit.transform.gameObject.name;
            distanceToTarget = Vector3.Distance(hit.point,transform.position);
        }
        else {
            lr.startColor = rayColorDefaultState;
            lr.endColor = rayColorDefaultState;
            UnsetTarget();
        }

        if (targetReticle != null) {
            targetReticle.position = rayTargetPosition;
            float targetScale = targetReticleSize * distanceToTarget;
            targetReticle.localScale = Vector3.one * targetScale;
            if (reticleMaterial != null) reticleMaterial.SetColor("_Color", reticleColor);
        }
        if (m_debugMode) {
            lr.SetPosition(0, rayOriginPosition);
            lr.SetPosition(1, rayTargetPosition);
        }
    }

    private void UnsetTarget() {
        if (eyeTarget != null && eyeTarget.GetComponent<EyeInteractable>() != null) {
            eyeTarget.GetComponent<EyeInteractable>().IsHovered = false;
        }
        eyeTarget = null;
    }
    private void SetTarget(Transform target) {
        if (eyeTarget != target) UnsetTarget();
        eyeTarget = target;
        if (eyeTarget.GetComponent<EyeInteractable>() != null) {
            eyeTarget.GetComponent<EyeInteractable>().IsHovered = true;
        }
    }
}
