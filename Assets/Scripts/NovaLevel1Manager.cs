using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class NovaLevel1Manager : MonoBehaviour
{
    public static NovaLevel1Manager Instance;
    public Animator novaAnimator;

    public List<DialogueScriptableObj> dialogues = new List<DialogueScriptableObj>();

    public List<GameObject> cakeSlices = new List<GameObject>();

    public EvidenceSpawner evidenceSpawner;
    public GameObject novaRightHandCake;

    public AudioSource eatSource;
    public StudioEventEmitter biteSfx;

    public bool grabbed = false;
    public bool bagDiscarded = false;

    public Transform bag;
    public GameObject garbageCan;
    public GameObject tableCup;
    public GameObject handCup;
    public bool talking = false;
    private float switchInterval = 10f;
    private float timer = 0f;
    private int cakeIndex = 0;
    public bool ate = false;
    public bool firesOut = false;

    public Coroutine levelCoroutine;

    void Start()
    {
        Instance = this;
        StartCoroutine(WaitForTaskManager());
    }

    IEnumerator EatCake()
    {
        talking = false;
        novaAnimator.SetTrigger("Eat");
        cakeSlices[cakeIndex].SetActive(false);
        cakeIndex++;
        yield return new WaitForSeconds(1f);
        novaRightHandCake.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        biteSfx.Play();
        novaRightHandCake.SetActive(false);
        yield return new WaitForSeconds(1f);
        talking = true;
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

        // start the drink task
        Level1TaskManager.StartTaskPourCoffee();
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
        transform.position = new Vector3(254.8f, -26.8f, 9.8f);
        transform.localRotation = new Quaternion(0, 0, 0, 0);

        talking = true;

        // blurb about herself
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(12f);

        // blurb about food
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(7f);
        Level1TaskManager.StartTaskEatFood();
        yield return new WaitUntil(() => ate);

        // evidence falls out
        talking = false;
        novaAnimator.SetTrigger("Evidence");
        yield return new WaitForSeconds(1f);
        evidenceSpawner.SpawnTempSpecial();

        // blurb about having to get the evidence
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        // wait until evidence is grabbed
        yield return new WaitUntil(() => grabbed);
        talking = true;

        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;
        yield return new WaitForSeconds(15f);

        yield return StartCoroutine(EatCake());

        // talk about having to do it before she finishes
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        yield return new WaitForSeconds(35f);

        //Level1TaskManager.StartTaskPickupEvidence();

        //yield return new WaitForSeconds(20f);

        // after a while she eats another slice
        yield return StartCoroutine(EatCake());

        yield return new WaitForSeconds(5f);

        // drink coffee
        StartCoroutine(DrinkCoffee());

        // prompt to refill the drink
        GlobalPlayerUIManager.Instance.LoadText(dialogues[index]);
        index++;

        yield return new WaitUntil(() => firesOut);

        // eat third slice
        yield return new WaitForSeconds(30f);

        yield return StartCoroutine(EatCake());

        // eat last slice
        yield return new WaitForSeconds(30f);

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
