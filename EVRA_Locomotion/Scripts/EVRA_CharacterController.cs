using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using EVRA;
using EVRA.Inputs;
using EVRA.Locomotion;

[System.Serializable]
public class TrackedFootCollider {
    public Transform foot;
    public Vector3 displacement;
    public TrackedFootCollider(Transform foot, Vector3 displacement) {
        this.foot = foot;
        this.displacement = displacement;
    }
}

public class EVRA_CharacterController : MonoBehaviour
{
    [Header("Necessary References")]
    [SerializeField, Tooltip("Reference to the Center Eye Camera Anchor of the player avatar.")]
    private Transform m_centerEyeCamera;
    [SerializeField, Tooltip("Reference to any objects that have colliders acting as the 'feet' of the player.\n\nRecommended that the colliders are under their own children, and that they are nested directly under the 'Tracking Space' object (aka this object).")]
    private List<Transform> m_footColliders;
    [SerializeField, Tooltip("Should your foot colliders follow the center eye camera, or should they not be tracked at all and allowed to move independently?")]
    private bool m_trackFeetUnderCenterEye = true;
    private List<TrackedFootCollider> m_trackedFeet = new List<TrackedFootCollider>();

    [Header("Horizontal Movement")]
    [SerializeField, Range(0f,100f), Tooltip("The max movement speed in m/s. On average, humans jog at 5m/s.")]
    private float m_maxSpeed = 5f;
    public float maxSpeed {
        get { return m_maxSpeed; }
        set { m_maxSpeed = (value > 100f) ? 100f : (value < 0f) ? 0f : value; }
    }
    [SerializeField, Range(0f, 100f), Tooltip("The maximum acceleration of the player in m/s^2.")]
    private float m_maxAcceleration = 10f;
    public float maxAcceleration {
        get { return m_maxAcceleration; }
        set { m_maxAcceleration = (value > 100f) ? 100f : (value < 0f) ? 0f : value; }
    }
    [SerializeField, Range(0f, 100f), Tooltip("In the air, 'maxAirAcceleration` is recommended to be lesser than 'maxAcceleration'.")]
    private float m_maxAirAcceleration = 5f;
    public float maxAirAcceleration {
        get { return m_maxAirAcceleration; }
        set { m_maxAirAcceleration = (value > 100f) ? 100f : (value < 0f) ? 0f : value; }
    }
    [SerializeField, Range(0f, 90f), Tooltip("The maximum horizontal slope (in degrees) that the player can walk up.")]
    private float m_maxGroundAngle = 25f;
    public float maxGroundAngle {
        get { return m_maxGroundAngle; }
        set { m_maxGroundAngle = (value > 90f) ? 90f : (value < 0f) ? 0f : value; }
    }
    [SerializeField, Tooltip("When moving, should it be relative to the forward direction of another object (ex. a Camera)? If so, place that object's Transform here. Then, 'forward' will be relative to the object's forward direction instead of the global forward. Recommended to be the center camera of the user.")]
    private Transform m_forwardDirectionRef;
    public Transform forwardDirectionRef {
        get { return m_forwardDirectionRef; }
        set { }
    }


    [Header("Rotational Movement")]
    [SerializeField, Range(0f, 360f), Tooltip("The amount of rotation every time the rotation is initialized")]
    private float m_rotationAmount = 30f;
    public float rotationAmount {
        get { return m_rotationAmount; }
        set { m_rotationAmount = (value > 360f) ? 360f : (value < 0f) ? 0f : value; }
    }

    [Header("Jump Movement")]
    [SerializeField, Range(0f, 10f), Tooltip("The jump height of the object, in meters. On average, humans jump at a height of 40cm or 0.4m")]
    private float m_jumpHeight = 0.4f;
    public float jumpHeight {
        get { return m_jumpHeight; }
        set { m_jumpHeight = (value > 10f) ? 10f : (value < 0f) ? 0f : value; }
    }
    [SerializeField, Range(0,5), Tooltip("How many mid-air jumps are allowed after the first jump? Granted, nobody can really jump in midair like Crash Bandicoot, but who knows?")]
    private int m_maxAirJumps = 0;
    public int maxAirJumps {
        get { return m_maxAirJumps; }
        set { m_maxAirJumps = (value > 5) ? 5 : (value < 0) ? 0 : value; }
    }

    private Rigidbody rb;

    private Vector3 velocity = Vector3.zero;
    private Vector3 desiredVelocity = Vector3.zero;
    public Vector3 cachedDesiredVelocity;
    private Vector3 desiredRotationAmount = Vector3.zero;

    //private Vector2 playerInput;
    private Vector3 m_playerInput;
    public Vector3 cachedPlayerInput;
    private Vector2 direction;
    public Vector2 cachedDirection;
    private Vector2 playerRotationInput;
    private bool desiredJump;
    private bool onGround;
    private int jumpPhase;

    private float minGroundDotProduct;
    private Vector3 contactNormal;

