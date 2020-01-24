using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardInput : MonoBehaviour
{

    #region External References
    public TextMeshProUGUI input, caret;
    public KeyboardOverlay overlayParent;
    private KeyboardController m_KeyboardController;
    #endregion

    #region Public Variables
    [TextArea]
    public string text = "Deactivated";
    public float blinkRate = 0.3f;
    #endregion

    #region Private Variables
    private bool active = false;
    private float fontSize = 0.025f;
    private string textBackup;
    private string defaultScaffoldCharacter = "_";
    private string scaffoldCharacter = "";
    #endregion

    private void Awake()
    {
        input.text = text + scaffoldCharacter;
        textBackup = input.text;
    }
    private void Update() {
        input.text = text + scaffoldCharacter;
        input.fontSize = fontSize;
        caret.fontSize = fontSize;
        if (!active) return;
        if (scaffoldCharacter.Length == 0 || scaffoldCharacter == " ") {
            caret.text = text + "<mark=#FF00FF>" + defaultScaffoldCharacter + "</mark>";
        } else {
            caret.text = text + "<mark=#FF00FF>" + scaffoldCharacter + "</mark>";
        }
    }

    private IEnumerator Blink() {
        while(true) {
            caret.enabled = !caret.enabled;
            yield return new WaitForSeconds(blinkRate);
        }
    }

    public void Initialize(KeyboardController parent, float size) {
        m_KeyboardController = parent;
        fontSize = size;
        SetText("");
    }

    public void Activate() {
        if (!active) {
            ResetScaffold();
            StartCoroutine("Blink");
            active = true;
        }
    }
    public void Deactivate() {
        if (active) {
            ResetScaffold();
            StopCoroutine("Blink");
            caret.enabled = false;
            active = false;
        }
    }
    public void UpdateScaffold(string s) {
        scaffoldCharacter = s;
    }
    public void ResetScaffold() {
        scaffoldCharacter = "";
    }
    public void AddScaffold(bool reset) {
        text += scaffoldCharacter;
        if (reset) ResetScaffold();
    }
    public void AddSpace() {
        text += " ";
        ResetScaffold();
    }
    public string GetScaffold() {
        return scaffoldCharacter;
    }
    public void DeleteCharacter() {
        text = text.Substring(0,text.Length-1);
        ResetScaffold();
    }
    public void SetText(string t) {
        text = t;
        ResetScaffold();
    }
}
