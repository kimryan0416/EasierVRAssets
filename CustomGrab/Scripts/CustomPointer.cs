using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

<<<<<<< HEAD
/* ------------
- The purpose of this component is to control a pointer (aka a Line Renderer) that is used for various functions such as pointing, pinging, and locomotion.
- In virtual reality, pointing is a common tool utilized in various user interfaces because it affords for (semi-)accurate selection and targeting. 
- In our custom package, we need a pointer for various activities, most notably distance grabbing and locomotion. This script interacts with these other functions and any others that may come in the future.
------------ */
[RequireComponent(typeof(LineRenderer))] // We require a Line Renderer to be attached to this GameObject - the Line Renderer renders the pointer.
public class CustomPointer: MonoBehaviour
{   
    #region Variables and References

    /// Should this script be initialized upon scene load? Or should it wait until it's initialized by another class?
    [SerializeField] [Tooltip("Should the pointer start from the getgo, or should it wait for an Init call?")]
    private bool m_shouldStart = true;

    /// Reference to our Custom Grabber, which is referenced when doing checks and balances in the script
    [SerializeField] [Tooltip("Reference to grabber, if this requires a grabber to work")]
    private CustomGrabber m_grabber = null;

    /// A storage reference to the Line Renderer component that is attached to this GameObject
    [Tooltip("Reference to LineRenderer")] // NOT SERIALIZED
=======
[RequireComponent(typeof(LineRenderer))]
public class CustomPointer: MonoBehaviour
{   

    [SerializeField]
    [Tooltip("Reference to grabber, if this requires a grabber to work")]
    private CustomGrabber m_grabber = null;

    // NOT SERIALIZED
    [Tooltip("Reference to LineRenderer")]
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private LineRenderer m_LineRenderer;
    public LineRenderer lineRenderer {
        get {   return m_LineRenderer;  }
        set {   m_LineRenderer = value; }
    }

<<<<<<< HEAD
    /// We can adjust our pointer to behave in certain ways.
    /// - Target = the pointer is meant to be used as a selection tool
    /// - Teleport = the pointer acts as teleportation destination selector
    /// - Set_Target = a target is determined by script, and the pointer is merely a line render between the hand and the set target
=======
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private enum pointerType {
        Target,
        Teleport,
        Set_Target
    }
<<<<<<< HEAD
    [SerializeField] [Tooltip("The type of pointer you wish to work with")]
    private pointerType m_pointerType;
    
    /// This is particularly used for locomotion. The line is either a straight line or a bezier curve downward to always hit the floor
    /// Laser - the line is a straight line, if the player wants it to be a straight line rather than a bezier curve
    /// Parabolic - the line arcs in a quadratic bezier curve. This makes it incredibly easy to select a destination because the player doesn't have to point their pointer downward.
    ///     In order to do a quadratic bezier curve, we need 3 points to reference. The three points are the pointer's current position, the end position of a forward-facing raycast that shoots out from the pointer, and the end position of a downward-facing raycast that starts from the previous raycast's end position.
=======
    [SerializeField]
    [Tooltip("The type of pointer you wish to work with")]
    private pointerType m_pointerType;
    
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private enum laserType {
        Laser,
        Parabolic
    }
<<<<<<< HEAD
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
    [Tooltip("Storage of raycast target, if any")] // NOT SERIALIZED
=======
    [SerializeField]
    [Tooltip("The type of laser to use for locomotion")]
    private laserType m_laserType;

    // NOT SERIALIZED
    [Tooltip("Get the pointer's hit position")]
    private Vector3 m_pointerDest = Vector3.zero;
    public Vector3 pointerDest {
        get {   return m_pointerDest;   }
    }
    // NOT SERIALIZED
    [Tooltip("storage of raycast target, if any")]
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private GameObject m_raycastTarget = null;
    public GameObject raycastTarget {
        get {   return m_raycastTarget;     }
        set {   m_raycastTarget = value;    }
    }

<<<<<<< HEAD
    /// Storing the ryacast's origin point.
    [SerializeField] [Tooltip("The origin point from where the raycast should be performed")]
=======
    [SerializeField]
    [Tooltip("The origin point from where the raycast should be performed")]
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private Transform m_raycastOrigin;
    public Transform raycastOrigin {
        get {   return m_raycastOrigin;     }
        set {   m_raycastOrigin = value;    }
    }

<<<<<<< HEAD
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
=======
    [SerializeField]
    [Tooltip("The distance the raycast is permitted to travel - defaul = 100 meters")]
    private float m_raycastDistance = 100f;
    public float raycastDistance {
        get {   return m_raycastDistance;   }
        set {   m_raycastDistance = value;  }
    }

