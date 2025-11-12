using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class FillCup : MonoBehaviour
{
    [SerializeField] private Transform liquid;
    [FormerlySerializedAs("fullCounter")] [SerializeField] private float secondsTillFull;
    [SerializeField] private float maxFillHeight = 1f;
    [SerializeField] private float initialHeight = 0.1f;

    private bool full = false;

    [SerializeField] private float fillCounter;
    private Vector3 baseScale;

    public Outline outline;

    [SerializeField] private HandConsole leftConsole;
    [SerializeField] private HandConsole rightConsole;

    [SerializeField] private DialogueScriptableObj burnDialogue;

    void Start()
    {
        fillCounter = 0.0f;
        float newYScale = maxFillHeight;
        float yOffset = (newYScale - initialHeight) / 1.5f;
        baseScale = new Vector3(3.5f, initialHeight, 3.5f);
        liquid.localScale = baseScale;
        liquid.localPosition = new Vector3(0f, yOffset, 0f);
        StartCoroutine(WaitForScoreKeeper());
        DisableOutline();
    }

    IEnumerator WaitForScoreKeeper()
    {
        yield return new WaitUntil(() => ScoreKeeper.Instance != null);
        ScoreKeeper.Instance.AddScoring("Filled Nova's coffee", 5, false, true, 0);
    }

    public void StartTask()
    {
        // trigger nova animation (maybe done outside)

        // set coffee to empty

        baseScale = new Vector3(3.5f, initialHeight, 3.5f);
        liquid.localScale = baseScale;
        liquid.localPosition = Vector3.zero;
        fillCounter = 0;

        // allow filling
        full = false;
    }

    public void AddCoffee()
    {
        if (full)
        {
            return;
        }

        EnableOutline();

        fillCounter += Time.deltaTime;
        if (fillCounter > secondsTillFull)
        {
            full = true;
            ScoreKeeper.Instance.IncrementScoring("Filled Nova's coffee");
            Level1TaskManager.CompleteTaskPourCoffee();
        }

        float fillProgress = fillCounter / secondsTillFull;
        float newYScale = Mathf.Lerp(initialHeight, maxFillHeight, fillProgress);
        float yOffset = (newYScale - initialHeight) / 1.5f;

        liquid.localScale = new Vector3(baseScale.x, newYScale, baseScale.z);
        liquid.localPosition = new Vector3(0f, yOffset, 0f);
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }

    public void EnableOutline()
    {
        outline.enabled = true;
    }

    IEnumerator BurnArm()
    {
        yield return new WaitForSeconds(5);

        // start fire
        FireManager.Instance.StartFireArea("leftArm");

        // disable the relevant arm
        leftConsole.DisableInteract();

        // start fire
        FireManager.Instance.StartFireArea("rightArm");

        // disable the relevant arm
        rightConsole.DisableInteract();

        FireManager.Instance.StartFireArea("spawn");

        // output dialogue
        GlobalPlayerUIManager.Instance.LoadText(burnDialogue);


    }
}
