using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class NovaLevel1Manager : MonoBehaviour
{
    public static NovaLevel1Manager Instance;
    public Animator novaAnimator;

    public List<DialogueScriptableObj> dialogues = new List<DialogueScriptableObj>();
    public DialogueScriptableObj dropFoodDialogue;
    public DialogueScriptableObj stealFoodDialogue;

    public DialogueScriptableObj feedFoodDialogue;
    public DialogueScriptableObj forceFeedDialogue;

    public CakeManager cakeManager;

    public EvidenceSpawner evidenceSpawner;
    public GameObject novaRightHandCake;

    public AudioSource eatSource;
    public StudioEventEmitter biteSfx;

    public bool grabbed = false;
    public bool bagDiscarded = false;
    public bool poured = false;

    public Transform bag;
    public GameObject garbageCan;
    public GameObject tableCup;
    public GameObject handCup;
    public CoffeePot coffeePot;
    public bool talking = false;
    public Food food;
    private float switchInterval = 10f;
    private float timer = 0f;
    public bool ate = false;

    public Coroutine levelCoroutine;

    [SerializeField] BlinkConsole blinkConsole;
    [SerializeField] SceneExitDoor sceneExitDoor;

    void Start()
    {
        Instance = this;
        StartCoroutine(WaitForTaskManager());
        blinkConsole.SetRunBlinkSystem(true);
        sceneExitDoor.gameObject.SetActive(false);
    }

    IEnumerator EatCake()
    {
        if (cakeManager.CanRemoveSlice())
        {
            talking = false;
            novaAnimator.SetTrigger("Eat");
            cakeManager.RemoveSlice().SetActive(false);
            yield return new WaitForSeconds(1f);
            novaRightHandCake.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            biteSfx.Play();
            novaRightHandCake.SetActive(false);
            yield return new WaitForSeconds(1f);
            talking = true;
            timer = switchInterval + 1;
        }
    }

    public void ThankForFood(int counter)
    {
        biteSfx.Play();

        if (counter > 2)
        {
            // ok thats a lil too much
            GlobalPlayerUIManager.Instance.LoadText(forceFeedDialogue);
        }
        else
        {
            // aww thanks
            GlobalPlayerUIManager.Instance.LoadText(feedFoodDialogue);
        }
    }

    public void StealFood()
    {
        // GlobalPlayerUIManager.Instance.LoadText(stealFoodDialogue);
        StartCoroutine(StealFoodRoutine());
    }

    IEnumerator StealFoodRoutine()
    {
        // play anim
        novaAnimator.SetTrigger("Sus");
        // comment on food drop
        GlobalPlayerUIManager.Instance.StopText();
        GlobalPlayerUIManager.Instance.LoadText(stealFoodDialogue);
        yield return new WaitForSeconds(2.5f);
    }

    public void DropFood()
    {
        // GlobalPlayerUIManager.Instance.LoadText(dropFoodDialogue);
        StartCoroutine(DropFoodRoutine());
    }

    IEnumerator DropFoodRoutine()
    {
        // play anim
        novaAnimator.SetTrigger("Sus");
        // comment on food drop
        GlobalPlayerUIManager.Instance.StopText();
        GlobalPlayerUIManager.Instance.LoadText(dropFoodDialogue);
        yield return new WaitForSeconds(2.5f);
    }

    IEnumerator WaitForTaskManager()
    {
        yield return new WaitUntil(() => Level1TaskManager.Instance != null);
        Level1TaskManager.StartTaskLevel1Intro();
    }

    IEnumerator DrinkCoffee()
    {
        talking = false;

        novaAnimator.SetTrigger("Drink");

        yield return null;

        yield return new WaitForSeconds(2);
        // make table cup disappear
        tableCup.SetActive(false);

        // make nova hand cup appear
        handCup.SetActive(true);

        yield return new WaitForSeconds(4.6f);

        // make table cup appear
        tableCup.SetActive(true);

        // make nova hand cup disappear
        handCup.SetActive(false);
        talking = true;
    }

    public void PlayLevelRoutine()
    {
        levelCoroutine = StartCoroutine(LevelStart());
    }

    public IEnumerator LevelStart()
    {
        Level1TaskManager.CompleteTaskLevel1Intro();

        int index = 0;
        // seat nova at seat, intro dialogue
        transform.position = new Vector3(254.8f, -26.8f, 5.8f);
        transform.localRotation = new Quaternion(0, 0, 0, 0);

        talking = true;

        // blurb about herself
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(12f);

        // blurb about food
        Level1TaskManager.StartTaskEatFood();
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(7f);
        
        // Wait until at least one bite of food is eaten
        yield return new WaitUntil(() => ate);

        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        yield return StartCoroutine(EatCake());

        float timeout = 15f;
        float timer = 0f;

        // progress when either food is all done or timer runs out
        while (food.canPickup && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // after a while she eats another slice
        yield return StartCoroutine(EatCake());

        yield return new WaitForSeconds(5f);

        // Enable the coffee pot burning the arm on pickup
        coffeePot.EnableBurnArm();
        
        // drink coffee
        StartCoroutine(DrinkCoffee());

        // start the drink task
        Level1TaskManager.StartTaskPourCoffee();

        // prompt to refill the drink
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // eat third slice
        yield return new WaitForSeconds(30f);

        yield return StartCoroutine(EatCake());

        // dont eat and end until coffee pour is attempted
        yield return new WaitUntil(() => poured);

        // thank them for filling it up
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]); // times up!!
        index++;

        // drink coffee
        yield return StartCoroutine(DrinkCoffee());

        // something to show you
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(3f);
        talking = false;
        novaAnimator.SetTrigger("Evidence");
        yield return new WaitForSeconds(1f);
        evidenceSpawner.SpawnTempSpecial();

        // blurb about having to get the evidence
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        Level1TaskManager.StartTaskPolaroid();

        // wait until evidence is grabbed
        yield return new WaitUntil(() => grabbed);

        Level1TaskManager.CompleteTaskPolaroid();
        talking = true;

        GlobalPlayerUIManager.Instance.StopText();
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(10);

        // damn i guess we gotta do it again
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // eat last slice
        yield return StartCoroutine(EatCake());

        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]); // times up!!
        index++;

        // discard food task
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        Level1TaskManager.StartTaskDiscardFood();
        yield return new WaitUntil(() => bagDiscarded);

        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]); // times up!!
        index++;
        Level1TaskManager.StartTaskLeaveCafe();
        sceneExitDoor.gameObject.SetActive(true);
        yield return new WaitForSeconds(10f);
    }
    // Update is called once per frame
    void Update()
    {
        // randomly triggers one, switches every 10 seconds
        timer += Time.deltaTime;

        if (timer >= switchInterval && talking)
        {
            timer = 0f;

            // Pick a random trigger
            int variant = Random.Range(0, 3); // 0,1,2

            // Reset all triggers first (optional, prevents overlap)
            novaAnimator.ResetTrigger("Talk 1");
            novaAnimator.ResetTrigger("Talk 2");
            novaAnimator.ResetTrigger("Talk 3");

            // Set the chosen trigger
            switch (variant)
            {
                case 0:
                    novaAnimator.SetTrigger("Talk 1");
                    break;
                case 1:
                    novaAnimator.SetTrigger("Talk 2");
                    break;
                case 2:
                    novaAnimator.SetTrigger("Talk 3");
                    break;
            }
        }
    }

    public void ShowScoreboard()
    {
        ScoreboardUIHandler.Instance.ShowScoreboard();
    }
}
