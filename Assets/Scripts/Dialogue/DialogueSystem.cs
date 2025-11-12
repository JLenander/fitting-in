using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public SplitscreenUIHandler dialogueUI;
    public float textDelay = 0.05f;   // typing speed
    public AudioSource audioSource;
    public AudioClip clip;

    private Sprite _spriteA;              // first image
    private Sprite _spriteB;

    private string[] _lines;            // dialogue lines

    private int _currentLine = 0;
    private bool _showingImageA = true;
    private string _line;
    private Coroutine _dialogueRoutine;
    private bool _isDialogueActive;
    private Queue<DialogueScriptableObj> _dialogueQueue = new();

    private Coroutine dialoguePlayer;

    public void StartDialogue(DialogueScriptableObj content)
    {
        if (_isDialogueActive)
        {
            // Queue the dialogue instead of interrupting
            _dialogueQueue.Enqueue(content);
            return;
        }

        dialogueUI.InitializeDialogue(); // shows dialogue box
        _currentLine = 0;
        _lines = content.lines;
        _spriteA = content.spirteA;
        _spriteB = content.spirteB;
        _isDialogueActive = true;
        dialoguePlayer = StartCoroutine(PlayDialogue());
    }

    public void ClearDialogue()
    {
        _dialogueQueue.Clear();
        _isDialogueActive = false;
        StopCoroutine(dialoguePlayer);

        // cleanup
        _line = "";
        dialogueUI.WriteDialogueText(_line);
        dialogueUI.HideDialogue();
        _isDialogueActive = false;
    }

    private IEnumerator PlayDialogue()
    {
        while (_currentLine < _lines.Length)
        {
            // Type out the text
            _line = "";
            dialogueUI.WriteDialogueText(_line);
            foreach (char c in _lines[_currentLine])
            {
                _line += c;
                dialogueUI.WriteDialogueText(_line);

                if (c != ' ')
                {
                    _showingImageA = !_showingImageA;
                    dialogueUI.ChangeDialogueSprite(_showingImageA ? _spriteA : _spriteB);
                    audioSource.PlayOneShot(clip);
                }

                yield return new WaitForSeconds(textDelay);
            }

            // Wait until player presses a key to continue
            yield return new WaitForSeconds(_line.Length * 0.05f);

            _currentLine++;
        }

        // dialogue finished - cleanup
        _line = "";
        dialogueUI.WriteDialogueText(_line);
        dialogueUI.HideDialogue();
        _isDialogueActive = false;

        // Check for queued dialogues
        if (_dialogueQueue.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
            DialogueScriptableObj nextDialogue = _dialogueQueue.Dequeue();
            StartDialogue(nextDialogue); // Recursive call
        }
    }
}
