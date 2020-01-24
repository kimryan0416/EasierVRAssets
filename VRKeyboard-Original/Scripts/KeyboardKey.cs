using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyboardKey : MonoBehaviour
{
    public List<string> characters = new List<string>();
    public GameObject highlightObject, selectedObject;
    public TextMeshProUGUI keyOverlay;
    public bool isSpace = false;
    private Collider m_Collider;
    private bool keyStatus = false;
    private int charactersIndex = -1;
    private string curCharacter = "";

    private void Awake() {
        m_Collider = this.GetComponent<Collider>();
        PrintOverlay();
        StartCoroutine(LoopThroughCharacters());
    }
    private IEnumerator LoopThroughCharacters() {
        while(true) {
            if (charactersIndex > -1) {
                curCharacter = characters[charactersIndex];
            }
            else {
                curCharacter = "";
            }
            yield return true;
        }
    }

    public void CollisionToggle(bool c) {
        m_Collider.enabled = c;
        return;
    }
    public void ResetKey() {
        highlightObject.SetActive(false);
        selectedObject.SetActive(false);
        ResetCharactersIndex();
        return;
    }
    public void HighlightKey() {
        highlightObject.SetActive(true);
        selectedObject.SetActive(false);
        return;
    }
    public void SelectKey() {
        highlightObject.SetActive(false);
        selectedObject.SetActive(true);
        return;
    }
    public void NextCharactersIndex() {
        if (charactersIndex == -1) charactersIndex = 0;
        else charactersIndex = (charactersIndex == characters.Count - 1) ? 0 : charactersIndex + 1;
        return;
    }
    public void ResetCharactersIndex() {
        charactersIndex = -1;
        return;
    }
    public string GetCurrentCharacter() {
        if (charactersIndex > -1) curCharacter = characters[charactersIndex];
        else curCharacter = "";
        return curCharacter;
    }
    public List<string> GetAllCharacters() {
        return characters;
    }
    public void PrintOverlay() {
        string ov = "";
        if (characters.Count == 1 && characters[0] == " ") {    ov = "space";   }
        else {
            foreach(string str in characters) {
                ov += str;
            }
        }
        keyOverlay.text = ov;
        return;
    }
    public int GetKeyIndex() {
        return charactersIndex;
    }

    private void OnCollisionEnter(Collider collision) {
        return;
    }
}
