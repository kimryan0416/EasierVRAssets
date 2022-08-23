using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_RoundedBoxCollider : MonoBehaviour
{
    // --------------------------------------------
    [Header("Global Collider Settings")]
    // --------------------------------------------
    #region Trigger Settings
        [SerializeField, Tooltip("The global trigger property for all colliders.")]
        private bool m_isTrigger = false;
        public bool isTrigger {
            get { return m_isTrigger; }
            set { SetTrigger(value); }
        }
        private bool m_prevTrigger;
    #endregion
    // --------------------------------------------
    #region Material Settings
        [SerializeField, Tooltip("The global physics material for all colliders.")]
        private PhysicMaterial m_material;
        public PhysicMaterial material {
            get {  return m_material; }
            set { SetMaterial(value); }
        }
        private PhysicMaterial m_prevMaterial;
    #endregion

    // --------------------------------------------
    [Header("Real-World Dimensions")]
    // --------------------------------------------
    #region X-coordinates
        [SerializeField, Tooltip("World Width X. Expected to be 2x of the radius. If smaller, minimum world width x is re-adjusted to 2*radius")]
        private float m_x = 1f;
        public float x {
            get { return m_x; }
            set { UpdateX(value); }
        }
        private float m_prevX = 1f;
    #endregion
    // --------------------------------------------
    #region Y-Coordinates
        [SerializeField, Tooltip("World Height Y. Expected to be 2x of the radius. If smaller, minimum world height y is re-adjusted to 2*radius")]
        private float m_y = 1f;
        public float y {
            get { return m_y; }
            set { UpdateY(value); }
        }
        private float m_prevY = 1f;
    #endregion
    // --------------------------------------------
    #region Z-Coordinates
        [SerializeField, Tooltip("World Length Z. Expected to be 2x of the radius. If smaller, minimum world length z is re-adjusted to 2*radius")]
        private float m_z = 1f;
        public float z {
            get { return m_z; }
            set { UpdateZ(value); }
        }
        private float m_prevZ = 1f;
    #endregion
    // -------------------------------------------- 
    #region Radius Coordinates
        [SerializeField, Tooltip("Radius of the rounded edges.")]
        private float m_radius = 0.25f;
        public float radius {
            get { return m_radius; }
            set { UpdateRadius(value); }
        }
        private float m_prevRadius = 0.25f;
    #endregion
    // --------------------------------------------
    #region Adjusted Coordinate Values
        private float adjustedX = 1f, innerX = 0.5f;
        private float adjustedY = 1f, innerY = 0.5f;
        private float adjustedZ = 1f, innerZ = 0.5f;
    #endregion

    // --------------------------------------------
    #region Capsule Collider Settings
        public class CapsuleTemplate {
            public Vector3 center;
            public int direction;
            public CapsuleTemplate(Vector3 center, int direction) {
                this.center = center;
                this.direction = direction;
            }
            public CapsuleTemplate(float x, float y, float z, int direction) {
                this.center = new Vector3(x,y,z);
                this.direction = direction;
            }
        }
        private List<CapsuleTemplate> capsuleTemplates = new List<CapsuleTemplate>() {
            new CapsuleTemplate(0.5f, 0f, 0.5f, 1),
            new CapsuleTemplate(0.5f, 0f, -0.5f, 1),
            new CapsuleTemplate(-0.5f, 0f, 0.5f, 1),
            new CapsuleTemplate(-0.5f, 0f, -0.5f, 1),

            new CapsuleTemplate(0f, 0.5f, 0.5f, 0),
            new CapsuleTemplate(0f, -0.5f, 0.5f, 0),
            new CapsuleTemplate(0f, 0.5f, -0.5f, 0),
            new CapsuleTemplate(0f, -0.5f, -0.5f, 0),

            new CapsuleTemplate(0.5f, 0.5f, 0f, 2),
            new CapsuleTemplate(0.5f, -0.5f, 0f, 2),
            new CapsuleTemplate(-0.5f, 0.5f, 0f, 2),
            new CapsuleTemplate(-0.5f, -0.5f, 0f, 2)
        };
        private List<CapsuleCollider> capsules = new List<CapsuleCollider>();
    #endregion
    // --------------------------------------------
    #region Box Collider Settings
        private BoxCollider xBox;
        private BoxCollider yBox;
        private BoxCollider zBox;
    #endregion
    // --------------------------------------------
    #region All Collider Settings
        private List<Collider> m_colliders = new List<Collider>();
        public List<Collider> colliders {
            get { return m_colliders; }
            set {}
        }
    #endregion

    // --------------------------------------------
    // --------------------------------------------
    
    private bool loaded = false;

    // --------------------------------------------
    // --------------------------------------------

    void Awake() {

        // Creating box colliders
        xBox = gameObject.AddComponent<BoxCollider>();
        xBox.size = new Vector3(2f, 1f, 1f);
        xBox.isTrigger = m_isTrigger;
        xBox.material = m_material;
        yBox = gameObject.AddComponent<BoxCollider>();
        yBox.size = new Vector3(1f, 2f, 1f);
        yBox.isTrigger = m_isTrigger;
        yBox.material = m_material;
        zBox = gameObject.AddComponent<BoxCollider>();
        zBox.size = new Vector3(1f, 1f, 2f);
        zBox.isTrigger = m_isTrigger;
        zBox.material = m_material;

        // Adding box colliders to lists
        m_colliders.Add(xBox);
        m_colliders.Add(yBox);
        m_colliders.Add(zBox);

        // Creating capsule colliders
        foreach(CapsuleTemplate template in capsuleTemplates) {
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.center = template.center;
            col.radius = m_radius;
            col.height = 2f;
            col.direction = template.direction;
            col.isTrigger = m_isTrigger;
            col.material = m_material;

            colliders.Add(col);
            capsules.Add(col);
        }
        
        /*
        foreach(Collider col in colliders) {
            col.isTrigger = isTrigger;
            col.material = material;
        }  
        capsules = GetComponentsInChildren<CapsuleCollider>();

        /*
        foreach(BoxCollider col in boxes) {
            col.isTrigger = isTrigger;
            col.material = material;
            Vector3 size = col.size;
            if (size.x > size.y && size.x > size.z) xBox = col;
            if (size.y > size.x && size.y > size.z) yBox = col;
            if (size.z > size.x && size.z > size.y) zBox = col;
        }
        */

        /*
        foreach(CapsuleCollider col in capsules) {
            col.isTrigger = isTrigger;
            col.material = material;
        }
        */

        UpdateColliderDimensions();
        SetTrigger();
        SetMaterial();
    }

    void Start() {
        loaded = true;
    }

    void UpdateX(float newX) {
        m_x = newX;
        UpdateColliderDimensions();
    }
    void UpdateY(float newY) {
        m_y = newY;
        UpdateColliderDimensions();
    }
    void UpdateZ(float newZ) {
        m_z = newZ;
        UpdateColliderDimensions();
    }
    void UpdateRadius(float newRadius) {
        m_radius = newRadius;
        UpdateColliderDimensions();
    }
    void UpdateColliderDimensions() {
        MakeCalculations();
        MakeAdjustments();
        SetPreviousValues();
    }

    void MakeCalculations() {
        CalculateX();
        CalculateY();
        CalculateZ();  
    }
    void CalculateX() {
        adjustedX = (m_x < 4*m_radius) ? 4*m_radius : m_x;
        innerX = adjustedX - (2*m_radius);
    }
    void CalculateY() {
        adjustedY = (m_y < 4*m_radius) ? 4*m_radius : m_y;
        innerY = adjustedY - (2*m_radius);
    }
    void CalculateZ() {
        adjustedZ = (m_z < 4*m_radius) ? 4*m_radius : m_z;
        innerZ = adjustedZ - (2*m_radius);
    }

    void MakeAdjustments() {
        foreach(CapsuleCollider col in capsules) {
            col.radius = m_radius;
            switch(col.direction) {
                case 0:
                    // x
                    col.height = adjustedX;
                    col.center = new Vector3(0f, Mathf.Sign(col.center.y)*(adjustedY*0.5f-m_radius), Mathf.Sign(col.center.z)*(adjustedZ*0.5f-m_radius));
                    break;
                case 1:
                    // y
                    col.height = adjustedY;
                    col.center = new Vector3(Mathf.Sign(col.center.x)*(adjustedX*0.5f-m_radius), 0f, Mathf.Sign(col.center.z)*(adjustedZ*0.5f-m_radius));
                    break;
                case 2:
                    // z
                    col.height = adjustedZ;
                    col.center = new Vector3(Mathf.Sign(col.center.x)*(adjustedX*0.5f-m_radius), Mathf.Sign(col.center.y)*(adjustedY*0.5f-m_radius), 0f);
                    break;
            }
        }
        xBox.size = new Vector3(adjustedX, innerY, innerZ);
        yBox.size = new Vector3(innerX, adjustedY, innerZ);
        zBox.size = new Vector3(innerX, innerY, adjustedZ);
    }

    void SetPreviousValues() {
        m_prevX = m_x;
        m_prevY = m_y;
        m_prevZ = m_z;
        m_prevRadius = m_radius;
    }

    public void SetTrigger(bool newTrigger) {
        m_isTrigger = newTrigger;
        SetTrigger();
    }
    public void SetTrigger() {
        foreach(Collider col in colliders) {
            col.isTrigger = m_isTrigger;
        }
        m_prevTrigger = m_isTrigger;
    }

    public void SetMaterial(PhysicMaterial newMaterial) {
        m_material = newMaterial;
        SetMaterial();
    }
    public void SetMaterial() {
        foreach(Collider col in colliders) {
            col.material = m_material;
        }
        m_prevMaterial = m_material;
    }

    public void EnableCollisions() {
        foreach(Collider col in colliders) {
            col.enabled = true;
        }
    }
    public void DisableCollisions() {
        foreach(Collider col in colliders) {
            col.enabled = false;
        }
    }

    void OnValidate() {
        if (!loaded) return;
        if (m_prevX != m_x || m_prevY != m_y || m_prevZ != m_z || m_prevRadius != m_radius) UpdateColliderDimensions();
        if (m_prevTrigger != m_isTrigger ) SetTrigger();
        if (m_prevMaterial != m_material) SetMaterial();
    }
}
