using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* ------------
- The purpose of this component is to control a pointer (aka a Line Renderer) that is used for various functions such as pointing, pinging, and locomotion.
- In virtual reality, pointing is a common tool utilized in various user interfaces because it affords for (semi-)accurate selection and targeting. 
- In our custom package, we need a pointer for various activities, most notably distance grabbing and locomotion. This script interacts with these other functions and any others that may come in the future.
------------ */
[RequireComponent(typeof(LineRenderer))] // We require a Line Renderer to be attached to this GameObject - the Line Renderer renders the pointer.
public class EVRA_Pointer: MonoBehaviour
{   
    [Tooltip("Reference to LineRenderer")] // NOT SERIALIZED
    private LineRenderer m_LR;
    public LineRenderer LR {
        get {   return m_LR;  }
        set {}
    }

    public enum LineType {
        Straight,
        BezierCurve,
    }
    [SerializeField] [Tooltip("The type of line to be used")]
    private LineType m_lineType = LineType.Straight;

    [SerializeField] [Tooltip("How far (in meters) should the line reach out?")]
    private float m_LineDistance = 10f;

    [SerializeField] [Tooltip("How many points on the line? The more points, the smoother the line")]
    private int numPositions = 2;

    [Tooltip("The current Transform target of the raycast")]
    private Transform m_forwardRaycastTarget = null;
    public Transform forwardRaycastTarget {
        get {   return m_forwardRaycastTarget; }
        set {}
    }
    [Tooltip("The current position of the raycast hit itself")]
    private Vector3 m_forwardRaycastHitPosition = Vector3.zero;
    public Vector3 forwardRaycastHitPosition {
        get {   return m_forwardRaycastHitPosition;    }
        set {}
    }
    [Tooltip("The current Transform target of the raycast")]
    private Transform m_downwardRaycastTarget = null;
    public Transform downwardRaycastTarget {
        get {   return m_downwardRaycastTarget; }
        set {}
    }
    [Tooltip("The current position of the raycast hit itself")]
    private Vector3 m_downwardRaycastHitPosition = Vector3.zero;
    public Vector3 downwardRaycastHitPosition {
        get {   return m_downwardRaycastHitPosition;    }
        set {}
    }
    // The raycast target, which would depend on whether the line is straight or bezier curved
    public Transform raycastTarget {
        get {   return (m_lineType == LineType.BezierCurve) ? m_downwardRaycastTarget : m_forwardRaycastTarget; }
        set {}
    }
    // The raycast hit position, which would depend on whether the line is straight or bezier curved
    public Vector3 raycastHitPosition {
        get {   return (m_lineType == LineType.BezierCurve) ? m_downwardRaycastHitPosition : m_forwardRaycastHitPosition; }
        set {}
    }

    
    [Tooltip("TRUE = the line will appear regardless if a target is hit; FALSE = the line will ONLY appear if a target is hit")]
    public bool m_alwaysShow = true;

    [Tooltip("Color of the default line")]
    public Color defaultColor = Color.yellow;

    [Tooltip("Color of the line when hitting a target")]
    public Color hitColor = Color.blue;

    public enum CollisionType {
        All,
        Only_EasierVRAssets,
        All_Except_EasierVRAssets
    }
    [SerializeField] [Tooltip("Should the pointer detect only EasierVRAssets objects?")]
    private CollisionType m_collisionType = CollisionType.Only_EasierVRAssets;

    [Tooltip("The layer mask that should be used for the raycasts")] // NOT SERIALIZED
    private int m_layerMask;

    [Tooltip("Unique boolean that overrides the usual `Activate()` and `Deactivate()` functions. If this boolean is set to false, then even if `Activate()` is called the line won't render")]
    private bool m_trulyOn = true;

    private void Awake() {
        if (!gameObject.GetComponent<LineRenderer>()) {
            m_LR = gameObject.AddComponent<LineRenderer>();
            m_LR.SetWidth(0.02f, 0.02f);
        } else {
            m_LR = gameObject.GetComponent<LineRenderer>();
        }

        switch(m_collisionType) {
            case (CollisionType.Only_EasierVRAssets):
                if (LayerMask.NameToLayer("EasierVRAssets") != -1) m_layerMask = 1 << LayerMask.NameToLayer("EasierVRAssets");
                else m_layerMask = -1;
                break;
            case (CollisionType.All_Except_EasierVRAssets):
                if (LayerMask.NameToLayer("EasierVRAssets") != -1) {
                    m_layerMask = 1 << LayerMask.NameToLayer("EasierVRAssets");
                    m_layerMask = ~m_layerMask;
                } else m_layerMask = -1;
                break;
            default:
                m_layerMask = -1;
                break;
        }

        m_LR.useWorldSpace = true;
        m_LR.receiveShadows = false;
        if (numPositions < 2) numPositions = 2;
    }

