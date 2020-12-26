using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{

    // Distance between the player and the dialogue box, in meters
    [Tooltip("Distance in meters the dialogue box is from the player")]
    public float m_distanceFromPlayer = 1f;
    // Angle below the horizontal plane of the user's eyes
    [Tooltip("Angle below the horizon")]
    public float m_angleBelowHorizon = 30f;

    [SerializeField] [Tooltip("Reference to the prefab for our dialogue box")]
    private GameObject m_CustomDialogueBoxPrefab;

    void Awake() {
        // When we start, we need to rotate the
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
