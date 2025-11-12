using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ScoreboardUIHandler : MonoBehaviour
{
    public static ScoreboardUIHandler Instance;
    public float pauseAfterTitle = 0.5f;
    public float scoreCountDuration = 1f; // how fast number goes up, perhaps scale with amount ltr
    public float betweenTitles = 0.3f;

    public string evidenceCountText = "Number of evidence collected";
    public string dominanteLeftText = "Hand that was used the most ";
    public string hurtDateCountText = "Number of times you've hit your date";

    [SerializeField] private UIDocument scoreboardDoc;
    private VisualElement scoreboardContainer;
    private Label scoreboardContent;
    private Label letterGrade;
    private Label letterGradeTitle;

    private int textWidth = 55;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        var root = scoreboardDoc.rootVisualElement;
        // find all needed UI elements
        scoreboardContent = root.Query<Label>("ScoreboardContent").First();
        scoreboardContainer = root.Query<VisualElement>("ScoreboardContainer").First();
        letterGrade = root.Query<Label>("LetterGrade").First();
        letterGradeTitle = root.Query<Label>("LetterGradeTitle").First();

        // disable for now
        letterGradeTitle.visible = false;
        letterGrade.visible = false;
        scoreboardContent.visible = false;
        scoreboardContainer.visible = false;

        // ShowScoreboard(); // for testing
    }

    // given a event and its count, dispaly on UI and also increment count
    public void ShowScoreboard()
    {
        scoreboardContent.visible = true;
        scoreboardContainer.visible = true;
        // get scores from scorekeeper
        ScoreboardData data = ScoreKeeper.Instance.GetScores();

        // test
        // ScoreboardData data = new ScoreboardData();

        // data.evidenceCount = 100;
        // data.dominanteLeft = false;
        // data.hurtDateCount = 2;
        // data.letter = "G";

        // Scoring test1 = new Scoring("One time", 1, false, true, 0, 1);
        // Scoring test12 = new Scoring("One time failed :(", 1, false, true, 0, 0);
        // Scoring test2 = new Scoring("Percent", 2, true, false, 2000, 65);
        // Scoring test3 = new Scoring("Normal", 2, false, false, 2000, 32);

        // data.events = new List<Scoring>
        // {
        //     test1,
        //     test12,
        //     test2,
        //     test3
        // };

        // clear content
        scoreboardContent.text = "";

        StartCoroutine(AnimateScoreboardRoutine(data));
    }

    /// <summary>
    /// adds dashes to space title from result
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    string AddDashes(string title)
    {
        int dashCount = Mathf.Max(0, textWidth - title.Length);
        string dashes = new string('-', dashCount);
        return title + " " + dashes;
    }

    /// <summary>
    /// in charge of printing all the data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    IEnumerator AnimateScoreboardRoutine(ScoreboardData data)
    {
        scoreboardContent.visible = true;
        // // print reoccuring data first
        string content = "";
        scoreboardContent.text = content;

        // // pause a bit
        // yield return new WaitForSeconds(pauseAfterTitle);

        // increment number
        // float elapsed = 0f;
        // int startScore = 0;
        // int targetScore = data.evidenceCount;

        // while (elapsed < scoreCountDuration)
        // {
        //     elapsed += Time.deltaTime;
        //     float t = Mathf.Clamp01(elapsed / scoreCountDuration);
        //     int current = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
        //     scoreboardContent.text = content + " " + current.ToString() + "x";
        //     yield return null;
        // }

        // content = content + " " + targetScore + "x\n";
        // scoreboardContent.text = content;

        // yield return new WaitForSeconds(betweenTitles); // small pause
        float elapsed;
        int startScore = 0;
        int targetScore = 0;

        if (data.hurtDateCount != 0)
        {
            content = content + AddDashes(hurtDateCountText);
            scoreboardContent.text = content;

            // pause a bit
            yield return new WaitForSeconds(pauseAfterTitle);

            // increment number
            elapsed = 0f;
            startScore = 0;
            targetScore = data.hurtDateCount;

            while (elapsed < scoreCountDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / scoreCountDuration);
                int current = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
                scoreboardContent.text = content + " " + current.ToString() + "x";
                yield return null;
            }

            content = content + " " + targetScore + "x\n";
            scoreboardContent.text = content;

            // pause a bit
            yield return new WaitForSeconds(pauseAfterTitle);
        }

        // event stuff
        foreach (var e in data.events)
        {
            if (e.oneTime)
            {
                string condition;
                if (e.status > 0)
                {
                    // success
                    condition = "SUCCESS";
                }
                else
                {
                    //fail
                    condition = "FAIL";
                }

                content = content + AddDashes(e.title);
                scoreboardContent.text = content;

                // pause a bit
                yield return new WaitForSeconds(pauseAfterTitle);

                content = content + " " + condition + "\n";
            }
            else
            {
                content = content + AddDashes(e.title);
                scoreboardContent.text = content;

                // pause a bit
                yield return new WaitForSeconds(pauseAfterTitle);

                // increment number
                elapsed = 0f;
                float startPercent = 0;
                float targetPercent = (float)e.status / (float)e.maxCount * 100f;
                if (e.percent)
                {
                    while (elapsed < scoreCountDuration)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / scoreCountDuration);
                        int current = Mathf.RoundToInt(Mathf.Lerp(startPercent, targetPercent, t));
                        scoreboardContent.text = content + " " + current.ToString() + "%";
                        yield return null;
                    }

                    content = content + " " + targetPercent + "%\n";
                }
                else
                {
                    while (elapsed < scoreCountDuration)
                    {
                        elapsed += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsed / scoreCountDuration);
                        int current = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
                        scoreboardContent.text = content + " " + current.ToString() + "x";
                        yield return null;
                    }

                    content = content + " " + targetScore + "x\n";
                }
            }

            scoreboardContent.text = content;
        }

        // Dominant hand
        content = content + dominanteLeftText;
        int dashCount = Mathf.Max(0, textWidth - dominanteLeftText.Length - 6);
        if (data.dominanteLeft)
        {
            string dashes = new string('-', dashCount);
            content = content + dashes + " Left\n";
        }
        else
        {
            string dashes = new string('-', dashCount);
            content = content + dashes + " Right\n";
        }
        scoreboardContent.text = content;

        yield return new WaitForSeconds(betweenTitles); // small pause


        // letter grade
        letterGradeTitle.visible = true;

        yield return new WaitForSeconds(2f); // small pause

        letterGrade.text = data.letter;
        letterGrade.visible = true;

        bool inputDetected = false;

        while (!inputDetected)
        {

            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad == null) continue;

                foreach (var control in gamepad.allControls)
                {
                    if (control is ButtonControl button && button.wasPressedThisFrame)
                    {
                        inputDetected = true;
                        break;
                    }
                }

                if (inputDetected)
                    break;
            }

            yield return null; // wait next frame
        }

        // exit back to level select
        CloseScoreboard();
        GlobalLevelManager.Instance.LoadLevelSelectScreen();
    }

    /// <summary>
    /// Close the scoreboard (visibly hide it)
    /// </summary>
    void CloseScoreboard()
    {
        letterGradeTitle.visible = false;
        letterGrade.visible = false;
        scoreboardContent.visible = false;
        scoreboardContainer.visible = false;
    }
}
