using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(LineRenderer))]
public class CustomPointer_Original: MonoBehaviour
{   

    [SerializeField]
    [Tooltip("Should the pointer start from the getgo, or should it wait for an Init call?")]
    private bool m_shouldStart = true;
    [SerializeField]
    [Tooltip("Reference to grabber, if this requires a grabber to work")]
    private CustomGrabber m_grabber = null;

    // NOT SERIALIZED
    [Tooltip("Reference to LineRenderer")]
    private LineRenderer m_LineRenderer;
    public LineRenderer lineRenderer {
        get {   return m_LineRenderer;  }
        set {   m_LineRenderer = value; }
    }

    private enum pointerType {
        Target,
        Teleport,
        Set_Target
    }
    [SerializeField]
    [Tooltip("The type of pointer you wish to work with")]
    private pointerType m_pointerType;
    
    private enum laserType {
        Laser,
        Parabolic
    }
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
    private GameObject m_raycastTarget = null;
    public GameObject raycastTarget {
        get {   return m_raycastTarget;     }
        set {   m_raycastTarget = value;    }
    }

    [SerializeField]
    [Tooltip("The origin point from where the raycast should be performed")]
    private Transform m_raycastOrigin;
    public Transform raycastOrigin {
        get {   return m_raycastOrigin;     }
        set {   m_raycastOrigin = value;    }
    }

    [SerializeField]
    [Tooltip("The distance the raycast is permitted to travel - default = 100 meters")]
    private float m_raycastDistance = 100f;
    public float raycastDistance {
        get {   return m_raycastDistance;   }
        set {   
            m_raycastDistance = value;  
            m_raycastDownwardDistance = value * 2f;
            ResizeVolumeDetectors();
        }
    }
    // NOT SERIALIZED
    [Tooltip("The distance the downward raycast is permitted to travel - default = 10m")]
    private float m_raycastDownwardDistance;
    public float raycastDownwardDistance {
        get {   return m_raycastDownwardDistance;   }
        set {   m_raycastDownwardDistance = value;  }
    }
    // NOT SERIALIZED
    [Tooltip("bool for detecting if the raycast is actually hitting anything")]
    private bool m_raycastDownwardHit = false;
    public bool raycastDownwardHit {
        get {   return m_raycastDownwardHit;    }
    }

    // NOT SERIALIZED
    [Tooltip("the list of points the line renderer will render - must be converted into Vector3[] to use properly")]
    private List<Vector3> m_linePoints = new List<Vector3>();

    [SerializeField]
    [Tooltip("The color the line must be rendered with")]
    private Color m_lineColor = new Color32(255, 255, 0, 255);
    public Color lineColor {
        get {   return m_lineColor;     }
        set {   m_lineColor = value;    }
    }

    [SerializeField]
    [Tooltip("The detection of a raycast target can only be performed when the pointer is activated")]
    private bool m_detectOnlyWhenLineSet = false;
    public bool detectOnlyWhenLineSet {
        get {   return m_detectOnlyWhenLineSet;   }
        set {   m_detectOnlyWhenLineSet = value;  }
    }

    [SerializeField]
    [Tooltip("The number of points the line renderer will use when printing the line")]
    private int m_numPoints = 30;
    public int numPoints {
        get {   return m_numPoints; }
        set {   m_numPoints = value;}
    }

    // NOT SERIALIZED
    [Tooltip("The location where the player must teleport to - only important to locomotion")]
    private Vector3 m_locomotionPosition = Vector3.zero;
    public Vector3 locomotionPosition {
        get {   return m_locomotionPosition;    }
        set {   m_locomotionPosition = value;   }
    }

    [SerializeField]
    [Tooltip("Reference to the hover cursor prefab")]
    private HoverCursor m_hoverCursor;
    private HoverCursor m_instantiatedHoverCursor;
    public HoverCursor instantiatedHoverCursor {
        get {   return m_instantiatedHoverCursor;   }
        set {   
            if (m_instantiatedHoverCursor != null) m_instantiatedHoverCursor.Relieve();
            m_instantiatedHoverCursor = value;
        }
    }

