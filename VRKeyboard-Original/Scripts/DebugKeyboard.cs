using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugKeyboard : MonoBehaviour
{
    public TextMeshProUGUI textToDebug;
    private TextMeshProUGUI debugTextBox;
    private KeyboardController m_KeyboardController;
    private Transform m_Transform;
    private float fontSize;
    private IEnumerator LastCharInfoCoroutine;
    private bool isActive = false;

    TMP_TextInfo textInfo;

    private void Start()
    {
        textInfo = textToDebug.textInfo;
        debugTextBox = this.GetComponent<TextMeshProUGUI>();
        m_Transform = this.GetComponent<Transform>();
        fontSize = this.GetComponent<KeyboardController>().fontSize;
        LastCharInfoCoroutine = LastCharInfo();
    }
    private void Update() {
        /*
        if (m_KeyboardController == null) {
            return;
        }
        //debugTextBox.text = m_KeyboardController.debugToggle.ToString();
        if (m_KeyboardController.debugToggle && !isActive) {
            Activate();
        }
        else if (!m_KeyboardController.debugToggle && isActive) {
            Deactivate();
        }
        */
    }
    private IEnumerator LastCharInfo() {
        while(true) {
            if (!m_KeyboardController.debugToggle) {
                debugTextBox.text = "";
                yield return null;
            }
            if (textInfo.characterCount > 0) {
                TMP_CharacterInfo cInfo = textInfo.characterInfo[textInfo.characterCount-1];
                Vector3 origin = m_Transform.TransformPoint(cInfo.origin, cInfo.baseLine, 0);
                Vector3 aboveOrigin = new Vector3(origin.x, origin.y + fontSize, 0);
                //float capHeight = cInfo.baseLine + cInfo.fontAsset.faceInfo.capLine * cInfo.scale;
                //Vector3 bottomLeft = m_Transform.TransformPoint(cInfo.bottomLeft);
                //Vector3 topLeft = m_Transform.TransformPoint(new Vector3(cInfo.topLeft.x, cInfo.topLeft.y, 0));
                //float height = topLeft.y - bottomLeft.y;
                debugTextBox.text = origin.ToString() + " | " + aboveOrigin.ToString();
            } else {
                debugTextBox.text = "NO CHARACTERS";
            }
            yield return null;
        }
    }

    public void Initialize(KeyboardController parent) {
        m_KeyboardController = parent;
        StartCoroutine(LastCharInfoCoroutine);
        return;
    }
    /*
    public void Activate() {
        StartCoroutine(LastCharInfoCoroutine);
        isActive = true;
    }
    public void Deactivate() {
        StopCoroutine(LastCharInfoCoroutine);
        isActive = false;
    }
    */
}
