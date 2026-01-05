using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> _dialogueSentences;

    public static DialogueManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _dialogueSentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        Debug.Log("Starting Convo With" + dialogue.name);

        _dialogueSentences.Clear();

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
        
    }

    public void EndDialogue()
    {
        Debug.Log("End Of Convo.");
    }
}