    // NOT SERIALIZED
    [Tooltip("the list of points the line renderer will render - must be converted into Vector3[] to use properly")]
    private List<Vector3> m_linePoints = new List<Vector3>();

    [SerializeField]
    [Tooltip("The color the line must be rendered with")]
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private Color m_lineColor = new Color32(255, 255, 0, 255);
    public Color lineColor {
        get {   return m_lineColor;     }
        set {   m_lineColor = value;    }
    }

<<<<<<< HEAD
    /// There may be cases where we should be still perform the raycast to select a target, without activating the line renderer. If TRUE, the raycast will still look for hit detection despite the pointer not being active.
    [SerializeField] [Tooltip("The detection of a raycast target can only be performed when the pointer is activated")]
    private bool m_detectOnlyWhenLineSet = false;
    public bool detectOnlyWhenLineSet {
        get {   return m_detectOnlyWhenLineSet;   }
        set {   m_detectOnlyWhenLineSet = value;  }
    }

    /// How many points should the line renderer use when printing the line?
    [SerializeField] [Tooltip("The number of points the line renderer will use when printing the line")]
=======
    [SerializeField]
    [Tooltip("The detection of a raycast target can only be performed when the pointer is activated")]
    private bool m_detectOnlyWhenActivated = false;
    public bool detectOnlyWhenActivated {
        get {   return m_detectOnlyWhenActivated;   }
        set {   m_detectOnlyWhenActivated = value;  }
    }

    [SerializeField]
    [Tooltip("The number of points the line renderer will use when printing the line")]
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private int m_numPoints = 30;
    public int numPoints {
        get {   return m_numPoints; }
        set {   m_numPoints = value;}
    }

<<<<<<< HEAD
    /// Where should the locomotion destination position be? Only important to `CustomLocomotion`
    [Tooltip("The location where the player must teleport to - only important to locomotion")] // NOT SERIALIZED
=======
    // NOT SERIALIZED
    [Tooltip("The location where the player must teleport to - only important to locomotion")]
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private Vector3 m_locomotionPosition = Vector3.zero;
    public Vector3 locomotionPosition {
        get {   return m_locomotionPosition;    }
        set {   m_locomotionPosition = value;   }
    }

<<<<<<< HEAD
    /// Reference to a hover cursor prefab that is used when the pointer hovers over something.
    /// When we hover over a grabbable or detectable object with our pointer, a hover cursor is rendered around the object to let the user know that they can interact with it.
    [SerializeField] [Tooltip("Reference to the hover cursor prefab")]
=======
    [SerializeField]
    [Tooltip("Reference to the hover cursor prefab")]
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private HoverCursor m_hoverCursor;
    private HoverCursor m_instantiatedHoverCursor;
    public HoverCursor instantiatedHoverCursor {
        get {   return m_instantiatedHoverCursor;   }
        set {   
            if (m_instantiatedHoverCursor != null) m_instantiatedHoverCursor.Relieve();
            m_instantiatedHoverCursor = value;
        }
    }

<<<<<<< HEAD
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

    /// Debug Mode simply turns on all rendered lines, volumetric cylinders, and cursors for debugging
    [SerializeField] [Tooltip("For debugging purposes")]
    private bool m_debugMode = false;
    public bool debugMode {
        get {   return m_debugMode;     }
        set {   SetDebugMode(value);    }
    }

    [SerializeField] [Tooltip("FOR DEBUGGING")]
    private GameObject m_XYZ;

    /// is this component active?
=======
    //[SerializeField]
    //private bool m_showTransparentLine = false;
    // Here's the rub:
    // The idea is that when "m_detectOnlyWhenActivated" is set to TRUE...
    //  ... then the custompointer will report the target as whatever's colliding with it...
    //  ... when the trigger is pressed
    // When "m_showTransparentLine" is set to TRUE...
    //  ... then the line's material will change
    // There are some things to keep in mind:
    // 1) the line renderer will ALWAYS be enabled
    // 2) the line renderer's appearance will change depending on the "m_showTransparentLine" parameter
    // 3) the line renderer's public target (not private target) will be set to NULL depending on the "m_detectOnlyWhenActivated" parameter

    [SerializeField]
    [Tooltip("For debugging purposes")]
    private bool m_debugMode = false;
    public bool debugMode {
        get {   return m_debugMode;     }
        set {   m_debugMode = value;    }
    }

    [SerializeField]
    [Tooltip("FOR DEBUGGING")]
    private GameObject m_XYZ;

>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    // NOT SERIALIZED
    private bool m_isActive = false;
    public bool isActive {
        get {   return m_isActive;  }
        set {   m_isActive = value; }
    }
    
<<<<<<< HEAD
    /// We can ascertain that only certain layers are being detected by the forward-facing raycast by using layer masks
    // The layer masks here are ones that our raycasters are NOT meant to detect
=======
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    // NOT SERIALIZED
    private int layerMaskHover, layerMaskLocomotion, combinedLayerMask;
    private List<int> m_layerMasks = new List<int>();

<<<<<<< HEAD
    #endregion
    #region Unity callbacks

