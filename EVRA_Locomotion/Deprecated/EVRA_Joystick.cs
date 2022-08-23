using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_Joystick : MonoBehaviour
{
    public CharacterController m_CharacterController;

    public float accelerationXZ = 0f;
    public float gravity = -9.8f;
    public float jumpForce = 1f;

    public float friction = 0.01f;
    public float maxSpeed = 1.4f;
    public Vector3 velocity = Vector3.zero;
    public float mass = 62f;

    public float maxFallSpeed = 0.1f;

    private Vector3 inputXZ = new Vector3(0f, -0f, 0f);
    private float inputY = 0f;
    private float xvelocity = 0f, zvelocity = 0f, yvelocity = 0f;

    private void Update() {
        // Get input
        inputXZ = new Vector3(Input.GetAxis("Horizontal"), -0f, Input.GetAxis("Vertical"));
        // Make sure input is applied to the forward direction of the characterController, not the forward direction of the World
        inputXZ = transform.TransformDirection(inputXZ);
        inputXZ = Vector3.ClampMagnitude(inputXZ, 1f); 
        inputY = (Input.GetKeyDown(KeyCode.Z) && m_CharacterController.isGrounded) ? 1f : 0f;
        
    }

    private void FixedUpdate() {
        accelerationXZ = inputXZ.magnitude / mass;
        // clamp input to max 1, so you can't have two buttons pressed like W and A for more speed diagonally
 
        velocity.x += inputXZ.x * accelerationXZ; // velocity buildup on the horizontal axis
        velocity.z += inputXZ.z * accelerationXZ; // velocity buildup on the vertical axis

        // if there's no input on the horizontal axis
        if (inputXZ.x == 0) {
            velocity.x = Mathf.SmoothDamp(velocity.x, 0f, ref xvelocity, friction); 
            // smoothdamp to zero
        }
        // if there's no input on the vertical axis
        if (inputXZ.z == 0) {
            velocity.z = Mathf.SmoothDamp(velocity.z, 0f, ref zvelocity, friction); 
            // smoothdamp to zero
        }
        velocity = Vector3.ClampMagnitude (velocity, maxSpeed); 
        // clamp the speed so the character doesn't accelerate forever

        if (inputY > 0f) {
            yvelocity += jumpForce;
            Debug.Log("JUMPING " + yvelocity.ToString());

        }

        if (!m_CharacterController.isGrounded) {
            float downwardVelocity = gravity * Time.deltaTime;
            downwardVelocity = Mathf.Clamp(downwardVelocity, -1f*Mathf.Abs(maxFallSpeed), 0f);
            yvelocity += downwardVelocity;
            // calculate downward speed based on gravity
        }
        velocity.y = yvelocity;


        Debug.Log("EVRA_Joystick: " + velocity.ToString());
 
        m_CharacterController.Move(velocity);
        //Debug.Log("EVRA-Movement: " + velocity.ToString());
    }


    /*
    public CharacterController m_CharacterController;
    private float moveStrength = 1f;
    private Vector3 moveThrottle = Vector3.zero;
    public float dampRate = 0.2f;

    private void Update() {
        bool moveForward = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
		bool moveLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
		bool moveRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
		bool moveBack = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        
        Quaternion ort = m_CharacterController.transform.rotation;
		Vector3 ortEuler = ort.eulerAngles;
		ortEuler.z = ortEuler.x = 0f;
		ort = Quaternion.Euler(ortEuler);

        moveStrength = 1f;
        if ((moveForward && moveLeft) || (moveForward && moveRight) || (moveBack && moveLeft) || (moveBack && moveRight)) {
			moveStrength = 0.70710678f;
        }
        
        if (moveForward) moveThrottle += ort * (transform.lossyScale.z * moveStrength * Vector3.forward);
		if (moveBack) moveThrottle += ort * (transform.lossyScale.z * moveStrength * Vector3.back);
		if (moveLeft) moveThrottle += ort * (transform.lossyScale.x * moveStrength * Vector3.left);
		if (moveRight) moveThrottle += ort * (transform.lossyScale.x * moveStrength * Vector3.right);

        float motorDamp = (1.0f + (dampRate * 60f * Time.deltaTime));
        float newY = (m_CharacterController.isGrounded) ? moveThrottle.y : moveThrottle.y / motorDamp;
        moveThrottle = new Vector3(moveThrottle.x/motorDamp, newY, moveThrottle.z/motorDamp);
        Debug.Log("Dampened MoveThrottle: " + moveThrottle.ToString());

        Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Debug.Log("EVRA_JOYSTICK: " + primaryAxis.ToString());
    }
    */


}
