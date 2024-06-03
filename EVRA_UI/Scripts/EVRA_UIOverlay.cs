using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EVRA_UIOverlay : MonoBehaviour
{
    /// <summary>
    /// Basic idea: we want to make any and all UI elements in this canvas 
    /// (or honestly, any Graphics element that is a child of this GameObject)
    /// appear on top of everything else in the world? Then apply this to that
    /// parent GameObject. We are effectively setting the z-buffer test and 
    /// forcing the graphics elements to all render above all other GameObjects.
    /// Source: Julien-Lynge
    /// URL: https://discussions.unity.com/t/world-space-canvas-on-top-of-everything/128165/3
    /// </summary>

    // We will populate this in Awake(). It refers to all children elements that derive from "Graphics"
    [SerializeField] Graphic[] uiElementsToApplyTo;
    [SerializeField] Renderer[] renderersToApplyTo;

    // We need to set this shader test mode - it's the equivalent of testing the z-buffer in OpenGL
    private const string shaderTestMode = "unity_GUIZTestMode";

    // If we want to try other effects... .but generally don't touch this.
    [SerializeField] UnityEngine.Rendering.CompareFunction desiredUIComparison = UnityEngine.Rendering.CompareFunction.Always;

    // Allows us to re-use materials
    private Dictionary<Material, Material> materialMappings = new Dictionary<Material, Material>();

    void Start() {
        if (uiElementsToApplyTo.Length == 0) {
            uiElementsToApplyTo = gameObject.GetComponentsInChildren<Graphic>();
        }
        if (renderersToApplyTo.Length == 0) {
            renderersToApplyTo = gameObject.GetComponentsInChildren<Renderer>();
        }

        foreach (var graphic in uiElementsToApplyTo) {
            Material material = graphic.materialForRendering;
            if (material == null) continue;
            if (!materialMappings.TryGetValue(material, out Material materialCopy)) {
                materialCopy = new Material(material);
                materialMappings.Add(material, materialCopy);
            }
            materialCopy.SetInt(shaderTestMode, (int)desiredUIComparison);
            graphic.material = materialCopy;
        }

        foreach (var renderer in renderersToApplyTo) {
            Material material = renderer.materials[0];
            if (material == null) continue;
            if (!materialMappings.TryGetValue(material, out Material materialCopy)) {
                materialCopy = new Material(material);
                materialMappings.Add(material, materialCopy);
            }
            materialCopy.SetInt(shaderTestMode, (int)desiredUIComparison);
            renderer.materials[0] = materialCopy;
        }
    }
}
