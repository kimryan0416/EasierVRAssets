using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardTypeController : MonoBehaviour
{
    [SerializeField]
    private KeyboardKey[] m_keys = new KeyboardKey[9];
    public KeyboardKey[] keys 
    {
        get {   return m_keys;  }
        set {   m_keys = value; }
    }
    [SerializeField]
    private int m_defaultIndex = 0;
    public int defaultIndex 
    {
        get {   return m_defaultIndex;  }
        set {   m_defaultIndex = value; }
    }
    [SerializeField]
    private List<Vector4> m_thumbstickAngleMapping = new List<Vector4>();
    public List<Vector4> thumbstickAngleMapping
    {
        get {   return m_thumbstickAngleMapping;    }
        set {   m_thumbstickAngleMapping = value;   }
    }
}
