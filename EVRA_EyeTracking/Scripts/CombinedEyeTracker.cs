using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinedEyeTracker : MonoBehaviour
{

    public List<EyeTrackingRay> eyes = new List<EyeTrackingRay>();
    [SerializeField] private bool _rayHit = false;
    public bool rayHit => _rayHit;
    private Vector3 _rayDir = Vector3.zero;
    private float rayDistance = -1f;
    private Vector3 rawRayDir = Vector3.zero;
    private string _rayTargetName = null;
    public string rayTargetName => _rayTargetName;
    private Vector3 _rayTargetPosition = Vector3.zero;
    public Vector3 rayTargetPosition => _rayTargetPosition;
    public float targetReticleSize = 1f;
    
    public Transform targetReticle = null;

    // Update is called once per frame
    void Update() {
        _rayHit = false;
        _rayDir = Vector3.zero;
        rayDistance = -1f;
        foreach(EyeTrackingRay ray in eyes) {
            _rayHit = _rayHit || ray.rayHit;
            rawRayDir = ray.rayTargetPosition - transform.position;
            _rayDir += rawRayDir.normalized;
            if (rayDistance == -1f || rawRayDir.magnitude < rayDistance) {
                rayDistance = rawRayDir.magnitude;
                _rayTargetName = ray.rayTargetName;
            }
        }
        _rayTargetPosition = transform.position + _rayDir.normalized*rayDistance;
        if (targetReticle != null) {
            float targetScale = targetReticleSize * rayDistance;
            targetReticle.localScale = Vector3.one * targetScale;
            targetReticle.position = _rayTargetPosition;
        }
    }
}
