using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// In charge of keeping, adding, and reducing scores
/// </summary>
/// 
/// Players will also be able to view individual stats such as 
/// which arm was used to grab the most, 
/// their arm accuracy, 
/// how many times they’ve launched their arm, 
/// how many evidence objects they have collected, 
/// what penalty actions they triggered, 
/// and whether they succeeded in the level’s special events
public class ScoreKeeper : MonoBehaviour
{
    public static ScoreKeeper Instance;

    // private ISplitscreenUIHandler _splitscreenUIHandler; // remove later
    private int overallScore; // player score
    public int maxScore; // what we think is the max score for the level
    // the rest of the special events will be aggregated in a list
    public List<Scoring> events;
    private int evidenceCount; // num of evidence gathered
    private int leftGrabCount; // number of times left hand used grab
    private int rightGrabCount; // number of times right hand used grab
    private int hurtDateCount; // num of times accidentally hit date
    private float elapsedTime;

    void Awake()
    {
        Instance = this; // easier to reference
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        overallScore = 0;
        evidenceCount = 0;
        leftGrabCount = 0;
        rightGrabCount = 0;
        hurtDateCount = 0;
        events = new List<Scoring>();
        elapsedTime = 0f;
        // _splitscreenUIHandler = FindAnyObjectByType<SplitscreenUIHandler>();
        // _splitscreenUIHandler.ChangeScoreText(0); // for debugging for now
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// Used to add or reduce score
    /// </summary>
    /// <param name="score"></param>
    public void ModifyScore(int score)
    {
        overallScore += score;
        // _splitscreenUIHandler.ChangeScoreText(overallScore);
    }

    /// <summary>
    /// To record arm grab counts
    /// </summary>
    /// <param name="left"></param>
    public void IncreaseArmCount(bool left)
    {
        if (left)
        {
            leftGrabCount++;
        }
        else
        {
            rightGrabCount++;
        }
    }

    /// <summary>
    /// log evidence success
    /// </summary>
    public void IncrementEvidence()
    {
        evidenceCount++;
    }

    /// <summary>
    /// Log hurting the date by accident
    /// </summary>
    public void IncrementHurtDate()
    {
        hurtDateCount++;
    }

    // for special events to tell scorekeeper about them
    public void AddScoring(string title, int score, bool percent, bool oneTime, int maxCount)
    {
        Scoring newEvent = new Scoring(title, score, percent, oneTime, maxCount);

        events.Add(newEvent);
    }

    /// <summary>
    /// To add to specific scoring (Shouldn't be used too often)
    /// </summary>
    /// <param name="title"></param>
    public void IncrementScoring(string title)
    {
        for (int i = 0; i < events.Count; i++)
        {
            // if title match add count
            if (events[i].title == title)
            {
                Debug.Log("Increased score for" + title);
                Scoring updated = events[i];
                updated.status++;
                events[i] = updated;
                return;
            }
        }

        // couldnt find event, print
        Debug.Log("Event not found");
    }

    int OverallEventScore()
    {
        int sum = 0;
        for (int i = 0; i < events.Count; i++)
        {
            Scoring singleEvent = events[i];
            if (singleEvent.oneTime) // a one time event
            {
                sum += singleEvent.score;
            }
            else if (singleEvent.percent) // a percentage complete event
            {
                sum += (int)(singleEvent.score * (float)(singleEvent.status / singleEvent.maxCount));
            }
            else // a countable event
            {
                sum += singleEvent.score * singleEvent.status;
            }

        }

        return sum;
    }

    float FinalTime()
    {
        // simple returns a time finished
        return elapsedTime;
        // // give the player a letter grade based on what they got compared
        // if (maxScore == 0) return "N/A";
        // if (maxScore < 0) return "F";

        // float ratio = currScore / maxScore;

        // if (ratio >= 0.90f) return "S";
        // if (ratio >= 0.80f) return "A";
        // if (ratio >= 0.65f) return "B";
        // if (ratio >= 0.50f) return "C";
        // if (ratio >= 0.35f) return "D";
        // return "F";
    }

    // called by scoreboard to get all data (done once at the end)
    public ScoreboardData GetScores()
    {
        ScoreboardData scoreboardData = new ScoreboardData();

        scoreboardData.evidenceCount = evidenceCount; // give evidence collected

        // calculate dominant hand
        scoreboardData.dominanteLeft = leftGrabCount > rightGrabCount;
        scoreboardData.hurtDateCount = hurtDateCount;

        scoreboardData.time = FinalTime();

        scoreboardData.events = events;

        return scoreboardData;
    }

    // number incrementer
    // IEnumerator NumberRoutine()
    // {

    // }

}

public struct ScoreboardData
{
    public int evidenceCount;
    public bool dominanteLeft;
    public int hurtDateCount;
    public float time;
    public List<Scoring> events;
}

public struct Scoring
{
    public string title; // name of the thing
    public int score; // how much each would score
    public int status; // how many times they succeeded
    public int maxCount; // how many times they need to succeed for 100%
    public bool percent; // if this is a percentage task (percent: status/maxCount)
    public bool oneTime; // if this is a one time task (status > 0 = success)

    public Scoring(string title, int score, bool percent, bool oneTime, int maxCount, int status = 0)
    {
        this.title = title;
        this.score = score;
        this.status = status;
        this.percent = percent;
        this.oneTime = oneTime;
        this.maxCount = maxCount;
    }
}