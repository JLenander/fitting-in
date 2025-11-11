using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public List<Material> materials = new List<Material>();

    public List<GameObject> normalLights = new List<GameObject>();

    public List<DialogueScriptableObj> dialogues = new List<DialogueScriptableObj>();

    public DialogueScriptableObj armRepeatDialog;

    public DialogueScriptableObj eyeRepeatDialog;

    public DialogueScriptableObj grabRepeatDialog;

    public HandConsole leftConsole;
    public HandConsole rightConsole;
    public HipConsole hipConsole;
    public HeadConsole headConsole;
    public BlinkConsole blinkConsole;

    public bool beginFire = false;
    public bool interactEyeTerminal = false;
    public bool interactArmTerminal = false;
    public bool grabBall = false;
    public bool scoreBall = false;
    public bool interactLegTerminal = false;
    private int index;
    private DialogueScriptableObj repeatDialogue;
    private Coroutine repeatDialogueRoutine;
    void Start()
    {
        Instance = this;
        index = 0;
        StartCoroutine(StartLevel());

        // disable all normal lights
        foreach (GameObject light in normalLights)
        {
            light.SetActive(false);
        }

        // disable all terminals first
        headConsole.DisableInteract();
        leftConsole.DisableInteract();
        rightConsole.DisableInteract();
        hipConsole.DisableInteract();
        blinkConsole.enabled = false;
    }

    IEnumerator StartLevel()
    {
        // 1. computer flavour text
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        yield return null;

        FireManager.Instance.StartFireArea("begin");

        yield return new WaitUntil(() => beginFire);

        foreach (GameObject light in normalLights)
        {
            light.SetActive(true);
        }

        // 2. fire is put out yay!
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // 3. message from general plorp about eye terminals
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // re-enable arms n head
        headConsole.EnableInteract();

        repeatDialogue = eyeRepeatDialog;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => interactEyeTerminal);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        // 4. message from general plorp about arm terminals
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        leftConsole.EnableInteract();
        rightConsole.EnableInteract();

        repeatDialogue = armRepeatDialog;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => interactArmTerminal);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        // 5. what the arm does
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // 6. how to grapple
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        repeatDialogue = grabRepeatDialog;
        index++;

        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => grabBall);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        // 7. how to play basketball
        repeatDialogue = dialogues[index];
        index++;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => scoreBall);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        // 8. Leg terminal
        hipConsole.EnableInteract();
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        yield return new WaitUntil(() => interactLegTerminal);
        GlobalPlayerUIManager.Instance.StopText();

        // 9. How to walk
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // done tutorial
        yield return new WaitForSeconds(15f);
        // 10. Brain terminal online
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // 11. General signing off
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // start task chain
        Level0TaskManager.StartTaskGoToPhone();
    }

    IEnumerator RepeatDialogue()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);

            GlobalPlayerUIManager.Instance.LoadText(repeatDialogue);
        }
    }
}