    private void Update() {
        if (!m_trulyOn || !m_LR.enabled) return; // End early if the line isn't even enabled
        Vector3 forwardPosition = FindForwardRaycast();
        Vector3 bottomPosition = FindBottomRaycast(forwardPosition);
        List<Vector3> positions = new List<Vector3>();
        
        m_LR.positionCount = numPositions + 1;
        if (m_lineType == LineType.BezierCurve) positions = BezierCurves.DetermineQuadraticCurve(numPositions, transform.position, forwardPosition, bottomPosition);
        else positions = BezierCurves.DetermineLinearCurve(numPositions, transform.position, forwardPosition);
        m_LR.SetPositions(positions.ToArray());

        if (m_LR.materials.Length == 0) return;

        Color m = defaultColor;
        if (m_lineType == LineType.BezierCurve) {
            if (m_downwardRaycastTarget != null) m = hitColor;
        } else {
            if (m_forwardRaycastTarget != null) m = hitColor;
        }
        m_LR.materials[0].SetColor("_Color",m);
    }

    private Vector3 FindForwardRaycast() {
        RaycastHit hit;
        Vector3 target = (m_alwaysShow) ? transform.position + (transform.TransformDirection(Vector3.forward) * m_LineDistance) : transform.position;
        bool raycastResult = false;
        if ((m_collisionType == CollisionType.All_Except_EasierVRAssets || m_collisionType == CollisionType.Only_EasierVRAssets) && m_layerMask != -1) {
            raycastResult = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, m_LineDistance, m_layerMask);
        } else {
            raycastResult = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, m_LineDistance);
        }
        if (raycastResult) {
            target = hit.point;
            m_forwardRaycastTarget = hit.collider.transform;
            m_forwardRaycastHitPosition = hit.point;
            /*
            if (m_lineType == LineType.Straight) {
                m_raycastTarget = hit.collider.transform;
                m_raycastHitPosition = hit.point;
            }
            */
        } else {
            m_forwardRaycastTarget = null;
            m_forwardRaycastHitPosition = Vector3.zero;
            /*
            if (m_lineType == LineType.Straight) {
                m_raycastTarget = null;
                m_raycastHitPosition = Vector3.zero;
            }
            */
        }
        return target;
    }

    private Vector3 FindBottomRaycast(Vector3 initial) {
        RaycastHit hit;
        Vector3 target = initial;
        bool raycastResult = false;
        if ((m_collisionType == CollisionType.All_Except_EasierVRAssets || m_collisionType == CollisionType.Only_EasierVRAssets) && m_layerMask != -1) {
            raycastResult = Physics.Raycast(new Vector3(initial.x,initial.y+0.01f,initial.z), -Vector3.up, out hit, Mathf.Infinity, m_layerMask);
        } else {
            raycastResult = Physics.Raycast(new Vector3(initial.x,initial.y+0.01f,initial.z), -Vector3.up, out hit, Mathf.Infinity);
        }
        if (raycastResult) {
            target = hit.point;
            m_downwardRaycastTarget = hit.collider.transform;
            m_downwardRaycastHitPosition = hit.point;
            /*
            if (m_lineType == LineType.BezierCurve) {
                m_raycastTarget = hit.collider.transform;
                m_raycastHitPosition = hit.point;
            }
            */
        } else {
            m_downwardRaycastTarget = null;
            m_downwardRaycastHitPosition = Vector3.zero;
            /*
            if (m_lineType == LineType.BezierCurve) {
                m_raycastTarget = null;
                m_raycastHitPosition = Vector3.zero;
            }
            */
        }
        return target;
    }

    public void Activate() {
        if (m_trulyOn) m_LR.enabled = true;
    }
    public void Deactivate() {
        m_LR.enabled = false;
    }

    public void TrulyDeactivate() {
        m_trulyOn = false;
        m_LR.positionCount = 2;
        Vector3[] tempPositions = new Vector3[2];
        tempPositions[0] = Vector3.zero;
        tempPositions[1] = Vector3.zero;
        m_LR.SetPositions(tempPositions);
    }
    public void TrulyActivate() {
        m_trulyOn = true;
    }

    /*
    #region Variables and References
        /// Should this script be initialized upon scene load? Or should it wait until it's initialized by another class?
        [SerializeField] [Tooltip("Should the pointer start from the getgo, or should it wait for an Init call?")]
        private bool m_shouldStart = true;

    /// Reference to our Custom Grabber, which is referenced when doing checks and balances in the script
    [SerializeField] [Tooltip("Reference to grabber, if this requires a grabber to work")]
    private CustomGrabber m_grabber = null;

    /// A storage reference to the Line Renderer component that is attached to this GameObject
    [Tooltip("Reference to LineRenderer")] // NOT SERIALIZED
    private LineRenderer m_LineRenderer;
    public LineRenderer lineRenderer {
        get {   return m_LineRenderer;  }
        set {   m_LineRenderer = value; }
    }

    private enum pointerDirection {
        XForward,
        XBackward,
        YForward,
        YBackward,
        ZForward,
        ZBackward
    }
    [SerializeField] [Tooltip("Which direction should we point the raycast to?")]
    private pointerDirection m_pointerDirection = pointerDirection.ZForward;


    /// We can adjust our pointer to behave in certain ways.
    /// - Target = the pointer is meant to be used as a selection tool
    /// - Teleport = the pointer acts as teleportation destination selector
    /// - Set_Target = a target is determined by script, and the pointer is merely a line render between the hand and the set target
    private enum pointerType {
        Target,
        Teleport,
        Set_Target
    }
    [SerializeField] [Tooltip("The type of pointer you wish to work with")]
    private pointerType m_pointerType;
    
    /// This is particularly used for locomotion. The line is either a straight line or a bezier curve downward to always hit the floor
    /// Laser - the line is a straight line, if the player wants it to be a straight line rather than a bezier curve
    /// Parabolic - the line arcs in a quadratic bezier curve. This makes it incredibly easy to select a destination because the player doesn't have to point their pointer downward.
    ///     In order to do a quadratic bezier curve, we need 3 points to reference. The three points are the pointer's current position, the end position of a forward-facing raycast that shoots out from the pointer, and the end position of a downward-facing raycast that starts from the previous raycast's end position.
    private enum laserType {
        Laser,
        Parabolic
    }
    [SerializeField] [Tooltip("The type of laser to use for locomotion")]
    private laserType m_laserType;

    /// Storing the pointer's target point. Using get{} set{} to prevent overwriting
    [Tooltip("Get the pointer's hit position")] // NOT SERIALIZED
    private Vector3 m_pointerDest = Vector3.zero;
    public Vector3 pointerDest {
        get {   return m_pointerDest;   }
        set {}
    }

    /// Storing the raycast's target point. Note that this is different from the pointer's destination point - the raycast doesn't curve.
    [SerializeField] [Tooltip("Storage of raycast target, if any")] // NOT SERIALIZED
    private GameObject m_raycastTarget = null;
    public GameObject raycastTarget {
        get {   return m_raycastTarget;     }
        set {   m_raycastTarget = value;    }
    }

    /// Storing the ryacast's origin point.
    [SerializeField] [Tooltip("The origin point from where the raycast should be performed")]
    private Transform m_raycastOrigin;
    public Transform raycastOrigin {
        get {   return m_raycastOrigin;     }
        set {   m_raycastOrigin = value;    }
    }

    /// How far is the forward-facing raycast allowed to travel?
    [SerializeField] [Tooltip("The distance the raycast is permitted to travel - default = 100 meters")]
    private float m_raycastDistance = 100f;
    public float raycastDistance {
        get {   return m_raycastDistance;   }
        set {   
            m_raycastDistance = value;  
            m_raycastDownwardDistance = value * 2f;
            ResizeVolumeDetectors();
        }
    }
    
    /// How far is the downward raycast permitted to travel?
    [Tooltip("The distance the downward raycast is permitted to travel - default = 10m")] // NOT SERIALIZED
    private float m_raycastDownwardDistance;
    public float raycastDownwardDistance {
        get {   return m_raycastDownwardDistance;   }
        set {   m_raycastDownwardDistance = value;  }
    }
    
    /// Is the downward-facing raycast hitting a floor or anything? Used by `CustomLocomotion` class to determine if the player is allowed to locomote.
    [Tooltip("bool for detecting if the raycast is actually hitting anything")] // NOT SERIALIZED
    private bool m_raycastDownwardHit = false;
    public bool raycastDownwardHit {
        get {   return m_raycastDownwardHit;    }
        set {}
    }

    /// The line renderer requires an array of Vector3 points to use as reference to render the line.
    [Tooltip("the list of points the line renderer will render - must be converted into array Vector3[] to use properly")] // NOT SERIALIZED
    private List<Vector3> m_linePoints = new List<Vector3>();

    /// The color of the line renderer
    [SerializeField] [Tooltip("The color the line must be rendered with")]
    private Color m_lineColor = new Color32(255, 255, 0, 255);
    public Color lineColor {
        get {   return m_lineColor;     }
        set {   m_lineColor = value;    }
    }

    /// There may be cases where we should be still perform the raycast to select a target, without activating the line renderer. If TRUE, the raycast will still look for hit detection despite the pointer not being active.
    [SerializeField] [Tooltip("The detection of a raycast target can only be performed when the pointer is activated")]
    private bool m_detectOnlyWhenLineSet = false;
    public bool detectOnlyWhenLineSet {
        get {   return m_detectOnlyWhenLineSet;   }
        set {   m_detectOnlyWhenLineSet = value;  }
    }

    /// How many points should the line renderer use when printing the line?
    [SerializeField] [Tooltip("The number of points the line renderer will use when printing the line")]
    private int m_numPoints = 30;
    public int numPoints {
        get {   return m_numPoints; }
        set {   m_numPoints = value;}
    }

    /// Where should the locomotion destination position be? Only important to `CustomLocomotion`
    [Tooltip("The location where the player must teleport to - only important to locomotion")] // NOT SERIALIZED
    private Vector3 m_locomotionPosition = Vector3.zero;
    public Vector3 locomotionPosition {
        get {   return m_locomotionPosition;    }
        set {   m_locomotionPosition = value;   }
    }

    /// Reference to a hover cursor prefab that is used when the pointer hovers over something.
    /// When we hover over a grabbable or detectable object with our pointer, a hover cursor is rendered around the object to let the user know that they can interact with it.
    [SerializeField] [Tooltip("Reference to the hover cursor prefab")]
    private HoverCursor m_hoverCursor;
    [SerializeField] [Tooltip("Should we use a hover cursor?")]
    private bool m_shouldUseHoverCursor = true;
    private HoverCursor m_instantiatedHoverCursor;
    public HoverCursor instantiatedHoverCursor {
        get {   return m_instantiatedHoverCursor;   }
        set {   
            if (m_instantiatedHoverCursor != null) m_instantiatedHoverCursor.Relieve();
            m_instantiatedHoverCursor = value;
        }
    }

    /// Sometimes, ojbects are too far away to be detected by a single raycast. In this case, we can use a volumetric detection method that
    ///     uses hit collision rather than a raycast to detect far-away objects. 
    /// The volume used is a series of cylinders that increase in radius the further we go out from the raycast origin point.
    /// Based on the forward-facing raycast's max distance, the cylinders' heights scale so that cylinders form a Cone shape, meaning the range of hit detection increases the farther the raycast is sent out.
    /// However, because volumetric detection generally puts a bit of stress on the system given the Quest's older Android architecture, we can toggle to rely on a single raycast instead if necessary.
    private enum longDistanceDetectionType {
        None,
        Volumetric
    }
    [SerializeField] [Tooltip("For long-distance detection, if we want some kind of long-distance object detection")]
    private longDistanceDetectionType m_longDistanceDetectionType = longDistanceDetectionType.None;
    [SerializeField] [Tooltip("Transform gameObject used for volume-based detection")]
    private List<Transform> m_volumeDetectors = new List<Transform>();

    [SerializeField] [Tooltip("All layer masks that are supposed to be ignored")]
    private List<string> m_layersToIgnore = new List<string>();

    /// Debug Mode simply turns on all rendered lines, volumetric cylinders, and cursors for debugging
    [SerializeField] [Tooltip("For debugging purposes")]
    private bool m_debugMode = false;
    public bool debugMode {
        get {   return m_debugMode;     }
        set {   SetDebugMode(value);    }
    }

    [SerializeField] [Tooltip("For debugging purposes")]
    private List<GameObject> m_debugObjects = new List<GameObject>();

    [SerializeField] [Tooltip("FOR DEBUGGING")]
    private GameObject m_XYZ;

    /// is this component active?
    // NOT SERIALIZED
    private bool m_isActive = false;
    public bool isActive {
        get {   return m_isActive;  }
        set {   m_isActive = value; }
    }
    
    /// We can ascertain that only certain layers are being detected by the forward-facing raycast by using layer masks
    // The layer masks here are ones that our raycasters are NOT meant to detect
    // NOT SERIALIZED
    private int layerMaskHover, layerMaskLocomotion, combinedLayerMask;
    private List<int> m_layerMasks = new List<int>();

    #endregion
    #region Unity callbacks

    /// If we're supposed to initialize upon scene starting, we do so.
    private void Start() {
        if (m_shouldStart) Init(true, m_grabber);
    }

    /// In each frame, we need to get the points for our line renderer, then detect if we've hit anything with our pointer, then render our linerenderer
    private void Update() {

        // Return early if we're suppoed to be off
        if (!m_isActive) return;
        // Get our line renderer's points. We also find `m_raycastTarget` and `m_locomotionPosition` at the same time
        m_linePoints = GetPoints();
        // If we're only detecting targets when the line is on, we set the raycast target to NULL.
        if (m_detectOnlyWhenLineSet) {    m_raycastTarget = (m_LineRenderer.enabled) ? m_raycastTarget : null;    }

        // This only applies if 1) our pointer is not a teleporting one, and 2) we have a hover cursor prefab set
        if (m_pointerType != pointerType.Teleport && m_shouldUseHoverCursor && m_hoverCursor != null) {
            // if we're hitting something with our raycast and that something is in a layer that is allowed to be hovered over...
            if (m_raycastTarget != null && m_raycastTarget.gameObject.layer != layerMaskHover && m_raycastTarget.gameObject.layer != LayerMask.NameToLayer("UI")) {
                // If we already have an instantiated hover cursor...
                if (instantiatedHoverCursor != null) {
                    // ... and that gameobject's reference ID doesn't match that of the raycast's target...
                    if (!GameObject.ReferenceEquals(instantiatedHoverCursor.target, m_raycastTarget)) {
                        // ... then we delete that existing Hover Cursor, create a new one, and initialize it to follow our raycast target (the hover cursors are their own class)
                        instantiatedHoverCursor.Relieve();
                        instantiatedHoverCursor = Instantiate(m_hoverCursor,Vector3.zero,Quaternion.identity);
                        instantiatedHoverCursor.Init(m_raycastTarget.transform, m_lineColor);
                    }
                } 
                else {  // If we don't have a hover cursor already instantiated in the world, then we instantiate a hover cursor
                    instantiatedHoverCursor = Instantiate(m_hoverCursor,Vector3.zero,Quaternion.identity);
                    instantiatedHoverCursor.Init(m_raycastTarget.transform, m_lineColor);
                }
            } 
            else if (instantiatedHoverCursor != null) { 
                // Either our raycast isn't hitting something or our target has a layer that's meant to be avoided, so we should relieve the hover cursor if it exists
                instantiatedHoverCursor.Relieve();
                instantiatedHoverCursor = null;
            }
        } 

        // if our line renderer itself is active, we render the line
        if (m_LineRenderer.enabled) {   
            m_LineRenderer.material.SetColor("_Color", m_lineColor);
            m_LineRenderer.positionCount = m_numPoints;
            Vector3[] points = m_linePoints.ToArray();
            m_LineRenderer.SetPositions(points); 
        }

        // If in debug mode, make objects appear or disappear based on if the pointer is hitting something
        if (m_debugMode) {
            bool isHitting = (m_raycastTarget != null);
            foreach(GameObject oj in m_debugObjects) {
                oj.SetActive(isHitting);
            }
        }
    }

    #endregion
    #region Initializers

    /// This initializer stores our references, generates our layer masks to avoid, and activates if it's allowed to start
    public void Init(bool canStart = false, CustomGrabber grabber = null) {
        m_LineRenderer = this.GetComponent<LineRenderer>();
        if (m_raycastOrigin == null) {  m_raycastOrigin = this.transform;   }
        m_grabber = grabber;
        m_raycastDownwardDistance = m_raycastDistance;

        layerMaskHover = 1 << LayerMask.NameToLayer("AvoidHover");
        foreach(string s in m_layersToIgnore) {
            int thisLayerID = LayerMask.NameToLayer(s);
            if (thisLayerID > -1) {
                int thisLayerMask = 1 << thisLayerID;
                layerMaskHover = layerMaskHover | thisLayerMask;
            }
        }
        layerMaskHover = ~layerMaskHover;
        layerMaskLocomotion = 1 << LayerMask.NameToLayer("Locomotion");
        layerMaskLocomotion = ~layerMaskLocomotion;
        combinedLayerMask = layerMaskHover | layerMaskLocomotion;

        m_LineRenderer.positionCount = m_numPoints;
        ResizeVolumeDetectors();
        SetDebugMode(m_debugMode);
        
        if (canStart) Activate();
    }

    public void Activate() {
        m_isActive = true;
    }
    public void Deactivate() {
        m_isActive = false;
        Vector3[] points = new Vector3[30];
        for (int i = 0; i < points.Length; i++) {
            points[i] = Vector3.zero;
        }
        m_LineRenderer.SetPositions(points);
    }

    // We resize ur volume detectors to make a cone shape based on 1) how far we're allowed to raycast forward, and 2) how many cylinders we are using to create the cone.
    private void ResizeVolumeDetectors() {
        float zSize = m_raycastDistance / m_volumeDetectors.Count;
        for(int i = 0; i < m_volumeDetectors.Count; i++) {
            Transform t = m_volumeDetectors[i];
            t.transform.localScale = new Vector3(t.transform.localScale.x, t.transform.localScale.y, zSize);
            t.transform.localPosition = new Vector3(0,0, (i*zSize)+(zSize/2));
        }
    }

    #endregion
    #region Raycasting Functions

    /// In this function, we 1) perform the necessary raycasts to get our pointer's target, and 2) generate the necessary points to generate the pointer's line renderers
    private List<Vector3> GetPoints() {
        List<Vector3> points = new List<Vector3>();
        Vector3 destination;

        // The end destination of our pointer and the lines the line renderer will render with is dependent on the type of pointer we're using. 
        switch(m_pointerType) {
            case(pointerType.Target):
                // If we're a targeting/selection pointer, we default to simply raycasting forward, then raycasting downward.
                m_pointerDest = CheckRaycastForward(layerMaskHover);
                destination = CheckRaycastDownward(m_pointerDest);
                // Based on our laser type, the line renderer will generate points to create either a curve or straight line.
                switch(m_laserType) {
                    case(laserType.Laser):
                        points = BezierCurves.DetermineLinearCurve(m_numPoints,m_raycastOrigin.position,m_pointerDest,0);
                        break;
                    case(laserType.Parabolic):
                        points = BezierCurves.DetermineQuadraticCurve(m_numPoints,m_raycastOrigin.position,m_pointerDest,destination,0);
                        break;
                }
                break;
            case(pointerType.Teleport):
                // Our pointer will only get a destination based on if we're grabbing anything in the hand. This is because an object being held in the hand can block the pointer, which is inconducive if we want to teleport elsewhere.
                m_pointerDest = (m_grabber != null && m_grabber.grabbedObject != null) ? CheckRaycastForward(combinedLayerMask) : CheckRaycastForward(layerMaskLocomotion);
                destination = CheckRaycastDownward(m_pointerDest);
                // Based on our laser type, the line renderer will generate points to create either a curve or straight line.
                switch(m_laserType) {
                    case(laserType.Laser):
                        points = BezierCurves.DetermineLinearCurve(m_numPoints,m_raycastOrigin.position,m_pointerDest,0);
                        if (m_LineRenderer.enabled) m_locomotionPosition = m_pointerDest;
                        break;
                    case(laserType.Parabolic):
                        points = BezierCurves.DetermineQuadraticCurve(m_numPoints,m_raycastOrigin.position,m_pointerDest,destination,0);
                        if (m_LineRenderer.enabled) m_locomotionPosition = destination;
                        break;
                }
                break;
            case(pointerType.Set_Target):
                // In this case, we have a target pre-selected already. Our pointer destination is merely either that target's position, or nothing (aka the origin point, which will not generate a line)
                destination = (m_raycastTarget != null) ? m_raycastTarget.transform.position : m_raycastOrigin.position;
                // The line renderer's points, in this case, is always going to be a linear line.
                points = BezierCurves.DetermineLinearCurve(m_numPoints,m_raycastOrigin.position,destination,0);
                break;
        }

        // We return the points we've generated.
        return points;
    }
 
    // The first step is to send a raycast 
    private Vector3 CheckRaycastForward(int layersToAvoid = 1 << 3) {
        RaycastHit rayHit;
        Vector3 raycastDirection;
        switch(m_pointerDirection) {
            case(pointerDirection.ZForward):
                raycastDirection = m_raycastOrigin.forward;
                break;
            case(pointerDirection.ZBackward):
                raycastDirection = m_raycastOrigin.forward * -1f;
                break;
            case(pointerDirection.YForward):
                raycastDirection = m_raycastOrigin.up;
                break;
            case(pointerDirection.YBackward):
                raycastDirection = m_raycastOrigin.up * -1f;
                break;
            case(pointerDirection.XForward):
                raycastDirection = m_raycastOrigin.right;
                break;
            case(pointerDirection.XBackward):
                raycastDirection = m_raycastOrigin.right * -1f;
                break;
            default:
                raycastDirection = m_raycastOrigin.forward;
                break;
        }
        Vector3 returnPoint = m_raycastOrigin.position + raycastDirection * m_raycastDistance;
        //if (Physics.Raycast(m_raycastOrigin.position, m_raycastOrigin.TransformDirection(Vector3.forward), out rayHit, m_raycastDistance, layersToAvoid)) {
        if (Physics.Raycast(m_raycastOrigin.position, raycastDirection, out rayHit, m_raycastDistance, layersToAvoid)) {
            // Something in front of it
            m_raycastTarget = rayHit.transform.gameObject;
            returnPoint = rayHit.point;
        } else {
            // Now, this is actually crucial.
            // If our raycast hit something, then we don't actually need to worry about this step... but sometimes, our raycast is trying to hit something far away, and our pointer isn't really having a good time. In these situations, we need volume-based detection.
            // Now, here's the conundrum: we can do volume-based detection, but the hard part is deciding how to select objects
            // For example, let's say there's two objects picked up by my volume-based detection system. 
            //      One is closer to the line but far away from the end of the line. The other is closer to the end of the line but is far from the line. 
            // Anyone might argue "It makes more sense for the object closer to the line itself to be favored"...
            //      But you don't really know for certain. You could still be trying to pick up that object that's far away even if there are objects closer to your line in the way.
            // Here's how I see it:
            //      There's a weighing that's going on. There are two factors in this weighing: 1) distance between the object and the line, and 2) distance between the object and the endpoint
            //      In other words, objects that are closer to the player (and therefore farther from the endpoint) are weighed less than those closer to the endpoint
            //      So even if the closer object is for sure closer to the line than the other object, the other object is favored over the closer object because the further-away object is closer to the end.
            //      However, this doesn't completely overwrite the object closer to us. If our line is close enough to the closer object, we'll select that closer object over the further object.
            // The weighing is done as such:
            //      weighed value = distance from line / distance from endpoint.
            //      distance from line = sin(angle between raycast origin and object) * distance between raycast origin and object, since sin(angle) = furthest / hypotenuse
            //          angle = Vector3.Angle((object's position - raycast origin position), m_raycastOrigin.TransformDirection(Vector3.forward))
            //          distance = Vector3.Distance(object's position, raycast origin position)
            //      distance from endpoint = Vector3.Distance(object's position, returnPoint)

            // This was a switch case because I initially wanted to have multiple ways to do detection, but volumetric is the only one I managed to develop over time.
            switch(m_longDistanceDetectionType) {
                case(longDistanceDetectionType.Volumetric):
                    // We break early if we don't have any volume detectors
                    if (m_volumeDetectors.Count == 0) {
                        m_raycastTarget = null;
                        break;
                    }
                    //float maxAngle = Mathf.Abs(Mathf.Deg2Rad * Vector3.Angle((new Vector3(returnPoint.x,returnPoint.y+m_volumeScale,returnPoint.z) - m_raycastOrigin.position), m_raycastOrigin.TransformDirection(Vector3.forward)));
                    List<Collider> hitColliders = new List<Collider>();
                    foreach(Transform t in m_volumeDetectors) {
                        Collider[] hc = Physics.OverlapBox(t.position, t.localScale / 2, t.rotation, layersToAvoid);
                        foreach(Collider c in hc) {
                            if (!hitColliders.Contains(c)) hitColliders.Add(c);
                        }
                    }
                    if (hitColliders.Count == 0) {
                        m_raycastTarget = null;
                        break;
                    }
                    float highestWeighedValue = 0f;
                    Collider closest = null;
                    foreach(Collider c in hitColliders) {
                        float angle = Mathf.Deg2Rad* Vector3.Angle(c.transform.position - m_raycastOrigin.position, m_raycastOrigin.TransformDirection(Vector3.forward));
                        //if (Mathf.Abs(angle) > maxAngle) continue;
                        float weighedValue = Mathf.Sin(angle) / Vector3.Distance(c.transform.position, m_raycastOrigin.position);
                        if (weighedValue > highestWeighedValue) {
                            highestWeighedValue = weighedValue;
                            closest = c;
                        }
                    }
                    m_raycastTarget = closest.gameObject;
                    break;
                default:
                    m_raycastTarget = null;
                    break;
            }
        }
        return returnPoint;
    }
    /// The raycast downward is a step performed for locomotion mostly. We cast a raycast downward starting from our forward raycast's destination point to see if we're hitting a floor.
    private Vector3 CheckRaycastDownward(Vector3 origin) {
        RaycastHit rayHit;
        Vector3 destination = origin;
        m_raycastDownwardHit = false;
        if (Physics.Raycast(new Vector3(origin.x,origin.y+0.01f,origin.z), -Vector3.up, out rayHit, m_raycastDownwardDistance+0.01f, layerMaskLocomotion)) {
            // Floor is hit
            destination = new Vector3(origin.x, rayHit.point.y, origin.z);
            m_raycastDownwardHit = true;
        }
        return destination;
    }

    #endregion
    #region Getters and Setters

    // A series of Get functions that return boolean values based on certain conditions
    public bool isTargetType() {
        return m_pointerType == pointerType.Target;
    }
    public bool isTeleportType() {
        return m_pointerType == pointerType.Teleport;
    }
    public bool isSetTargetType() {
        return m_pointerType == pointerType.Set_Target;
    }

    // We use this function as a Get function for the type of pointer.
    // This is necessary because the `pointerType` is not a static class and therefore is only scopable within this class. We need to interpret strings and convert them into `pointerType` enum values
    public string GetPointerType() {
        string t = "";
        switch(m_pointerType) {
            case(pointerType.Target):
                t = "Target";
                break;
            case(pointerType.Teleport):
                t = "Teleport";
                break;
            case(pointerType.Set_Target):
                t = "SetTarget";
                break;
        }
        return t;
    }

    // We use this function if we want to programmatically change the kind of pointer we're working with.
    // This is necessary because the `pointerType` is not a static class and therefore is only scopable within this class. We need to interpret strings and convert them into `pointerType` enum values
    public void SetPointerType(string t, GameObject targetForSet = null) {
        switch(t) {
            case("Target"):
                m_pointerType = pointerType.Target;
                break;
            case("Teleport"):
                m_pointerType = pointerType.Teleport;
                break;
            case("SetTarget"):
                m_pointerType = pointerType.Set_Target;
                m_raycastTarget = targetForSet;
                break;
        }
        return;
    }
    // We can also set our laser type here, if we wish to change the laser type programmatically
    public void SetLaserType(string t) {
        switch(t) {
            case("Laser"):
                m_laserType = laserType.Laser;
                break;
            case("Parabolic"):
                m_laserType = laserType.Parabolic;
                break;
        }
        return;
    }
    // A Set function that sets the target for our raycast - used for when our pointer type is `Set_Target`
    public void SetTarget(GameObject targetForSet = null) {
        m_raycastTarget = targetForSet;
    }

    // A series of getters and setters pertaining to the line renderer itself
    public bool LineIsEnabled() {
        return m_LineRenderer.enabled;
    }
    public void LineSet(bool s) {
        m_LineRenderer.enabled = (!m_debugMode) ? s : true;
    }
    public void LineToggle() {
        m_LineRenderer.enabled = (!m_debugMode) ? !m_LineRenderer.enabled : true;
    }
    public void LineOff() {
        m_LineRenderer.enabled = (!m_debugMode) ? false : true;
    }
    public void LineOn() {
        m_LineRenderer.enabled = true;
    }
    public void LineOff(OVRInput.Controller c) {
        if (c == m_grabber.OVRController) m_LineRenderer.enabled = (!m_debugMode) ? false : true;
    }
    public void LineOn(OVRInput.Controller c) {
        if (c == m_grabber.OVRController) m_LineRenderer.enabled = true;
    }

    public void DisableHoverCursor() {
        m_shouldUseHoverCursor = false;
    }
    public void EnableHoverCursor() {
        m_shouldUseHoverCursor = true;
    }

    // Set our debug mode
    private void SetDebugMode(bool dMode) {
        m_debugMode = dMode;
        //m_XYZ.SetActive(m_debugMode);
        foreach(Transform t in m_volumeDetectors) {
            t.GetComponent<Renderer>().enabled = dMode;
        }
    }

    #endregion
    */
}