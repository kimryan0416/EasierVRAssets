using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardOverlay : MonoBehaviour
{

    public TextMeshProUGUI overlay, caret;
    public GameObject background;
    private KeyboardController m_KeyboardController;
    private KeyboardInput m_Input;
    private KeyboardKey m_Key = null;
    private List<string> characters = new List<string>();
    private float fontSize;
    private bool debugToggle = false;
    TMP_TextInfo textInfo = null;

    // Start is called before the first frame update
    void Awake()
    {
        HideOverlay();
    }

    private IEnumerator CustomUpdate() {
        while(true) {
            textInfo = m_Input.input.textInfo;
            fontSize = m_KeyboardController.fontSize;
            overlay.fontSize = fontSize * 0.8f;
            caret.fontSize = fontSize * 0.8f;
            UpdateHeight(fontSize);

            if (textInfo == null) {
                if (debugToggle) {
                    ShowOverlay();
                    overlay.text = "TXTINF";
                } else {
                    HideOverlay();
                }
                yield return null;
                continue;
            }
            
            m_Key = m_KeyboardController.GetCurrentKey();
            if (m_Key == null) {
                if (debugToggle) {
                    ShowOverlay();
                    overlay.text = "NLLKY";
                } else {
                    HideOverlay();
                }
                characters = new List<string>();
                yield return null;
                continue;
            }

            if (textInfo.characterCount > 0) {
                PositionOverlay();
            }

            string overlayText = "", caretText = "";
            characters = m_Key.GetAllCharacters();
            int currentCharIndex = m_Key.GetKeyIndex();
            if (characters.Count == 1 && characters[0] == " ") {
                overlayText = "<i>space</i>";
                caretText = "<i>space</i>";
            } else {
                for (int i = 0; i < characters.Count; i++) {
                    overlayText += characters[i];
                    if (i == currentCharIndex) {    caretText += "<mark=#F1F1F1>" + characters[i] + "</mark>"; } 
                    else {  caretText += characters[i];    }
                }
            }
            PrintOverlay(overlayText, caretText);

            ShowOverlay();
            yield return null;
        }
    }
    private void PositionOverlay() {
        TMP_CharacterInfo cInfo = textInfo.characterInfo[textInfo.characterCount-1];
        float meanline = cInfo.baseLine + cInfo.fontAsset.faceInfo.meanLine * cInfo.scale;
        float capHeight = cInfo.baseLine + cInfo.fontAsset.faceInfo.capLine * cInfo.scale;
        Vector3 origin = new Vector3(cInfo.topLeft.x, meanline + fontSize, 0);
        this.GetComponent<RectTransform>().localPosition = origin;
    }
    private void PrintOverlay(string overlayText, string caretText) {
        overlay.text = overlayText;
        caret.text = caretText;
        
    }
    private void HideOverlay() {
        overlay.gameObject.SetActive(false);
        caret.gameObject.SetActive(false);
        background.SetActive(false);
    }
    private void ShowOverlay() {
        background.SetActive(true);
        caret.gameObject.SetActive(true);
        overlay.gameObject.SetActive(true);
    }
    private void UpdateHeight(float h) {
        //background.GetComponent<RectTransform>().sizeDelta = new Vector2(background.GetComponent<RectTransform>().sizeDelta.x, h);
        caret.GetComponent<RectTransform>().sizeDelta = new Vector2(caret.GetComponent<RectTransform>().sizeDelta.x, h);
        overlay.GetComponent<RectTransform>().sizeDelta = new Vector2(overlay.GetComponent<RectTransform>().sizeDelta.x, h);
    }
    private void UpdateWidth() {
        //TMP_LineInfo lineInfo = textInfo.lineInfo[0];
        //float ascender = lineInfo.ascender;
        //float descender = lineInfo.descender;
        //Vector3 bottomLeft = transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.firstCharacterIndex].bottomLeft.x, descender, 0));
        //Vector3 bottomRight = transform.TransformPoint(new Vector3(textInfo.characterInfo[lineInfo.lastCharacterIndex].topRight.x, descender, 0));
        //float newWidth = Mathf.Abs(bottomRight.x - bottomLeft.x);
        //background.GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, background.GetComponent<RectTransform>().sizeDelta.y);
        //caret.GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, caret.GetComponent<RectTransform>().sizeDelta.y);
        //overlay.GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, overlay.GetComponent<RectTransform>().sizeDelta.y);
        Bounds textBounds = m_Input.input.textBounds;
        float newWidth = 2 * textBounds.extents.x;
        background.GetComponent<RectTransform>().transform.localScale = new Vector3(newWidth, background.GetComponent<RectTransform>().transform.localScale.y, background.GetComponent<RectTransform>().transform.localScale.z);
        //background.GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, background.GetComponent<RectTransform>().sizeDelta.y);

    }

    public void Initialize(KeyboardController parent, KeyboardInput input) {
        m_KeyboardController = parent;
        m_Input = input;
        debugToggle = m_KeyboardController.debugToggle;
        StartCoroutine(CustomUpdate());
        return;
    }
}