    /*
    [Header("Child Collider Events")]
    [SerializeField, Tooltip("If you attach a rigidbody to this object, then all colliders nested in children elements will combine into a single collider.\n\nIf you need to track any trigger events in children colliders, list their collider and any methods that should be called.")]
    private List<TriggerProxy> childrenWithTriggerEvents;
    [SerializeField, Tooltip("If you attach a rigidbody to this object, then all colliders nested in children elements will combine into a single collider.\n\nIf you need to track any collision events in children colliders, list their collider and any methods that should be called.")]
    private List<CollisionProxy> childrenWithCollisionEvents;
    */

    void PrepareReferences() {
        // Prepare rigidbody
        if (CommonFunctions.HasComponent<Rigidbody>(this.gameObject)) {
            // We have a rigidbody, we can continue
            rb = GetComponent<Rigidbody>();
        } else {
            // We don't have a rigidbody - so let's create one
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.freezeRotation = true;
        }
    }

    void OnValidate() {
        minGroundDotProduct = Mathf.Cos(m_maxGroundAngle * Mathf.Deg2Rad);
    }

    void FixChildColliders() {
        // Return early if no feet colliders
        //if (!CommonFunctions.HasComponent<Collider>(this.gameObject)) return;
        if (m_footColliders.Count == 0) return;

        // Get list of all children colliders
        Collider[] childrenColliders = GetComponentsInChildren<Collider>();

        // Process each foot collider
        foreach(Transform t in m_footColliders) {
            Vector3 disp = t.position - transform.position;
            m_trackedFeet.Add(new TrackedFootCollider(t, disp));

            Collider c = t.GetComponent<Collider>();
            foreach (Collider col in childrenColliders) {
                // checking if it is our collider, then skip it, 
                if(col != c) {
                    // if it is not our collider then ignore collision between our collider and childs collider
                    Physics.IgnoreCollision(col, c);
                }
            }
        }
    }

    void Awake() {
        PrepareReferences();
        FixChildColliders();
        OnValidate();
    }

    void Update() {
        // Adjust position of foot colliders
        if (m_trackFeetUnderCenterEye) {
            foreach(TrackedFootCollider foot in m_trackedFeet) {
                foot.foot.eulerAngles = new Vector3(foot.foot.eulerAngles.x, m_centerEyeCamera.eulerAngles.y, foot.foot.eulerAngles.z);
                foot.foot.position =  new Vector3(m_centerEyeCamera.position.x, transform.position.y, m_centerEyeCamera.position.z) + (m_centerEyeCamera.rotation * foot.displacement);
            }
        }

        // Get the direction that the object should move
        //Vector2 playerInput = GetHorizontalInput();
        //direction = LocomotionFunctions.InputRelativeToParent(m_playerInput, m_forwardDirectionRef);
        Vector2 forward = new Vector2(m_forwardDirectionRef.forward.x, m_forwardDirectionRef.forward.z).normalized;
        Vector2 right = new Vector2(m_forwardDirectionRef.right.x, m_forwardDirectionRef.right.z).normalized;
        //project forward and right vectors on the horizontal plane (y = 0)
        direction = forward * m_playerInput.y + right * m_playerInput.x;
        //Vector3 direction = LocomotionFunctions.InputRelativeToParent(playerInput, m_forwardDirectionRef);
        direction = Vector2.ClampMagnitude(direction, 1f);
        //direction = Vector3.ClampMagnitude(direction, 1f);

        // Convert that direction to a desired velocity
        desiredVelocity = new Vector3(direction.x, 0f, direction.y); 
        desiredVelocity *= m_maxSpeed;
        //desiredVelocity = direction * m_maxSpeed;

        /*
        
        //rotationAmount = Quaternion.LookRotation(desiredRotationAmount, Vector3.up);
        rotationAmount =  Quaternion.Euler(desiredRotationAmount);
        // Update transform's rotation
        //transform.Rotate(rotationAmount);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationAmount, Time.deltaTime * rotationSpeed);
        */

        // Extrapolate if we should jump or not
        //desiredJump |= GetVerticalInput();

        // Modify rotation
        AdjustRotation();
        
        cachedPlayerInput = m_playerInput;
        cachedDirection = direction;
        cachedDesiredVelocity = desiredVelocity;
        
        m_playerInput = Vector2.zero;
        direction = Vector2.zero;
        //playerInput = Vector3.zero;
        playerRotationInput = Vector2.zero;

        // Later, FixedUpdate() will handle `desiredVelocity` and `desiredJump`
    }

    // Functions to get the current inputs (usually from either keyboard or joystick)
    public void GetMovementInput(Vector2 input) {
        m_playerInput = input;
        //playerInput = new Vector3(input.x, 0f, input.y);
    }
    public void GetMovementInput(InputEventDataPackage package) {
        GetMovementInput(package.inputs[InputType.Thumbstick].response.direction);
    }
    public void GetMovementInput(Vector3 input) {
        m_playerInput = new Vector2(input.x, input.z);
    }
    public void GetRotationInput(Vector2 input) {
        playerRotationInput = input;
    }
    public void GetRotationInput(InputEventDataPackage package) {
        GetRotationInput(package.inputs[InputType.Thumbstick].response.direction);
    }
    public void ResetRotationInput() {
        playerRotationInput = Vector2.zero;
    }
    public void Jump() {
        //return Input.GetButtonDown("Jump");
        desiredJump = true;
    }
    public void Jump(InputEventDataPackage package) {
        Jump();
    }

