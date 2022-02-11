using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HandPropulsion : MonoBehaviour
{
    public OVRPlayerController playerController;
    public CharacterController characterController;
    public EVRA_Hand hand;

    public TextMeshProUGUI textboxDebug;

    // Update is called once per frame
    void Update()
    {   
        textboxDebug.text = hand.velocity.ToString() + " | " + (Mathf.Round(hand.velocity.magnitude * 100f) / 100f).ToString();
        if (hand.velocity.y < 0 && hand.velocity.y > hand.velocity.x && hand.velocity.y > hand.velocity.x && hand.velocity.magnitude > 2f) {
            playerController.Jump();
        }
        //textboxDebug.text = playerController.moveDirection.ToString() + " | " + playerController.MoveThrottle.ToString();
        //Debug.Log(playerController.moveDirection.ToString());
    }
}