    private enum longDistanceDetectionType {
        None,
        Volumetric
    }
    [SerializeField]
    [Tooltip("For long-distance detection, if we want some kind of long-distance object detection")]
    private longDistanceDetectionType m_longDistanceDetectionType = longDistanceDetectionType.None;
    [SerializeField]
    [Tooltip("Transform gameObject used for volume-based detection")]
    private List<Transform> m_volumeDetectors = new List<Transform>();

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
        set {   SetDebugMode(value);    }
    }

    [SerializeField]
    [Tooltip("FOR DEBUGGING")]
    private GameObject m_XYZ;

    // NOT SERIALIZED
    private bool m_isActive = false;
    public bool isActive {
        get {   return m_isActive;  }
        set {   m_isActive = value; }
    }
    
    // NOT SERIALIZED
    private int layerMaskHover, layerMaskLocomotion, combinedLayerMask;
    private List<int> m_layerMasks = new List<int>();

    private void Start() {
        if (m_shouldStart) Init(true);
    }

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

    private void Update() {
        if (!m_isActive) return;

        m_linePoints = GetPoints();

        if (m_detectOnlyWhenLineSet) {    m_raycastTarget = (m_LineRenderer.enabled) ? m_raycastTarget : null;    }

        if (m_pointerType != pointerType.Teleport && m_hoverCursor != null) {
            if (m_raycastTarget != null && m_raycastTarget.gameObject.layer != layerMaskHover) {
                if (instantiatedHoverCursor != null) {
                    if (!GameObject.ReferenceEquals(instantiatedHoverCursor.target, m_raycastTarget)) {
                        instantiatedHoverCursor.Relieve();
                        instantiatedHoverCursor = Instantiate(m_hoverCursor,Vector3.zero,Quaternion.identity);
                        instantiatedHoverCursor.Init(m_raycastTarget.transform, m_lineColor);
                    }
                } else {
                    instantiatedHoverCursor = Instantiate(m_hoverCursor,Vector3.zero,Quaternion.identity);
                    instantiatedHoverCursor.Init(m_raycastTarget.transform, m_lineColor);
                }
            } 
            else if (instantiatedHoverCursor != null) {
                instantiatedHoverCursor.Relieve();
                instantiatedHoverCursor = null;
            }
        } 

        if (m_LineRenderer.enabled) {   
            m_LineRenderer.material.SetColor("_Color", m_lineColor);
            m_LineRenderer.positionCount = m_numPoints;
            Vector3[] points = m_linePoints.ToArray();
            m_LineRenderer.SetPositions(points); 
        }
    }

    private List<Vector3> GetPoints() {
        List<Vector3> points = new List<Vector3>();
        Vector3 destination;

        switch(m_pointerType) {
            case(pointerType.Target):
                m_pointerDest = CheckRaycastForward(layerMaskHover);
                destination = CheckRaycastDownward(m_pointerDest);
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
                m_pointerDest = (m_grabber != null && m_grabber.grabbedObject != null) ? CheckRaycastForward(combinedLayerMask) : CheckRaycastForward(layerMaskLocomotion);
                destination = CheckRaycastDownward(m_pointerDest);
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
                destination = (m_raycastTarget != null) ? m_raycastTarget.transform.position : m_raycastOrigin.position;
                points = BezierCurves.DetermineLinearCurve(m_numPoints,m_raycastOrigin.position,destination,0);
                break;
        }

        return points;
    }
 
    private Vector3 CheckRaycastForward(int layersToAvoid = 1 << 3) {
        RaycastHit rayHit;
        Vector3 returnPoint = m_raycastOrigin.position + m_raycastOrigin.forward * m_raycastDistance;
        if (Physics.Raycast(m_raycastOrigin.position, m_raycastOrigin.TransformDirection(Vector3.forward), out rayHit, m_raycastDistance, layersToAvoid)) {
            // Something in front of it
            m_raycastTarget = rayHit.transform.gameObject;
            returnPoint = rayHit.point;
        } else {
            // Now, this is actually crucial.
            // If our raycast hit something, then we don't actually need to worry about this step... but sometimes, our raycast is trying to hit something
            //      far away, and our pointer isn't really having a good time.
            // In these situations, we need volume-based detection
            // Now, here's the conundrum: we can do volume-based detection, but the hard part is deciding how to select objects
            // For example, let's say there's two objets picked up by my volume-based detection system. 
            //      One is closer to the line but far away from the end of the line. The other is closer to the end of the line but is far from the line. 
            // Anyone might argue "It makes more sense for the object closer to the line itself to be favored"...
            //      But you really know for certain. You could still be trying to pick up that object that's far away even if there are objects closer to your line in the way.
            // Here's how I see it:
            //      There's a weighing that's going on. There are two factors in this weighing: 1) distance between the object and the line, and 2) distance between the object and the endpoint
            //      In other words, objects that are closer to the player (and therefore farther from the endpoint) are weighed less than those closer to the endpoint
            //      So even if the closer object is for sure closer to the line than the other object, the other object is favored over the closer object because the further-away object is closer to the end.
            // The weighing is doen as such:
            //      weighed value = distance from line / distance from endpoint.
            //      distance from line = sin(angle between raycast origin and object) * distance between raycast origin and object, since sin(angle) = furthest / hypotenuse
            //          angle = Vector3.Angle((object's position - raycast origin position), m_raycastOrigin.TransformDirection(Vector3.forward))
            //          distance = Vector3.Distance(object's position, raycast origin position)
            //      distance from endpoint = Vector3.Distance(object's position, returnPoint)
            switch(m_longDistanceDetectionType) {
                case(longDistanceDetectionType.Volumetric):
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

    public bool isTargetType() {
        return m_pointerType == pointerType.Target;
    }
    public bool isTeleportType() {
        return m_pointerType == pointerType.Teleport;
    }
    public bool isSetTargetType() {
        return m_pointerType == pointerType.Set_Target;
    }
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

    public void SetTarget(GameObject targetForSet = null) {
        m_raycastTarget = targetForSet;
    }

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

    private void ResizeVolumeDetectors() {
        float zSize = m_raycastDistance / m_volumeDetectors.Count;
        for(int i = 0; i < m_volumeDetectors.Count; i++) {
            Transform t = m_volumeDetectors[i];
            t.transform.localScale = new Vector3(t.transform.localScale.x, t.transform.localScale.y, zSize);
            t.transform.localPosition = new Vector3(0,0, (i*zSize)+(zSize/2));
        }
    }

    private void SetDebugMode(bool dMode) {
        m_debugMode = dMode;
        //m_XYZ.SetActive(m_debugMode);
        foreach(Transform t in m_volumeDetectors) {
            t.GetComponent<Renderer>().enabled = dMode;
        }
    }
}