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

    public DialogueScriptableObj blinkRepeatDialogue;

    public DialogueScriptableObj armRepeatDialog;

    public DialogueScriptableObj eyeRepeatDialog;

    public DialogueScriptableObj grabRepeatDialog;

    public DialogueScriptableObj dropRepeatDialogue;

    public HandConsole leftConsole;
    public HandConsole rightConsole;
    public HipConsole hipConsole;
    public HeadConsole headConsole;
    public BlinkConsole blinkConsole;

    public GameObject blinkLight;
    public GameObject headLight;
    public List<GameObject> armLights = new List<GameObject>();
    public List<GameObject> legLights = new List<GameObject>();

    public bool beginFire = false;
    public bool blinked = false;
    public bool interactEyeTerminal = false;
    public bool eyeAim = false;
    public bool interactArmTerminal = false;
    public bool grabBall = false;
    public bool dropBall = false;
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
        SetLightsDeactive(normalLights);

        // disable all terminals first
        headConsole.DisableInteract();
        leftConsole.DisableInteract();
        rightConsole.DisableInteract();
        hipConsole.DisableInteract();
        blinkConsole.DisableInteract();

        blinkLight.SetActive(false);
        headLight.SetActive(false);
        SetLightsDeactive(armLights);
        SetLightsDeactive(legLights);
    }

    IEnumerator StartLevel()
    {
        // 1. computer flavour text
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        yield return null;

        FireManager.Instance.StartFireArea("begin");

        yield return new WaitUntil(() => beginFire);

        SetLightsActive(normalLights);

        // 2. fire is put out yay!
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(27f);

        // blink to ensure fires are gone
        blinkConsole.EnableInteract();
        blinkConsole.EnableOutline();
        headConsole.DisableInteract();

        blinkLight.SetActive(true);
        SetLightsDeactive(normalLights);

        repeatDialogue = blinkRepeatDialogue;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => blinked);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        blinkConsole.DisableInteract();
        blinkConsole.enabled = false;

        blinkLight.SetActive(false);
        SetLightsActive(normalLights);

        // 3. message from general plorp about eye terminals
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(5f);

        // re-enable head
        headConsole.EnableInteract();
        headLight.SetActive(true);
        SetLightsDeactive(normalLights);

        repeatDialogue = eyeRepeatDialog;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => interactEyeTerminal);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        headLight.SetActive(false);
        SetLightsActive(normalLights);

        // 4. move until the rectile becomes green
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(5f);
        eyeAim = false;
        yield return new WaitUntil(() => eyeAim && interactEyeTerminal);
        Debug.Log("eye aim " + eyeAim);

        // 5. message from general plorp about arm terminals
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(10f);
        SetLightsActive(armLights);
        SetLightsDeactive(normalLights);

        leftConsole.EnableInteract();
        rightConsole.EnableInteract();

        leftConsole.EnableOutline();
        rightConsole.EnableOutline();

        repeatDialogue = armRepeatDialog;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => interactArmTerminal);
        leftConsole.DisableOutline();
        rightConsole.DisableOutline();
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();
        SetLightsDeactive(armLights);
        SetLightsActive(normalLights);

        // 6. what the arm does
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // 7. how to grapple
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        repeatDialogue = grabRepeatDialog;
        index++;

        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => grabBall);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        // drop ball
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        repeatDialogue = dropRepeatDialogue;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());
        yield return new WaitUntil(() => dropBall);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        // Start play basketball
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // 8. how to play basketball
        repeatDialogue = dialogues[index];
        index++;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => scoreBall);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        // 9. Leg terminal
        hipConsole.EnableInteract();
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(5f);
        SetLightsActive(legLights);
        SetLightsDeactive(normalLights);

        yield return new WaitUntil(() => interactLegTerminal);
        GlobalPlayerUIManager.Instance.StopText();
        SetLightsDeactive(legLights);
        SetLightsActive(normalLights);

        // 10. How to walk
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // done tutorial
        yield return new WaitForSeconds(15f);
        // 11. Brain terminal online
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // 12. General signing off
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // start task chain
        Level0TaskManager.StartTaskGoToPhone();

        // reenable blinking fires 
        blinkConsole.enabled = true;
        blinkConsole.EnableInteract();
    }

    IEnumerator RepeatDialogue()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);

            GlobalPlayerUIManager.Instance.LoadText(repeatDialogue);
        }
    }

    private void SetLightsActive(List<GameObject> lightsList)
    {
        foreach (GameObject light in lightsList)
        {
            light.SetActive(true);
        }
    }

    private void SetLightsDeactive(List<GameObject> lightsList)
    {
        foreach (GameObject light in lightsList)
        {
            light.SetActive(false);
        }
    }
}
