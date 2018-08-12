using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextAnimate : MonoBehaviour {

    private Text dialogText;

    private void Start() {
        dialogText = GetComponent<Text>();
        dialogText.text = "";
    }

    void StartAnimate(string sequence) {
        StartCoroutine(AnimateString(sequence));
    }

    IEnumerator AnimateString(string sequence) {
        dialogText.text = "";
        foreach(char character in sequence.ToCharArray()) {
            dialogText.text += character;
            yield return null;
        }
    }
}
