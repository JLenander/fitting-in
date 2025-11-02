using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Task
{
    public string id;
    public string title;
    [TextArea] public string description;
    public string location;
    public string urgency;

    public int currentProgress;
    public int targetProgress = 1;

    public bool canStart;
    public bool isActive;

    public UnityEvent onStart;

    public UnityEvent onComplete;

    public bool isCompleted => currentProgress >= targetProgress;

    // start task only if it can start and not completed
    public void StartTask()
    {
        if (isCompleted)
        {
            Debug.LogWarning($"Task '{id}' already completed!");
            return;
        }

        if (!canStart)
        {
            Debug.LogWarning($"Task '{id}' cannot start yet!");
            return;
        }

        currentProgress = 0;
        isActive = true;
        Debug.Log($"Task '{id}' started!");

        onStart?.Invoke();

        TaskManager.GenericInstance.AppendActiveTask(this);
    }

    // Add progress (only if started and not already complete)
    // public void AddProgress()
    // {
    //     if (!canStart)
    //     {
    //         Debug.LogWarning($"Task '{id}' can't progress because it hasn't started.");
    //         return;
    //     }

    //     if (isCompleted)
    //     {
    //         Debug.Log($"Task '{id}' is already completed!");
    //         return;
    //     }
    //     currentProgress = Mathf.Min(currentProgress + amount, targetProgress);
    //     Debug.Log(currentProgress);

    //     if (isCompleted)
    //     {
    //         Debug.Log($"Task '{id}' completed!");
    //         ResetTask();

    //         // TASK COMPLETED
    //         TaskManager.Instance.RemoveActiveTask(this);
    //     }
    // }

    public void CompleteTask()
    {
        if (!canStart)
        {
            Debug.LogWarning($"Task '{id}' can't progress because it hasn't started.");
            return;
        }

        Debug.Log($"Task '{id}' completed!");
        ResetTask();

        onComplete?.Invoke();

        // TASK COMPLETED
        TaskManager.GenericInstance.RemoveActiveTask(this);
    }

    public void ResetTask()
    {
        currentProgress = 0;
        canStart = false;
        isActive = false;
    }

    public void RestartTask()
    {
        Debug.Log($"Task '{id}' restarted!");
        TaskManager.GenericInstance.AppendActiveTask(this);
    }
}
