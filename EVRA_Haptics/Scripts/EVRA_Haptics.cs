using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_Haptics : MonoBehaviour
{
    [SerializeField] [Tooltip("Reference to the parent EVRA_Pointer")]
    private EVRA_Hand m_Hand;

    [Tooltip("List of objects with colliders in range.")]
    public List<Collider> inRange = new List<Collider>();

    [Tooltip("The frequency of the vibration")]
    public float frequency = 0f;
    
    [Tooltip("The amplitude of the vibration")]
    public float amplitude = 0f;

    public AnimationCurve frequencyCurve;
    public AnimationCurve amplitudeCurve;

    [Tooltip("Radius of object detection")]
    public float radius = 0.3f;

    public enum HapticsPreference {
        Closest,
        Average_Distance,
        Constant
    }
    [SerializeField] [Tooltip("Preference for how the haptics should be dolled out")]
    private HapticsPreference m_StrengthOfHapticsBy = HapticsPreference.Closest;

    [Tooltip("Colliders to ignore from haptic detection")]
    public List<Collider> ignoreColliders = new List<Collider>();

    private void Start() {
        SetHaptics(frequency, amplitude);
        /*
        if (m_triggers.Count > 0) {
            foreach(EVRA_HapticTrigger t in m_triggers) {
                t.Init(this);
            }
        }
        */
    }

    private void Update() {
        FindSurroundingObjects();
        if (inRange.Count == 0) {
            SetHaptics();
            return;
        }
        switch(m_StrengthOfHapticsBy) {
            case (HapticsPreference.Closest):
                HapticsByClosest();
                break;
            case (HapticsPreference.Average_Distance):
                HapticsByAverageDistance();
                break;
            case (HapticsPreference.Constant):
                HapticsByConstant();
                break;
            default:
                HapticsByClosest();
                break;
        }
    }

    public void FindSurroundingObjects() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        inRange = new List<Collider>(hitColliders);
        if (ignoreColliders.Count > 0) {
            foreach(Collider ig in ignoreColliders) inRange.Remove(ig);
        }
    }

    private void HapticsByClosest() {
        Transform closest = inRange[0].transform;
        float distance = Vector3.Distance(m_Hand.transform.position, closest.position);
        Transform temp;
        float tempDistance;
        RaycastHit hit;
        for(int i = 1; i < inRange.Count; i++) {
            temp = inRange[i].transform;
            tempDistance = Vector3.Distance(m_Hand.transform.position, temp.position);
            if (Physics.Raycast(transform.position, temp.position.normalized, out hit, Mathf.Infinity)) {
                tempDistance = Vector3.Distance(m_Hand.transform.position, hit.point);
            }
            if (tempDistance < distance) {
                distance = tempDistance;
                closest = temp;
            }
        }
        SetHaptics(frequencyCurve.Evaluate(distance), amplitudeCurve.Evaluate(distance));
    }
    private void HapticsByAverageDistance() {
        float totalDistance = 0f;
        Transform temp;
        float tempDistance;
        RaycastHit hit;
        for(int i = 0; i < inRange.Count; i++) {
            temp = inRange[i].transform;
            tempDistance = Vector3.Distance(m_Hand.transform.position, temp.position);
            if (Physics.Raycast(transform.position, temp.position.normalized, out hit, Mathf.Infinity)) {
                tempDistance = Vector3.Distance(m_Hand.transform.position, hit.point);
            }
            totalDistance += tempDistance;
        }
        float avgDistance = totalDistance / inRange.Count;
        SetHaptics(frequencyCurve.Evaluate(avgDistance), amplitudeCurve.Evaluate(avgDistance));
    }
    private void HapticsByConstant() {
        if (inRange.Count > 0) SetHaptics(frequencyCurve.Evaluate(0f), amplitudeCurve.Evaluate(0f));
        else SetHaptics();
    }

    public void SetHaptics(float freq = 0f, float amp = 0f) {
        OVRInput.SetControllerVibration(freq, amp, m_Hand.OVRController);
    }

    public IEnumerator SetHapticOverDuration(float freq, float amp, float duration) {
        OVRInput.SetControllerVibration(freq, amp, m_Hand.OVRController);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, m_Hand.OVRController);
    }
}
