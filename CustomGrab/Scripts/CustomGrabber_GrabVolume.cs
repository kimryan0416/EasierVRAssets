using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomGrabber_GrabVolume : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The world-space radius where detection should occur")]
    private float m_collisionRadius = 0.5f;
    public float collisionRadius {
        get {   return m_collisionRadius;   }
        set {   m_collisionRadius = value;  }
    }

    // NOT SERIALIZED
    [Tooltip("Reference to all objects that are in range")]
    private List<Transform> m_inRange = new List<Transform>();
    public List<Transform> inRange {
        get {   return m_inRange;   }
    }
    // Get the closest in range
    public Transform closestInRange {
        get {   return (m_inRange.Count > 0) ? m_inRange[0] : null; }
    }

    /*
    [SerializeField]
    [Tooltip("GameObjects to exclude from the detection process")]
    private List<int> m_layersExcludedFromDetection = new List<int>();
    */

    // NOT SERIALIZED
    [Tooltip("Stores all instantiated hovers")]
    private Dictionary<int, HoverCursor> m_hovers = new Dictionary<int, HoverCursor>();

    [SerializeField]
    [Tooltip("Color of hover cursor, if hover cursor is added - otherwise, just changes itself's color")]
    private Color m_hoverColor = Color.yellow;
    // NOT SERIALIZED
    [Tooltip("Original color material of original material - used only if the hover prefab is not set")]
    private Color m_originalColor;

    [SerializeField]
    [Tooltip("The transform where the collision should occur")]
    private Transform m_collisionOrigin;

    [SerializeField]
    [Tooltip("Reference to hover prefab")]
    private HoverCursor m_hoverCursorPrefab;

    // NOT SERIALIZED
    [Tooltip("Reference to CustomUpdate coroutine")]
    private IEnumerator m_customUpdate;
    // NOT SERIALIZED
    [Tooltip("Check if CustomUpdate is still running")]
    private bool m_updateRunning = false;

    [SerializeField]
    [Tooltip("Boolean to check if the custom update should activate upon starting the game")]
    private bool m_shouldStartOnRun = true;
    public bool shouldStartOnRun {
        get {   return m_shouldStartOnRun;  }
    }
    [SerializeField]
    [Tooltip("Boolean to check if itself and its children can collide with other objects")]
    private bool m_canCollide = false;
    public bool canCollide {
        get {   return m_canCollide;      }
        set {   ToggleCollision(value);   }
    }

    public void Start() {
        if (m_shouldStartOnRun) Init();
    }

    public void Init(bool canStart = false) {
        if (m_collisionOrigin == null) m_collisionOrigin = this.transform;
        m_customUpdate = CustomUpdate();
        ToggleCollision(m_canCollide);
        m_originalColor = m_collisionOrigin.GetComponent<Renderer>().material.GetColor("_Color");
        if (m_shouldStartOnRun || canStart) Activate();
    }

    /*
    public List<GameObject> GetInRange<T>(Transform origin, float rad = 1f) {
        if (origin == null) return null;
        Collider[] hitColliders = Physics.OverlapSphere(origin.position, rad);
        foreach (Collider c in hitColliders) {
            if (c.GetComponent<T>() != null) {
                inRange.Add(c.gameObject);
            }
        }
        List<GameObject> inRange = inRange.OrderBy(x => Vector3.Distance(origin.position,x.transform.position)).ToList();
        return inRange;
    }
    */

    public IEnumerator CustomUpdate() {
        // update boolean to tell system that this coroutine is running
        m_updateRunning = true;
        // Execute while loop
        while(true) {
            // get all gameobjects in range
            // m_inRange = CommonFunctions.GetInRange<Transform, CustomGrabbable, HoverCursor>(m_collisionOrigin, m_collisionRadius, LayerMask.NameToLayer("AvoidHover"));
            m_inRange = CommonFunctions.GetInRange<Transform, CustomGrabbable, HoverCursor>(m_collisionOrigin, m_collisionRadius);
            if (m_hoverCursorPrefab == null) {
                m_collisionOrigin.GetComponent<Renderer>().material.color = (m_inRange.Count > 0) ? m_hoverColor : m_originalColor;
            } else {
                // Get a list of all instance ID's to check for 
                List<int> idsToCheck = m_hovers.Keys.ToList(); 
                // Foreach object inside what was found in range, we need to check if 1) any new hovers were detected, and 2) if any match existing copies
                foreach(Transform g in m_inRange) {
                    if (!m_hovers.ContainsKey(g.gameObject.GetInstanceID())) {
                        // a new gameobject has entered the scene... add it
                        HoverCursor newHover = Instantiate(m_hoverCursorPrefab, Vector3.zero, Quaternion.identity) as HoverCursor;
                        newHover.Init(g, m_hoverColor);
                        m_hovers.Add(g.gameObject.GetInstanceID(), newHover);
                    } else {
                        idsToCheck.Remove(g.gameObject.GetInstanceID());
                    }
                }
                // For any id's that we did not find, we relieve them from their duties.
                while(idsToCheck.Count > 0) {
                    m_hovers[idsToCheck[0]].Relieve();
                    m_hovers.Remove(idsToCheck[0]);
                    idsToCheck.RemoveAt(0);
                }
            }
            yield return null;
        }
    }

    public void Activate() {
        if (!m_updateRunning) StartCoroutine(m_customUpdate);
        return;
    }
    public void Deactivate() {
        if (m_updateRunning) {
            StopCoroutine(m_customUpdate);
            m_updateRunning = false;
        }
        return;
    }

    public void ToggleCollision(bool toggle = false) {
        m_canCollide = toggle;
        Collider[] cs = this.GetComponentsInChildren<Collider>() as Collider[];
        foreach(Collider c in cs) {
            c.enabled = m_canCollide;
        }
    }

    /*
    public void AddToExclusionList(string layer) {
        int layerID = LayerMask.NameToLayer(layer);
        if (!m_layersExcludedFromDetection.Contains(layerID)) {
            m_layersExcludedFromDetection.Add(layerID);
        }
        return;
    }

    public void RemoveFromExclusionList(string layer) {
        int layerID = LayerMask.NameToLayer(layer);
        if (m_layersExcludedFromDetection.Contains(layerID)) {
            m_layersExcludedFromDetection.Remove(layerID);
        }
        return;
    }
    */
}
