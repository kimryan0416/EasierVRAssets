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
    public float radius = 0.2f;

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
            frequency = 0f;
            amplitude = 0f;
            SetHaptics(frequency, amplitude);
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
        //Transform closest = inRange[0].transform;
        float distance = Vector3.Distance(m_Hand.transform.position, inRange[0].transform.position);
        //Transform temp;
        Collider temp;
        float tempDistance;
        Vector3 closestPointInBounds;
        RaycastHit hit;
        for(int i = 1; i < inRange.Count; i++) {
            /*
            temp = inRange[i].transform;
            tempDistance = Vector3.Distance(m_Hand.transform.position, temp.position);
            if (Physics.Raycast(transform.position, temp.position.normalized, out hit, Mathf.Infinity)) {
                tempDistance = Vector3.Distance(m_Hand.transform.position, hit.point);
            }
            */
            temp = inRange[i];
            closestPointInBounds = temp.ClosestPointOnBounds(m_Hand.transform.position);
            tempDistance = Vector3.Distance(m_Hand.transform.position, closestPointInBounds);
            if (tempDistance < distance) {
                distance = tempDistance;
                //closest = temp;
            }
        }
        frequency = frequencyCurve.Evaluate(distance);
        amplitude = amplitudeCurve.Evaluate(distance);
        SetHaptics(frequency, amplitude);
    }
    private void HapticsByAverageDistance() {
        float totalDistance = 0f;
        //Transform temp;
        Collider temp;
        float tempDistance;
        Vector3 closestPointInBounds;
        RaycastHit hit;
        for(int i = 0; i < inRange.Count; i++) {
            /*
            temp = inRange[i].transform;
            tempDistance = Vector3.Distance(m_Hand.transform.position, temp.position);
            if (Physics.Raycast(transform.position, temp.position.normalized, out hit, Mathf.Infinity)) {
                tempDistance = Vector3.Distance(m_Hand.transform.position, hit.point);
            }
            */
            temp = inRange[i];
            closestPointInBounds = temp.ClosestPointOnBounds(m_Hand.transform.position);
            tempDistance = Vector3.Distance(m_Hand.transform.position, closestPointInBounds);
            totalDistance += tempDistance;
        }
        float avgDistance = totalDistance / inRange.Count;
        frequency = frequencyCurve.Evaluate(avgDistance);
        amplitude = amplitudeCurve.Evaluate(avgDistance);
        SetHaptics(frequency, amplitude);
    }
    private void HapticsByConstant() {
        if (inRange.Count > 0) {
            frequency = frequencyCurve.Evaluate(0f);
            amplitude =  amplitudeCurve.Evaluate(0f);
        } else {
            frequency = 0f;
            amplitude = 0f;
        }
        SetHaptics(frequency, amplitude);
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
