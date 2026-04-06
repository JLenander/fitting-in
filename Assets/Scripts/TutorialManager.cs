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
    
    // Scene Exit door stuff
    public SceneExitDoor sceneExitDoor;
    public Vector3 target;

    public Transform robotCameraTransform;
    
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
        
        // Disable blink system
        blinkConsole.SetRunBlinkSystem(false);

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
        yield return new WaitForSeconds(8.75f);

        // blink to ensure fires are gone
        blinkConsole.EnableInteract();
        blinkConsole.SetRunBlinkSystem(false);
        EnableSilhouetteOutline(blinkConsole);
        headConsole.DisableInteract();

        blinkLight.SetActive(true);
        SetLightsDeactive(normalLights);

        repeatDialogue = blinkRepeatDialogue;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => blinked);
        DisableSilhouetteOutline(blinkConsole);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        blinkConsole.DisableInteract();
        blinkConsole.SetRunBlinkSystem(false);
        blinkLight.SetActive(false);
        SetLightsActive(normalLights);

        // 3. message from general plorp about eye terminals
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(4.75f);

        // re-enable head
        headConsole.EnableInteract();
        headLight.SetActive(true);
        SetLightsDeactive(normalLights);
        EnableSilhouetteOutline(headConsole);

        repeatDialogue = eyeRepeatDialog;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => interactEyeTerminal);
        DisableSilhouetteOutline(headConsole);
        StopCoroutine(repeatDialogueRoutine);
        GlobalPlayerUIManager.Instance.StopText();

        headLight.SetActive(false);
        SetLightsActive(normalLights);

        // 4. move until the rectile becomes green
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(1.5f);
        eyeAim = false;
        yield return new WaitUntil(() => eyeAim && interactEyeTerminal);
        Debug.Log("eye aim " + eyeAim);

        // 5. message from general plorp about arm terminals
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(9.5f);
        SetLightsActive(armLights);
        SetLightsDeactive(normalLights);
        
        leftConsole.EnableInteract();
        rightConsole.EnableInteract();

        EnableSilhouetteOutline(leftConsole);
        EnableSilhouetteOutline(rightConsole);

        repeatDialogue = armRepeatDialog;
        repeatDialogueRoutine = StartCoroutine(RepeatDialogue());

        yield return new WaitUntil(() => interactArmTerminal);
        DisableSilhouetteOutline(leftConsole);
        DisableSilhouetteOutline(rightConsole);
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

        // // drop ball
        // GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        // index++;
        //
        // repeatDialogue = dropRepeatDialogue;
        // repeatDialogueRoutine = StartCoroutine(RepeatDialogue());
        // yield return new WaitUntil(() => dropBall);
        // StopCoroutine(repeatDialogueRoutine);
        // GlobalPlayerUIManager.Instance.StopText();

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
        yield return new WaitForSeconds(5.75f);
        EnableSilhouetteOutline(hipConsole);
        SetLightsActive(legLights);
        SetLightsDeactive(normalLights);

        yield return new WaitUntil(() => interactLegTerminal);
        DisableSilhouetteOutline(hipConsole);
        GlobalPlayerUIManager.Instance.StopText();
        SetLightsDeactive(legLights);
        SetLightsActive(normalLights);

        // 10. How to walk
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // done tutorial
        yield return new WaitForSeconds(12f);
        // 11. Brain terminal online
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // 12. General signing off
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // start task chain
        // Level0TaskManager.StartTaskGoToPhone();
        yield return new WaitForSeconds(10.25f);
        Level0TaskManager.StartTaskLeavePhone();
        
        // reenable blinking fires 
        // blinkConsole.enabled = true;
        blinkConsole.EnableInteract();
        blinkConsole.SetRunBlinkSystem(true);

        // Activate, after a delay, the door slide up sequence once the robot camera is looking at the door
        yield return new WaitForSeconds(8f);
        float angle;
        do
        {
            angle = Vector3.Angle(robotCameraTransform.forward, -1 * sceneExitDoor.transform.forward);
            // Debug.Log("Angle is " + angle);
            yield return new WaitForSeconds(0.1f);
        } while (angle > 45f);
        Debug.Log("Activating door");
        yield return new WaitForSeconds(1f);
        while (Vector3.Distance(sceneExitDoor.transform.position, target) > 0.01f)
        {
            sceneExitDoor.transform.position = Vector3.MoveTowards(sceneExitDoor.transform.position, target, 10 * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator RepeatDialogue()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);

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

    /// <summary>
    /// Enables the silhouette outline and outline mode for an interactable
    /// </summary>
    private void EnableSilhouetteOutline(Interactable interactable)
    {
        interactable.EnableForceOutline();
        interactable.ChangeOutlineMode(Outline.Mode.OutlineAndSilhouette);
        interactable.EnableOutline();
    }

    /// <summary>
    /// Disables the interactable outline and resets the mode to original setting
    /// </summary>
    private void DisableSilhouetteOutline(Interactable interactable)
    {
        interactable.DisableForceOutline();
        interactable.DisableOutline();
        interactable.ResetOutlineModeToAssetSetting();
    }
}