    // Called later after Update - updates rigidbody velocity
    void FixedUpdate() {
        // Reset the status, including the rb's velocity, what the current jumpPhase is, and what the contact normal is
        UpdateState();
        // Modify velocity based on various factors
        AdjustVelocity();
        // modify velocity again, this time taking into account if the payer wants to jump
        if (desiredJump) {
            desiredJump = false;
            AdjustJump();
        }
        // Update rigidbody's new velocity
        rb.velocity = velocity;

        // Reset state for next update loop
        ClearState();
    }

    void ClearState() {
        // Don't worry - OnCollisionStay() will reset these values back to what they're expected to
        onGround = false;
        contactNormal = Vector3.zero;
    }

    void UpdateState() {
        velocity = rb.velocity;

        if (onGround) {
            jumpPhase = 0;
            contactNormal.Normalize();
        } else {
            contactNormal = Vector3.up;
        }
    }

    void AdjustJump() {
        if (onGround || jumpPhase < m_maxAirJumps) {
            jumpPhase += 1;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * m_jumpHeight);
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0f) {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            velocity += contactNormal * jumpSpeed;
        }
    }

    void OnCollisionEnter(Collision collision) {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision) {
        EvaluateCollision(collision);
    }

    /*
    void OnTriggerEnter(Collider other) {
        // We compare this received collider with any in our children, if any
        if (childrenWithTriggerEvents.Count == 0) return;
        TriggerProxy closest = null;
        float distance = 0f;
        for (int i = 0; i < childrenWithTriggerEvents.Count; i++) {
            TriggerProxy proxy = childrenWithTriggerEvents[i];
            float curDistance = Vector3.Distance(other.transform.position, proxy.sourceCollider.transform.position);
            if (i==0 || distance > curDistance) {
                distance = curDistance;
                closest = proxy;
            }
        }
        if (closest != null) {
            try {
                closest.triggerEnterRefs?.Invoke(other);
            } catch {}
        }
    }
    */

    /*
    void OnTriggerExit(Collider other) {
        if (childrenWithTriggerEvents.Count == 0) return;
        TriggerProxy closest = null;
        float distance = 0f;
        for (int i = 0; i < childrenWithTriggerEvents.Count; i++) {
            TriggerProxy proxy = childrenWithTriggerEvents[i];
            float curDistance = Vector3.Distance(other.transform.position, proxy.sourceCollider.transform.position);
            if (i==0 || distance > curDistance) {
                distance = curDistance;
                closest = proxy;
            }
        }
        if (closest != null) {
            try {
                closest.triggerExitRefs?.Invoke(other);
            } catch {}
        }
    }
    */

    /*
    void OnTriggerStay(Collider other) {
        // We compare this received collider with any in our children, if any
        if (childrenWithTriggerEvents.Count == 0) return;
        TriggerProxy closest = null;
        float distance = 0f;
        for (int i = 0; i < childrenWithTriggerEvents.Count; i++) {
            TriggerProxy proxy = childrenWithTriggerEvents[i];
            float curDistance = Vector3.Distance(other.transform.position, proxy.sourceCollider.transform.position);
            if (i==0 || distance > curDistance) {
                distance = curDistance;
                closest = proxy;
            }
        }
        if (closest != null) {
            try {
                closest.triggerStayRefs?.Invoke(other);
            } catch {}
        }
    }
    */

    void EvaluateCollision(Collision collision) {
        for (int i = 0; i < collision.contactCount; i++) {
			Vector3 normal = collision.GetContact(i).normal;
            //onGround |= normal.y >= minGroundDotProduct;
            if (normal.y >= minGroundDotProduct) {
                onGround = true;
                contactNormal += normal;
            }
		}
    }

    Vector3 ProjectOnContactPlane(Vector3 vector) {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    void AdjustVelocity() {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
        Vector3 yAxis = Vector3.up;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);
        float currentY = Vector3.Dot(velocity, yAxis);

        float acceleration = onGround ? m_maxAcceleration : m_maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);
        float newY = 
            Mathf.MoveTowards(currentY, desiredVelocity.y, maxSpeedChange);

        //velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ) + yAxis * (newY - currentY);
    }

    void AdjustRotation() {
        Vector2 rotation = playerRotationInput;
        rotation = Vector2.ClampMagnitude(rotation, 1f);
        if (Mathf.Abs(rotation.x) > Mathf.Abs(rotation.y)) {
            float angleToAdd = (rotation.x > 0f) ? m_rotationAmount : -m_rotationAmount;
            desiredRotationAmount = new Vector3(0f, angleToAdd, 0f);
            transform.Rotate(desiredRotationAmount, Space.Self);
        }
    }

    public void ToggleGravity(bool to) {
        rb.useGravity = to;
    }
}
