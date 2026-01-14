using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> _dialogueSentences;
    private Coroutine _currentSentenceCourotine;
    private GameObject _lookCamera;

    public static DialogueManager instance;

    public static event Action onDialougeEnd;
    public static event Action onSentenceEnd;
    public static event Action onSentenceStart;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            _dialogueSentences = new Queue<string>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        if(_lookCamera == null)
        {
            _lookCamera = GameObject.FindGameObjectWithTag("LookCamera");
        }
        Debug.Log("Starting Convo With" + dialogue.name);

        SetPlayerInteraction(false);

        _dialogueSentences.Clear();
        UIManager.instance.ShowDialogueBox();
        UIManager.instance.SetDialogueName(dialogue.name);
        UIManager.instance.HideGameHUD();
        foreach (string sentence in dialogue.sentences)
        {
            _dialogueSentences.Enqueue(sentence);
        }

        DisplayNextSentecne();
    }

    public void DisplayNextSentecne()
    {
        if (_dialogueSentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string currentSentence = _dialogueSentences.Dequeue();

        UIManager.instance.SetDialogueText(currentSentence);

        if (_currentSentenceCourotine != null)
        {
            StopCoroutine(_currentSentenceCourotine);
        }

        _currentSentenceCourotine = StartCoroutine(ShowDialogueAnimation(UIManager.instance.GetDialougeTextObj()));
        onSentenceStart?.Invoke();

    }

    public void EndDialogue()
    {

        SetPlayerInteraction(true);
        Debug.Log("End Of Convo.");
        UIManager.instance.SetDialogueName("");
        UIManager.instance.SetDialogueText("");
        UIManager.instance.HideDialogueBox();
        UIManager.instance.ShowGameHUD();
        onDialougeEnd?.Invoke();
    }

    private void SetPlayerInteraction(bool isActive)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerMovement>().canMove = isActive;
        Camera.main.GetComponent<RayInetractor>().enabled = isActive;
        Cursor.lockState = (isActive) ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = !isActive;

        if(_lookCamera != null)
        {
            _lookCamera.SetActive(isActive);
        }
    }

    IEnumerator ShowDialogueAnimation(TextMeshProUGUI text)
    {
        text.maxVisibleCharacters = 0;
        Char[] TextCharArray = text.text.ToCharArray();
        foreach (char c in TextCharArray)
        {
            text.maxVisibleCharacters++;
            yield return new WaitForSeconds(0.05f);
        }

        if(text.maxVisibleCharacters == TextCharArray.Length)
        {
            onSentenceEnd?.Invoke();
        }
    }

}
