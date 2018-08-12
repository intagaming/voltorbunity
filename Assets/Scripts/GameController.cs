using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour {
    const float frameTime = 0.016715113193576f;

    public int[,] values = new int[5, 5];

    public GameObject dialogBox;
    public GameObject scoreDialogBox;
    public RenderManager renderManager;
    public GameObject[] gameObjectReferences;
    public Sprite[] spriteReferences;
    public PointerController pointerController;

    public bool scoreDialogShowing = false;
    private bool allowScoreDialogHideInput = false;

    private bool scrollingDialogInput;
    private bool allowScrollingDialogInput = true;
    private bool ignoreButtonClick = false;
    private bool showedAdvanceTips = false;

    public enum GameResult {
        None,
        Advance,
        GameOver
    }
    private GameResult gameResult = GameResult.None;

    private bool allowDelid = false;

    private int coins = 0;
    private int level = 1;
    private int newLevel = 1;


    //private AnimatorManager anim;

    void Start() {
        if (!PlayerPrefs.HasKey("collectedCoins")) PlayerPrefs.SetInt("collectedCoins", 0);
        int collectedCoins = PlayerPrefs.GetInt("collectedCoins");
        int i = 5;
        while (collectedCoins > 0) {
            GetGameObjectReference("UpperDigit" + i).GetComponent<SpriteRenderer>().sprite = GetSpriteReference("InfoDigits_" + collectedCoins % 10);
            collectedCoins = collectedCoins / 10;
            i--;
        }
        for (i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                values[i, j] = 1;
            }
        }
        level = 1;
        PlayStartAnimation();
    }

    void PlayStartAnimation() {
        StartCoroutine(StartGameAnimation());
    }

    void Show3OptionsMenu(string menuId) {
        GetGameObjectReference("3OptionsBorder").SetActive(true);
        GetGameObjectReference(menuId).SetActive(true);
        pointerController.setPointerType(PointerController.PointerType.MenuButton);
        pointerController.showPointer(3, 3);
    }

    void Show4OptionsMenu(string menuId) {
        GetGameObjectReference("4OptionsBorder").SetActive(true);
        GetGameObjectReference(menuId).SetActive(true);
        pointerController.setPointerType(PointerController.PointerType.MenuButton);
        pointerController.showPointer(4, 4);
    }

    void HideAllMenus() {
        foreach (Transform child in GameObject.Find("Lower Screen/MenuCanvas").transform) {
            child.gameObject.SetActive(false);
        }
        pointerController.hidePointer();
    }

    public void ButtonClick(GameObject buttonObject) {
        if (ignoreButtonClick) return;
        ignoreButtonClick = true;
        StartCoroutine(ButtonClickAnimatee(buttonObject));
    }

    IEnumerator ButtonClickAnimatee(GameObject buttonObject) {
        Image buttonImage = buttonObject.GetComponent<Image>();
        string buttonName = buttonImage.gameObject.name;

        int count = 0;
        foreach (Transform child in buttonImage.gameObject.transform.parent) {
            if (child.gameObject.name == buttonName) {
                pointerController.setPosition(pointerController.getPosition().y - count);
            } else count++;
        }

        //Copy texture
        Texture texture = buttonImage.sprite.texture;
        RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height);
        Graphics.Blit(texture, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        Texture2D myTexture2D = new Texture2D(texture.width, texture.height);
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        myTexture2D.filterMode = FilterMode.Point;

        buttonImage.sprite = Sprite.Create(myTexture2D, buttonImage.sprite.rect, buttonImage.sprite.pivot);

        Color[] oldPixels = myTexture2D.GetPixels();
        Color[] newPixels = myTexture2D.GetPixels();
        for (int i = 0; i < newPixels.Length; i++) {
            Color pixel = newPixels[i];
            if (pixel.r == 120 / 255.0F && pixel.g == 192 / 255.0F && pixel.b == 176 / 255.0F) {
                newPixels[i] = new Color(248 / 255.0F, 240 / 255.0F, 176 / 255.0F);
            } else if (pixel.r == 80 / 255.0F && pixel.g == 80 / 255.0F && pixel.b == 88 / 255.0F) {
                newPixels[i] = new Color(80 / 255.0F, 104 / 255.0F, 120 / 255.0F);
            }
        }

        myTexture2D.SetPixels(newPixels);
        myTexture2D.Apply();
        for (int i = 0; i < 6; i++) {
            yield return null;
        }
        myTexture2D.SetPixels(oldPixels);
        myTexture2D.Apply();
        for (int i = 0; i < 4; i++) {
            yield return null;
        }
        myTexture2D.SetPixels(newPixels);
        myTexture2D.Apply();
        for (int i = 0; i < 4; i++) {
            yield return null;
        }
        myTexture2D.SetPixels(oldPixels);
        myTexture2D.Apply();
        for (int i = 0; i < 6; i++) {
            yield return null;
        }
        
        GetType().GetMethod(buttonName).Invoke(this, null);
        ignoreButtonClick = false;
    }

    GameObject GetGameObjectReference(string gameObjectName) {
        foreach (GameObject reference in gameObjectReferences) {
            if (reference.name == gameObjectName)
                return reference;
        }
        return null;
    }

    Sprite GetSpriteReference(string spriteName) {
        foreach (Sprite reference in spriteReferences) {
            if (reference.name == spriteName)
                return reference;
        }
        return null;
    }


    // ========== Coroutines ========== 
    IEnumerator StartGameAnimation() {
        Image upperTransitionImage = GameObject.Find("Upper Screen/TransitionCanvas/TransitionImage").GetComponent<Image>();
        Image lowerTransitionImage = GameObject.Find("Lower Screen/TransitionCanvas/TransitionImage").GetComponent<Image>();

        for (int i = 0; i < 2; i++) // 2 frames of Transition0 sprite
            yield return null;
        for (int i = 1; i <= 5; i++) {
            upperTransitionImage.sprite = lowerTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }
        upperTransitionImage.enabled = lowerTransitionImage.enabled = false;

        Image fadeBlackImage = GameObject.Find("Lower Screen/FadeInImage/FadeBlackImage").GetComponent<Image>();
        fadeBlackImage.canvas.sortingLayerName = "Transitions";
        var color = fadeBlackImage.color;
        for (int i = 0; i < 6; i++) {
            color.a = i * 0.09f;
            fadeBlackImage.color = color;
            yield return new WaitForSeconds(frameTime*2);
        }
        if (level == 2 && !showedAdvanceTips) {
            showedAdvanceTips = true;
            List<List<string>> sequencesList = new List<List<string>>();
            List<string> sequence = new List<string>();
            sequence.Add("Advanced to Game Lv. " + level + "!");
            sequencesList.Add(sequence);
            sequence = new List<string>();
            sequence.Add("Congratulations!");
            sequencesList.Add(sequence);
            sequence = new List<string>();
            sequence.Add("You can receive even more Coins\nin the next game!");
            sequencesList.Add(sequence);
            StartCoroutine(MultipleScrollingDialog(sequencesList, StartCoroutineFunc, DelayedStartCoroutineFunc(DialogBoxShow("Play VOLTORB Flip Lv. " + level + "?", Show3OptionsMenu, "MainMenu"), 14)));
        } else if(newLevel < level){
            level = newLevel;
            List<List<string>> sequencesList = new List<List<string>>();
            List<string> sequence = new List<string>();
            sequence.Add("Dropped to Game Lv. " + level + ".");
            sequencesList.Add(sequence);
            StartCoroutine(MultipleScrollingDialog(sequencesList, StartCoroutineFunc, DelayedStartCoroutineFunc(DialogBoxShow("Play VOLTORB Flip Lv. " + level + "?", Show3OptionsMenu, "MainMenu"), 14)));

        } else {
            StartCoroutine(DialogBoxShow("Play VOLTORB Flip Lv. " + level + "?", Show3OptionsMenu, "MainMenu"));
        }
    }

    IEnumerator DelayedStartCoroutineFunc(IEnumerator coroutine, int delay = 0) {
        for(int i = 0; i < delay; i++) { yield return null; }
        StartCoroutine(coroutine);
    }

    IEnumerator DialogBoxShow(string sequence) {
        dialogBox.SetActive(true);
        yield return null;
        Text dialogText = dialogBox.GetComponentInChildren<Text>();
        dialogText.text = "";
        foreach (char character in sequence.ToCharArray()) {
            dialogText.text += character;
            yield return null;
        }
    }

    IEnumerator DialogBoxShow(string sequence, Action callback) {
        dialogBox.SetActive(true);
        yield return null;
        Text dialogText = dialogBox.GetComponentInChildren<Text>();
        dialogText.text = "";
        foreach (char character in sequence.ToCharArray()) {
            dialogText.text += character;
            yield return null;
        }
        if (callback != null) callback();
    }

    IEnumerator DialogBoxShow<T>(string sequence, Action<T> callback, T parameter) {
        dialogBox.SetActive(true);
        yield return null;
        Text dialogText = dialogBox.GetComponentInChildren<Text>();
        dialogText.text = "";
        foreach (char character in sequence.ToCharArray()) {
            dialogText.text += character;
            yield return null;
        }
        if (callback != null) callback(parameter);
    }

    IEnumerator MultipleScrollingDialog(List<List<string>> sequencesList) {
        dialogBox.SetActive(true);
        yield return null;
        Text dialogText = dialogBox.GetComponentInChildren<Text>();
        RectTransform transform = (RectTransform)GetGameObjectReference("DialogText").transform;
        foreach (List<string> sequences in sequencesList) {
            dialogText.text = "";
            transform.offsetMax = new Vector2(transform.offsetMax.x, 0);
            yield return null;
            for (int i = 0; i < sequences.Count; i++) {
                string sequence = sequences[i];
                foreach (char character in sequence.ToCharArray()) {
                    dialogText.text += character;
                    yield return null;
                }
                scrollingDialogInput = false;
                var coroutine = ScrollingDialogAnimation();
                StartCoroutine(coroutine);
                while (!scrollingDialogInput) yield return null;
                StopScrollingDialogAnimation(coroutine);
                if (i < sequences.Count - 1) {
                    for (int j = 0; j < 4; j++) {
                        transform.offsetMax = new Vector2(transform.offsetMax.x, transform.offsetMax.y + 2.6325f);
                        //transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y + 2.6325f);
                        yield return null;
                    }
                }
            }
        }
    }

    IEnumerator MultipleScrollingDialog(List<List<string>> sequencesList, Action callback) {
        dialogBox.SetActive(true);
        yield return null;
        Text dialogText = dialogBox.GetComponentInChildren<Text>();
        RectTransform transform = (RectTransform)GetGameObjectReference("DialogText").transform;
        foreach (List<string> sequences in sequencesList) {
            dialogText.text = "";
            transform.offsetMax = new Vector2(transform.offsetMax.x, 0);
            yield return null;
            for (int i = 0; i < sequences.Count; i++) {
                string sequence = sequences[i];
                foreach (char character in sequence.ToCharArray()) {
                    dialogText.text += character;
                    yield return null;
                }
                scrollingDialogInput = false;
                var coroutine = ScrollingDialogAnimation();
                StartCoroutine(coroutine);
                while (!scrollingDialogInput) yield return null;
                StopScrollingDialogAnimation(coroutine);
                if (i < sequences.Count - 1) {
                    for (int j = 0; j < 4; j++) {
                        transform.offsetMax = new Vector2(transform.offsetMax.x, transform.offsetMax.y + 2.6325f);
                        //transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y + 2.6325f);
                        yield return null;
                    }
                }
            }
        }
        if (callback != null) callback();
    }

    IEnumerator MultipleScrollingDialog<T>(List<List<string>> sequencesList, Action<T> callback, T parameter) {
        dialogBox.SetActive(true);
        yield return null;
        Text dialogText = dialogBox.GetComponentInChildren<Text>();
        RectTransform transform = (RectTransform)GetGameObjectReference("DialogText").transform;
        foreach (List<string> sequences in sequencesList) {
            dialogText.text = "";
            transform.offsetMax = new Vector2(transform.offsetMax.x, 0);
            yield return null;
            for (int i = 0; i < sequences.Count; i++) {
                string sequence = sequences[i];
                foreach (char character in sequence.ToCharArray()) {
                    dialogText.text += character;
                    yield return null;
                }
                scrollingDialogInput = false;
                var coroutine = ScrollingDialogAnimation();
                StartCoroutine(coroutine);
                while (!scrollingDialogInput) yield return null;
                StopScrollingDialogAnimation(coroutine);
                if (i < sequences.Count - 1) {
                    for (int j = 0; j < 4; j++) {
                        transform.offsetMax = new Vector2(transform.offsetMax.x, transform.offsetMax.y + 2.6325f);
                        //transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y + 2.6325f);
                        yield return null;
                    }
                }
            }
        }
        if (callback != null) callback(parameter);
    }

    //IEnumerator MultipleScrollingDialogsChain(List<List<List<string>>> sequencesLists) {
    //    dialogBox.SetActive(true);
    //    Text dialogText = dialogBox.GetComponentInChildren<Text>();
    //    RectTransform transform = (RectTransform)GetGameObjectReference("DialogText").transform;
    //    foreach (List<List<string>> sequencesList in sequencesLists) {
    //        foreach (List<string> sequences in sequencesList) {
    //            dialogText.text = "";
    //            transform.offsetMax = new Vector2(transform.offsetMax.x, 0);
    //            yield return null;
    //            for (int i = 0; i < sequences.Count; i++) {
    //                string sequence = sequences[i];
    //                foreach (char character in sequence.ToCharArray()) {
    //                    dialogText.text += character;
    //                    yield return null;
    //                }
    //                scrollingDialogInput = false;
    //                var coroutine = ScrollingDialogAnimation();
    //                StartCoroutine(coroutine);
    //                while (!scrollingDialogInput) yield return null;
    //                StopScrollingDialogAnimation(coroutine);
    //                if (i < sequences.Count - 1) {
    //                    for (int j = 0; j < 4; j++) {
    //                        transform.offsetMax = new Vector2(transform.offsetMax.x, transform.offsetMax.y + 2.6325f);
    //                        //transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y + 2.6325f);
    //                        yield return null;
    //                    }
    //                }
    //            }
    //        }


    //    }
    //}

    

    IEnumerator ScrollingDialogAnimation() {
        GetGameObjectReference("DialogTriangle").SetActive(true);
        RectTransform transform = (RectTransform)GetGameObjectReference("DialogTriangle").transform;
        transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, 22);
        yield return null;
        bool direction = false;
        while (true) {
            if (direction == false) {
                if (transform.anchoredPosition.y > 20) {
                    transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y - 1);
                } else {
                    direction = true;
                    transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y + 1);
                }
            } else if (direction == true) {
                if (transform.anchoredPosition.y < 22) {
                    transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y + 1);
                } else {
                    direction = false;
                    transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, transform.anchoredPosition.y - 1);
                }
            }
            for (int i = 0; i < 10; i++) {
                yield return null;
            }
        }
    }

    void StopScrollingDialogAnimation(IEnumerator coroutine) {
        StopCoroutine(coroutine);
        GetGameObjectReference("DialogTriangle").SetActive(false);
    }

    // ========== Buttons ========== 

    public void MainMenu_PlayButton() {
        HideAllMenus();
        HideDialog();
        StartCoroutine(PlayBeginAnimation());
    }

    IEnumerator PlayBeginAnimation() {
        Image fadeBlackImage = GameObject.Find("Lower Screen/FadeInImage/FadeBlackImage").GetComponent<Image>();
        var color = fadeBlackImage.color;
        for (int i = 4; i >= 0; i--) {
            color.a = i * 0.09f;
            fadeBlackImage.color = color;
            yield return new WaitForSeconds(frameTime*2);
        }
        ValuesPopulate(level);
        renderManager.UpdateValues();
        pointerController.setPointerType(PointerController.PointerType.Tile);
        pointerController.showPointer();
    }

    public void MainMenu_GameInfoButton() {
        HideAllMenus();
        StartCoroutine(DialogBoxShow("Which set of info?", Show4OptionsMenu, "GameInfoMenu"));
    }

    public void MainMenu_QuitButton() {
        Debug.Log("Quit");
    }

    public void GameInfoMenu_HowToPlayButton() {
        HideAllMenus();
        StartCoroutine(GameInfoMenu_HowToPlayButton_Animation());
    }

    IEnumerator GameInfoMenu_HowToPlayButton_Animation() {
        Image upperTransitionImage = GameObject.Find("Upper Screen/TransitionCanvas/TransitionImage").GetComponent<Image>();
        upperTransitionImage.enabled = true;

        for (int i = 0; i < 4; i++)
            yield return null;
        for (int i = 5; i >= 0; i--) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        GetGameObjectReference("InfoCanvas").SetActive(false);
        GetGameObjectReference("HowToPlayCanvas").SetActive(true);

        for (int i = 0; i < 5; i++)
            yield return null;
        for (int i = 1; i <= 5; i++) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        upperTransitionImage.enabled = false;

        List<List<string>> sequencesList = new List<List<string>>();
        List<string> sequences = new List<string>();
        sequences.Add("VOLTORB Flip is a game in which you flip\nover cards to find numbers hidden");
        sequences.Add("\nbeneath them.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("The cards are hiding the numbers\n1 through 3... and VOLTORB as well.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("The first number you flip over will give\nyou that many Coins.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("From then on, the next number you\nfind will multiply the total amount of");
        sequences.Add("\nCoins you've collected by that number.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("If it's a 2, your total will be multiplied\nby \"x2.\"");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("If it's a 3, your total will be multiplied\nby \"x3.\"");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("But if you flip over a VOLTORB, it's\ngame over.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("When that happens, you'll lose all the\nCoins you've collected in the");
        sequences.Add("\ncurrent game.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("If you select \"Quit,\" you'll withdraw\nfrom the game.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("If you get to a difficult spot, you might\nwant to end the game early.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("Once you've found all the hidden\n2 and 3 cards, you've cleared");
        sequences.Add("\nthe game.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("Once you've flipped over all these\ncards, then you'll advance to the");
        sequences.Add("\nnext level.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("As you move up in levels, you will be\nable to receive more Coins.");
        sequences.Add("\nDo your best!");
        sequencesList.Add(sequences);

        StartCoroutine(MultipleScrollingDialog(sequencesList, StartCoroutineFunc, GameInfoMenu_HowToPlayButton_Done()));
    }

    IEnumerator GameInfoMenu_HowToPlayButton_Done() {
        Image upperTransitionImage = GameObject.Find("Upper Screen/TransitionCanvas/TransitionImage").GetComponent<Image>();
        upperTransitionImage.enabled = true;

        for (int i = 0; i < 4; i++)
            yield return null;
        for (int i = 5; i >= 0; i--) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        GetGameObjectReference("HowToPlayCanvas").SetActive(false);
        GetGameObjectReference("InfoCanvas").SetActive(true);

        for (int i = 0; i < 5; i++)
            yield return null;
        for (int i = 1; i <= 5; i++) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        upperTransitionImage.enabled = false;

        ResetDialogText();
        StartCoroutine(DialogBoxShow("Which set of info?", Show4OptionsMenu, "GameInfoMenu"));
    }

    public void GameInfoMenu_HintButton() {
        HideAllMenus();
        StartCoroutine(GameInfoMenu_HintButton_Animation());
    }

    IEnumerator GameInfoMenu_HintButton_Animation() {
        Image upperTransitionImage = GameObject.Find("Upper Screen/TransitionCanvas/TransitionImage").GetComponent<Image>();
        upperTransitionImage.enabled = true;

        for (int i = 0; i < 4; i++)
            yield return null;
        for (int i = 5; i >= 0; i--) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        GetGameObjectReference("InfoCanvas").SetActive(false);
        GetGameObjectReference("HintCanvas").SetActive(true);

        for (int i = 0; i < 5; i++)
            yield return null;
        for (int i = 1; i <= 5; i++) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        upperTransitionImage.enabled = false;

        List<List<string>> sequencesList = new List<List<string>>();
        List<string> sequences = new List<string>();
        sequences.Add("The numbers at the side of the board\ngive you a clue about the numbers");
        sequences.Add("\nhidden on the backs of the panels.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("The larger the number, the more likely\nit is that there are many large numbers");
        sequences.Add("\nhidden in that row or column.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("In the same way, you can tell how many\nVOLTORB are hidden in the row");
        sequences.Add("\nor column.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("Consider the hidden number totals and\nthe VOLTORB totals carefully as you flip");
        sequences.Add("\nover panels.");
        sequencesList.Add(sequences);

        StartCoroutine(MultipleScrollingDialog(sequencesList, StartCoroutineFunc, GameInfoMenu_HintButton_Done()));
    }

    IEnumerator GameInfoMenu_HintButton_Done() {
        Image upperTransitionImage = GameObject.Find("Upper Screen/TransitionCanvas/TransitionImage").GetComponent<Image>();
        upperTransitionImage.enabled = true;

        for (int i = 0; i < 4; i++)
            yield return null;
        for (int i = 5; i >= 0; i--) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        GetGameObjectReference("HintCanvas").SetActive(false);
        GetGameObjectReference("InfoCanvas").SetActive(true);

        for (int i = 0; i < 5; i++)
            yield return null;
        for (int i = 1; i <= 5; i++) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        upperTransitionImage.enabled = false;

        ResetDialogText();
        StartCoroutine(DialogBoxShow("Which set of info?", Show4OptionsMenu, "GameInfoMenu"));
    }

    public void GameInfoMenu_AboutMemosButton() {
        HideAllMenus();
        StartCoroutine(GameInfoMenu_AboutMemosButton_Animation());
    }

    IEnumerator GameInfoMenu_AboutMemosButton_Animation() {
        Image upperTransitionImage = GameObject.Find("Upper Screen/TransitionCanvas/TransitionImage").GetComponent<Image>();
        upperTransitionImage.enabled = true;

        for (int i = 0; i < 4; i++)
            yield return null;
        for (int i = 5; i >= 0; i--) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        GetGameObjectReference("InfoCanvas").SetActive(false);
        GetGameObjectReference("AboutMemosCanvas").SetActive(true);

        for (int i = 0; i < 5; i++)
            yield return null;
        for (int i = 1; i <= 5; i++) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        upperTransitionImage.enabled = false;

        List<List<string>> sequencesList = new List<List<string>>();
        List<string> sequences = new List<string>();
        sequences.Add("Select \"Open Memo\" to mark\nthe cards.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("You can mark the cards with the\nnumbers 1 through 3, but also with a");
        sequences.Add("\nVOLTORB mark.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("When you have an idea of the numbers\nhidden on the back of the cards, touch");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("\"Open Memo\" and select the cards to mark.");
        sequencesList.Add(sequences);
        sequences = new List<string>();
        sequences.Add("If you want to remove a mark, touch the\nmark again, and it will disappear.");
        sequencesList.Add(sequences);

        IEnumerator coroutine = GameInfoMenu_AboutMemosButton_UpperScreenAnimation();
        StartCoroutine(coroutine);
        StartCoroutine(MultipleScrollingDialog(sequencesList, StartCoroutineFunc, GameInfoMenu_AboutMemosButton_Done(coroutine)));
    }

    IEnumerator GameInfoMenu_AboutMemosButton_UpperScreenAnimation() {
        int i = 1;
        SpriteRenderer renderer = GetGameObjectReference("AboutMemosCanvas").GetComponentInChildren<SpriteRenderer>();
        while (true) {
            if (i == 1) {
                renderer.sprite = GetSpriteReference("AboutMemos" + i);
                i = 2;
            } else {
                renderer.sprite = GetSpriteReference("AboutMemos" + i);
                i = 1;
            }
            for (int j = 0; j < 32; j++) {
                yield return null;
            }
        }
    }

    IEnumerator GameInfoMenu_AboutMemosButton_Done(IEnumerator animationToCancel) {
        Image upperTransitionImage = GameObject.Find("Upper Screen/TransitionCanvas/TransitionImage").GetComponent<Image>();
        upperTransitionImage.enabled = true;

        for (int i = 0; i < 4; i++)
            yield return null;
        for (int i = 5; i >= 0; i--) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }
        StopCoroutine(animationToCancel);

        GetGameObjectReference("AboutMemosCanvas").SetActive(false);
        GetGameObjectReference("InfoCanvas").SetActive(true);

        for (int i = 0; i < 5; i++)
            yield return null;
        for (int i = 1; i <= 5; i++) {
            upperTransitionImage.sprite = GetSpriteReference("Transition" + i);
            for (int j = 0; j < 2; j++)
                yield return null;
        }

        upperTransitionImage.enabled = false;

        ResetDialogText();
        StartCoroutine(DialogBoxShow("Which set of info?", Show4OptionsMenu, "GameInfoMenu"));
    }

    public void GameInfoMenu_ReturnButton() {
        HideAllMenus();
        StartCoroutine(DialogBoxShow("Play VOLTORB Flip Lv. 1?", Show3OptionsMenu, "MainMenu"));
    }

    void StartCoroutineFunc(IEnumerator coroutine) {
        StartCoroutine(coroutine);
    }

    public void ResetDialogText() {
        RectTransform transform = (RectTransform)GetGameObjectReference("DialogText").transform;
        transform.offsetMax = new Vector2(transform.offsetMax.x, 0);
        Text dialogText = dialogBox.GetComponentInChildren<Text>();
        dialogText.text = "";
    }

    public void HideDialog() {
        ResetDialogText();
        dialogBox.SetActive(false);
    }

    void ValuesPopulate(int level = 1) {
        //Calculates the numbers of x2 and x3
        int numsOf2 = 0;
        int numsOf3 = 0;
        int numsOfVoltorb = 0;
        int min = 0;
        int max = 0;
        //https://bulbapedia.bulbagarden.net/wiki/Talk:Voltorb_Flip
        switch (level) {
            case 1:
                min = 24; max = 48; numsOfVoltorb = 6;
                break;
            case 2:
                min = 54; max = 96; numsOfVoltorb = 7;
                break;
            case 3:
                min = 108; max = 192; numsOfVoltorb = 8;
                break;
            case 4:
                min = 216; max = 324; numsOfVoltorb = 8;
                break;
            case 5:
                min = 384; max = 576; numsOfVoltorb = 10;
                break;
            case 6:
            case 7:
            case 8:
                min = 4608; max = 5832; numsOfVoltorb = 10;
                break;
        }
        int coins = 1;
        int i = 0;
        System.Random rnd = new System.Random();
        while (true) {
            // Calculate coins
            coins = 1;
            for (i = 0; i < numsOf2; i++) {
                coins *= 2;
            }
            for (i = 0; i < numsOf3; i++) {
                coins *= 3;
            }
            if (coins < min) {
                if (coins * 3 > max) {
                    if (coins * 2 > max) {
                        numsOf3--;
                    }
                    numsOf2++;
                } else {
                    if (rnd.Next(0, 2) == 0) {
                        numsOf2++;
                    } else numsOf3++;
                }
            } else break;
        }

        // Populate the field
        List<List<int>> list = new List<List<int>>();
        List<int> childList;
        for (i = 0; i < 5; i++) {
            childList = new List<int>() { 0, 0, 1, 2, 3, 4 };
            childList[0] = i;
            list.Add(childList);
        }

        for (i = 0; i < numsOf2; i++) {
            int childListIndex = rnd.Next(0, list.Count);
            List<int> randomChildList = list[childListIndex];
            int row = randomChildList[0];
            int columnIndex = rnd.Next(1, randomChildList.Count);
            int column = randomChildList[columnIndex];

            values[row, column] = 2;

            list[childListIndex].RemoveAt(columnIndex);
            if (list[childListIndex].Count == 1) {
                list.RemoveAt(childListIndex);
            }
        }
        for (i = 0; i < numsOf3; i++) {
            int childListIndex = rnd.Next(0, list.Count);
            List<int> randomChildList = list[childListIndex];
            int row = randomChildList[0];
            int columnIndex = rnd.Next(1, randomChildList.Count);
            int column = randomChildList[columnIndex];

            values[row, column] = 3;

            list[childListIndex].RemoveAt(columnIndex);
            if (list[childListIndex].Count == 1) {
                list.RemoveAt(childListIndex);
            }
        }

        for (i = 0; i < numsOfVoltorb; i++) {
            int childListIndex = rnd.Next(0, list.Count);
            List<int> randomChildList = list[childListIndex];
            int row = randomChildList[0];
            int columnIndex = rnd.Next(1, randomChildList.Count);
            int column = randomChildList[columnIndex];

            values[row, column] = 0;

            list[childListIndex].RemoveAt(columnIndex);
            if (list[childListIndex].Count == 1) {
                list.RemoveAt(childListIndex);
            }
        }
    }

    public void FlipTile(int row, int column, bool ignoreResult = false) {
        StartCoroutine(FlipTileAnimation(row, column, values[row, column], ignoreResult));
    }

    IEnumerator FlipTileAnimation(int row, int column, int tileValue, bool ignoreResult = false) {
        //for(int i = 0; i < 10; i++) {
        //    yield return null;
        //}
        if(!ignoreResult) renderManager.flipStatus[row, column] = true;
        bool showedDialog = false;
        renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[3]);
        for (int i = 0; i < 2; i++) {
            yield return null;
        }
        renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[4]);
        for (int i = 0; i < 4; i++) {
            yield return null;
        }
        switch(tileValue) {
            case 0:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[8]);
                break;
            case 1:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[5]);
                break;
            case 2:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[6]);
                break;
            case 3:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[7]);
                break;
        }
        for (int i = 0; i < 2; i++) {
            yield return null;
        }
        switch (tileValue) {
            case 0:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[9]);
                break;
            case 1:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[0]);
                break;
            case 2:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[1]);
                break;
            case 3:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[2]);
                break;
        }
        for (int i = 0; i < 4; i++) {
            if (!ignoreResult && i == 2) {
                if (tileValue != 0) {
                    if (coins == 0) {
                        showScoreDialog(tileValue + "! Received " + tileValue + " Coin(s)!");
                        showedDialog = true;
                    } else if(tileValue > 1) {
                        showScoreDialog("x" + tileValue + "! Received " + coins*tileValue + " Coins!");
                        showedDialog = true;
                    }
                }
            }
            yield return null;
        }
        if (!ignoreResult) {
            if (tileValue == 0) {
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[4]);
                for (int i = 0; i < 8; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[5]);
                for (int i = 0; i < 8; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[6]);
                for (int i = 0; i < 8; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[7]);
                for (int i = 0; i < 4; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[8]);
                for (int i = 0; i < 4; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[9]);
                for (int i = 0; i < 6; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[10]);
                for (int i = 0; i < 6; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[11]);
                for (int i = 0; i < 6; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[12]);
                for (int i = 0; i < 4; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), null);
                for (int i = 0; i < 6; i++) {
                    yield return null;
                }
                gameResult = GameResult.GameOver;
                showScoreDialog("Oh no! You get 0 Coins!");
                StartCoroutine(UpdateGameScoreAnimation(0));
                for(int i = 0; i < 245; i++) {
                    yield return null;
                }
                hideScoreDialog();
                for (int i = 0; i < 10; i++) {
                    yield return null;
                }
                FlipAllTiles();
                for (int i = 0; i < 12; i++) {
                    yield return null;
                }
                allowDelid = true;
            } else {
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[0]);
                for (int i = 0; i < 7; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[1]);
                for (int i = 0; i < 7; i++) {
                    if (i == 3) {
                        if (coins == 0) {
                            StartCoroutine(UpdateGameScoreAnimation(tileValue));
                        } else {
                            StartCoroutine(UpdateGameScoreAnimation(coins * tileValue));
                        }
                    }
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[2]);
                for (int i = 0; i < 7; i++) {
                    yield return null;
                }
                if (!scoreDialogShowing) pointerController.ignoreInput = false;
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.flipEffectsTiles[3]);
                for (int i = 0; i < 7; i++) {
                    yield return null;
                }
                renderManager.flipEffectsTilemap.SetTile(new Vector3Int(column, -row, 0), null);
                if (showedDialog) {
                    allowScoreDialogHideInput = true;
                }
            }
        }
    }

    IEnumerator DelidTileAnimation(int row, int column, int tileValue) {
        switch (tileValue) {
            case 0:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[8]);
                break;
            case 1:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[5]);
                break;
            case 2:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[6]);
                break;
            case 3:
                renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[7]);
                break;
        }
        for (int i = 0; i < 2; i++) {
            yield return null;
        }
        renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[4]);
        for (int i = 0; i < 4; i++) {
            yield return null;
        }
        renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), renderManager.tilesTiles[3]);
        for (int i = 0; i < 2; i++) {
            yield return null;
        }
        renderManager.tilesTilemap.SetTile(new Vector3Int(column, -row, 0), null);
        for (int i = 0; i < 4; i++) {
            yield return null;
        }
    }

    IEnumerator UpdateGameScoreAnimation(int newCoins) {
        while(coins < newCoins) {
            coins++;
            UpdateGameScore();
            yield return null;
            yield return null;
        }
        while(coins > newCoins) {
            coins--;
            UpdateGameScore();
            yield return null;
            yield return null;
        }
    }

    public void UpdateGameScore() {
        int i;
        for(i = 1; i <= 5; i++) {
            GetGameObjectReference("LowerDigit" + i).GetComponent<SpriteRenderer>().sprite = GetSpriteReference("InfoDigits_0");
        }
        i = 5;
        int tmpCoins = coins;
        while (tmpCoins > 0) {
            GetGameObjectReference("LowerDigit" + i).GetComponent<SpriteRenderer>().sprite = GetSpriteReference("InfoDigits_" + tmpCoins % 10);
            tmpCoins = tmpCoins / 10;
            i--;
        }
    }

    public void showScoreDialog(string text) {
        scoreDialogBox.transform.GetChild(0).gameObject.SetActive(true);
        Text dialogText = scoreDialogBox.GetComponentInChildren<Text>();
        dialogText.text = text;
        scoreDialogShowing = true;
    }

    public void hideScoreDialog() {
        scoreDialogBox.transform.GetChild(0).gameObject.SetActive(false);
        Text dialogText = scoreDialogBox.GetComponentInChildren<Text>();
        dialogText.text = "";
        scoreDialogShowing = false;
        if (gameResult == GameResult.GameOver) {

        } else {
            pointerController.ignoreInput = false;
            //Check for game completion
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    if (values[i, j] > 1 && renderManager.flipStatus[i, j] == false)
                        return;
                }
            }
            StartCoroutine(gameCompletion());
        }
    }

    IEnumerator gameCompletion() {
        pointerController.ignoreInput = true;
        for (int i = 0; i < 4; i++)
            yield return null;
        List<List<string>> sequencesList = new List<List<string>>();
        List<string> sequence = new List<string>();
        sequence.Add("Game clear!");
        sequencesList.Add(sequence);
        sequence = new List<string>();
        sequence.Add("You've found all of the hidden x2 and\nx3 cards.");
        sequencesList.Add(sequence);
        sequence = new List<string>();
        sequence.Add("This means you've found all the Coins\nin this game, so the game is now over.");
        sequencesList.Add(sequence);
        StartCoroutine(MultipleScrollingDialog(sequencesList, gameCompletion2));
        Image fadeBlackImage = GameObject.Find("Lower Screen/FadeInImage/FadeBlackImage").GetComponent<Image>();
        fadeBlackImage.canvas.sortingLayerName = "Transitions2";
        var color = fadeBlackImage.color;
        for (int i = 0; i < 6; i++) {
            color.a = i * 0.09f;
            fadeBlackImage.color = color;
            yield return new WaitForSeconds(frameTime*2);
        }
    }

    void gameCompletion2() {
        List<List<string>> sequencesList = new List<List<string>>();
        List<string> sequence = new List<string>();
        sequence.Add("An\nreceived " + coins + " Coin(s)!");
        sequencesList.Add(sequence);
        allowScrollingDialogInput = false;
        StartCoroutine(MultipleScrollingDialog(sequencesList, StartCoroutineFunc, gameCompletion3()));

        StartCoroutine(CollectCoins());
    }

    IEnumerator CollectCoins() {
        int i;
        for (i = 1; i <= 5; i++) {
            GetGameObjectReference("UpperDigit" + i).GetComponent<SpriteRenderer>().sprite = GetSpriteReference("InfoDigits_0");
        }
        i = 5;
        int savedCoins = PlayerPrefs.GetInt("collectedCoins");
        PlayerPrefs.SetInt("collectedCoins", savedCoins + coins);
        while (coins > 0) {
            int savedCoinsBefore = savedCoins;
            savedCoins++;
            coins--;
            int tmpSavedCoins = savedCoins;
            while (i >= 1) {
                GetGameObjectReference("UpperDigit" + i).GetComponent<SpriteRenderer>().sprite = GetSpriteReference("InfoDigits_" + tmpSavedCoins % 10);
                tmpSavedCoins = tmpSavedCoins / 10;
                i--;
                savedCoinsBefore = savedCoinsBefore / 10;
                if (tmpSavedCoins != savedCoinsBefore) break;
            }
            i = 5;
            UpdateGameScore();
            yield return null;
        }
        allowScrollingDialogInput = true;
    }

    IEnumerator gameCompletion3() {
        HideDialog();
        Image fadeBlackImage = GameObject.Find("Lower Screen/FadeInImage/FadeBlackImage").GetComponent<Image>();
        var color = fadeBlackImage.color;
        for (int i = 4; i >= 0; i--) {
            color.a = i * 0.09f;
            fadeBlackImage.color = color;
            yield return new WaitForSeconds(frameTime * 2);
        }
        FlipAllTiles();
        for(int i = 0; i < 12; i++) { yield return null; }
        gameResult = GameResult.Advance;
        allowDelid = true;
    }

    void FlipAllTiles() {
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                if (renderManager.flipStatus[i, j] == false) {
                    FlipTile(i, j, true);
                }
            }
        }
    }

    IEnumerator DelidAllTiles() {
        allowDelid = false;
        for (int j = 0; j < 5; j++) {
            for (int i = 0; i < 5; i++) {
                StartCoroutine(DelidTileAnimation(i, j, values[i, j]));
            }
            for (int i = 0; i < 12; i++) { yield return null; }
        }
        pointerController.hidePointer();
        CheckGameResult();
    }

    void CheckGameResult() {
        switch(gameResult) {
            case GameResult.GameOver:
                int multiplerCards = 0;
                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 5; j++) {
                        if (renderManager.flipStatus[i, j] == true && values[i, j] >= 1) {
                            multiplerCards++;
                        }
                    }
                }
                if (multiplerCards == 0) {
                    newLevel = 1;
                } else if (multiplerCards < level) {
                    newLevel = multiplerCards;
                }
                GameReset();
                PlayStartAnimation();
                break;
            case GameResult.Advance:
                GameReset();
                level++;
                newLevel = level;
                GameObject.Find("Upper Screen/InfoCanvas/InfoText1").GetComponent<Text>().text = "VOLTORB Flip Lv. " + level + "\nFlip the Cards and Collect Coins!";
                PlayStartAnimation();
                break;
        }
    }

    void GameReset() {
        renderManager.horizontalValues.Clear();
        renderManager.verticalValues.Clear();
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                values[i, j] = 1;
                renderManager.flipStatus[i, j] = false;
            }
            renderManager.horizontalValues.Add(new List<int>() { 0, 0, 0 });
            renderManager.verticalValues.Add(new List<int>() { 0, 0, 0 });
        }
        gameResult = GameResult.None;
        pointerController.ignoreInput = false;
    }

    private void OnGUI() {
        Event e = Event.current;
        if (e.isKey && e.type == EventType.KeyDown) {
            if(scoreDialogShowing && allowScoreDialogHideInput) {
                hideScoreDialog();
                allowScoreDialogHideInput = false;
                return;
            } else if(allowDelid && gameResult != GameResult.None) {
                //Delid
                allowDelid = false;
                StartCoroutine(DelidAllTiles());
            }
        }
        pointerController.OnGUI_Pointer(e);
    }

    private void Update() {
        if (allowScrollingDialogInput && scrollingDialogInput == false) {
            if (Input.anyKeyDown) {
                scrollingDialogInput = true;
            }
        }
    }

}