    /// If we're supposed to initialize upon scene starting, we do so.
    private void Start() {
        if (m_shouldStart) Init(true);
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
        if (m_pointerType != pointerType.Teleport && m_hoverCursor != null) {
            // if we're hitting something with our raycast and that something is in a layer that is allowed to be hovered over...
            if (m_raycastTarget != null && m_raycastTarget.gameObject.layer != layerMaskHover) {
                // If we already have an instantiated hover cursor...
                if (instantiatedHoverCursor != null) {
                    // ... and that gameobject's reference ID doesn't match that of the raycast's target...
                    if (!GameObject.ReferenceEquals(instantiatedHoverCursor.target, m_raycastTarget)) {
                        // ... then we delete that existing Hover Cursor, create a new one, and initialize it to follow our raycast target (the hover cursors are their own class)
=======
    public void Init(bool canStart = false) {
        m_LineRenderer = this.GetComponent<LineRenderer>();
        if (m_raycastOrigin == null) {  m_raycastOrigin = this.transform;   }
        
        layerMaskHover = 1 << LayerMask.NameToLayer("AvoidHover");
        layerMaskHover = ~layerMaskHover;
        layerMaskLocomotion = 1 << LayerMask.NameToLayer("Locomotion");
        layerMaskLocomotion = ~layerMaskLocomotion;
        combinedLayerMask = layerMaskHover | layerMaskLocomotion;

        m_LineRenderer.positionCount = m_numPoints;

        if (canStart) Activate();
    }

    private void Update() {
        if (!m_isActive) return;
        // (if in debug mode, turn on XYZ)
        m_XYZ.SetActive(m_debugMode);
        // Update start position

        m_linePoints = GetPoints();

        if (m_detectOnlyWhenActivated) {
            m_raycastTarget = (m_LineRenderer.enabled) ? m_raycastTarget : null;
        }

        if (m_pointerType != pointerType.Teleport && m_hoverCursor != null) {
            if (m_raycastTarget != null && m_raycastTarget.gameObject.layer != layerMaskHover) {
                if (instantiatedHoverCursor != null) {
                    if (!GameObject.ReferenceEquals(instantiatedHoverCursor.target, m_raycastTarget)) {
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
                        instantiatedHoverCursor.Relieve();
                        instantiatedHoverCursor = Instantiate(m_hoverCursor,Vector3.zero,Quaternion.identity);
                        instantiatedHoverCursor.Init(m_raycastTarget.transform, m_lineColor);
                    }
<<<<<<< HEAD
                } 
                else {  // If we don't have a hover cursor already instantiated in the world, then we instantiate a hover cursor
=======
                } else {
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
                    instantiatedHoverCursor = Instantiate(m_hoverCursor,Vector3.zero,Quaternion.identity);
                    instantiatedHoverCursor.Init(m_raycastTarget.transform, m_lineColor);
                }
            } 
<<<<<<< HEAD
            else if (instantiatedHoverCursor != null) { 
                // Either our raycast isn't hitting something or our target has a layer that's meant to be avoided, so we should relieve the hover cursor if it exists
=======
            else if (instantiatedHoverCursor != null) {
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
                instantiatedHoverCursor.Relieve();
                instantiatedHoverCursor = null;
            }
        } 

<<<<<<< HEAD
        // if our line renderer itself is active, we render the line
=======
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
        if (m_LineRenderer.enabled) {   
            m_LineRenderer.material.SetColor("_Color", m_lineColor);
            m_LineRenderer.positionCount = m_numPoints;
            Vector3[] points = m_linePoints.ToArray();
            m_LineRenderer.SetPositions(points); 
        }
    }

<<<<<<< HEAD
    #endregion
    #region Initializers

    /// This initializer stores our references, generates our layer masks to avoid, and activates if it's allowed to start
    public void Init(bool canStart = false, CustomGrabber grabber = null) {
        m_LineRenderer = this.GetComponent<LineRenderer>();
        if (m_raycastOrigin == null) {  m_raycastOrigin = this.transform;   }
        m_grabber = grabber;
        m_raycastDownwardDistance = m_raycastDistance;
        
        layerMaskHover = 1 << LayerMask.NameToLayer("AvoidHover");
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
=======
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private List<Vector3> GetPoints() {
        List<Vector3> points = new List<Vector3>();
        Vector3 destination;

<<<<<<< HEAD
        // The end destination of our pointer and the lines the line renderer will render with is dependent on the type of pointer we're using. 
        switch(m_pointerType) {
            case(pointerType.Target):
                // If we're a targeting/selection pointer, we default to simply raycasting forward, then raycasting downward.
                m_pointerDest = CheckRaycastForward(layerMaskHover);
                destination = CheckRaycastDownward(m_pointerDest);
                // Based on our laser type, the line renderer will generate points to create either a curve or straight line.
=======
        switch(m_pointerType) {
            case(pointerType.Target):
                m_pointerDest = CheckRaycastForward(layerMaskHover);
                destination = CheckRaycastDownward(m_pointerDest);
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
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
<<<<<<< HEAD
                // Our pointer will only get a destination based on if we're grabbing anything in the hand. This is because an object being held in the hand can block the pointer, which is inconducive if we want to teleport elsewhere.
                m_pointerDest = (m_grabber != null && m_grabber.grabbedObject != null) ? CheckRaycastForward(combinedLayerMask) : CheckRaycastForward(layerMaskLocomotion);
                destination = CheckRaycastDownward(m_pointerDest);
                // Based on our laser type, the line renderer will generate points to create either a curve or straight line.
=======
                m_pointerDest = (m_grabber != null && m_grabber.grabbedObject != null) ? CheckRaycastForward(combinedLayerMask) : CheckRaycastForward(layerMaskLocomotion);
                destination = CheckRaycastDownward(m_pointerDest);
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
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
<<<<<<< HEAD
                // In this case, we have a target pre-selected already. Our pointer destination is merely either that target's position, or nothing (aka the origin point, which will not generate a line)
                destination = (m_raycastTarget != null) ? m_raycastTarget.transform.position : m_raycastOrigin.position;
                // The line renderer's points, in this case, is always going to be a linear line.
=======
                destination = (m_raycastTarget != null) ? m_raycastTarget.transform.position : m_raycastOrigin.position;
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
                points = BezierCurves.DetermineLinearCurve(m_numPoints,m_raycastOrigin.position,destination,0);
                break;
        }

<<<<<<< HEAD
        // We return the points we've generated.
        return points;
    }
 
    // The first step is to send a raycast 
=======
        return points;
    }
 
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    private Vector3 CheckRaycastForward(int layersToAvoid = 1 << 3) {
        RaycastHit rayHit;
        Vector3 returnPoint = m_raycastOrigin.position + m_raycastOrigin.forward * m_raycastDistance;
        if (Physics.Raycast(m_raycastOrigin.position, m_raycastOrigin.TransformDirection(Vector3.forward), out rayHit, m_raycastDistance, layersToAvoid)) {
            // Something in front of it
            m_raycastTarget = rayHit.transform.gameObject;
            returnPoint = rayHit.point;
        } else {
<<<<<<< HEAD
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
=======
            m_raycastTarget = null;
        }
        return returnPoint;
    }

    private Vector3 CheckRaycastDownward(Vector3 origin) {
        RaycastHit rayHit;
        Vector3 destination = origin;
        if (Physics.Raycast(origin, -Vector3.up, out rayHit, Mathf.Infinity, layerMaskLocomotion)) {
            // Floor is hit
            destination = new Vector3(origin.x, rayHit.point.y, origin.z);
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
        }
        return destination;
    }

<<<<<<< HEAD
    #endregion
    #region Getters and Setters

    // A series of Get functions that return boolean values based on certain conditions
=======
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
    public bool isTargetType() {
        return m_pointerType == pointerType.Target;
    }
    public bool isTeleportType() {
        return m_pointerType == pointerType.Teleport;
    }
    public bool isSetTargetType() {
        return m_pointerType == pointerType.Set_Target;
    }
<<<<<<< HEAD

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
=======
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
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
<<<<<<< HEAD
    // We can also set our laser type here, if we wish to change the laser type programmatically
=======

    public void SetTarget(GameObject targetForSet = null) {
        m_raycastTarget = targetForSet;
    }

>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
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
<<<<<<< HEAD
    // A Set function that sets the target for our raycast - used for when our pointer type is `Set_Target`
    public void SetTarget(GameObject targetForSet = null) {
        m_raycastTarget = targetForSet;
    }

    // A series of getters and setters pertaining to the line renderer itself
=======


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
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
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
<<<<<<< HEAD
    public void LineOff(OVRInput.Controller c) {
        if (c == m_grabber.OVRController) m_LineRenderer.enabled = (!m_debugMode) ? false : true;
    }
    public void LineOn(OVRInput.Controller c) {
        if (c == m_grabber.OVRController) m_LineRenderer.enabled = true;
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
=======
>>>>>>> c82efec4878c9490084e46fa0d09d909a2222c5f
}