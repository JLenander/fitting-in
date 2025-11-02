using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// The base class for a level's Task Manager.
/// This has methods for initializing and keeping track of the player's tasks for a particular level.
/// Implementing classes should have explicit methods associated with the task IDs that aren't exposed outside
/// of the class so no magic strings are needed. <br />
/// Child classes should implement public task specific version for StartTask, CompleteTask
/// </summary>
public abstract class TaskManager : MonoBehaviour
{
    /// <summary>
    /// Use this instance of the TaskManager for methods and Tasks that are not level specific. <br />
    /// For example: The Head Console may need to start the FixLeftArm task but the HeadConsole script
    /// is the same across levels so it calls this GenericInstance.
    /// See <see cref="Level0TaskManager.Instance"/> for a level specific TaskManager Instance example.
    /// </summary>
    public static TaskManager GenericInstance;
    
    BrainUIHandler uIHandler;

    private Dictionary<string, Task> _tasks = new ();

    private List<Task> activeTasks = new (); // a list of active tasks ordered chronologically

    // The lists of tasks for this level (for this Task Manager)
    [System.Serializable]
    public class TaskData
    {
        public TaskInfoSO taskInfoSO;
        public UnityEvent onStart;
        public UnityEvent onComplete;
    }
    
    public List<TaskData> tasksToRegister = new ();
    
    public void Awake()
    {
        // This should overwrite the previous instance as it will be a child level-specific Task Manager that is
        // setting this when the level scene loads.
        GenericInstance = this;
        
        RegisterTasks();
    }

    private void RegisterTasks()
    {
        foreach (var data in tasksToRegister)
        {
            TaskInfoSO taskInfo = data.taskInfoSO;

            var task = new Task
            {
                id = taskInfo.id,
                title = taskInfo.title,
                description = taskInfo.description,
                urgency = taskInfo.urgency,
                location = taskInfo.location,
                targetProgress = taskInfo.targetProgress,
                onStart = data.onStart,
                onComplete = data.onComplete
            };

            _tasks.TryAdd(task.id, task);
        }
    }
    
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Common tasks (all levels have these tasks)
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public static void StartTaskFixLeftArm() { GenericInstance.StartTask("FixLeft"); }
    public static void CompleteTaskFixLeftArm() { GenericInstance.CompleteTask("FixLeft"); }
    public static void StartTaskFixRightArm() { GenericInstance.StartTask("FixRight"); }
    public static void CompleteTaskFixRightArm() { GenericInstance.CompleteTask("FixRight"); }
    
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // TaskManager methods
    // (Protected methods like StartTask are protected to encourage no magic string usage as child classes
    // should have methods like the common task StartTaskFixRightArm() in this class).
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public Task GetTask(string id)
    {
        _tasks.TryGetValue(id, out var task);
        return task;
    }

    // public void AddProgress(string id, int amount)
    // {
    //     if (_tasks.TryGetValue(id, out var task))
    //     {
    //         if (!task.isActive)
    //             StartTask(id);
    //         task.AddProgress(amount);
    //     }
    //     else
    //         Debug.LogWarning($"Task '{id}' not found!");
    // }

    protected void StartTask(string id)
    {
        if (_tasks.TryGetValue(id, out var task))
        {
            task.canStart = true;
            task.StartTask();
        }
        else
        {
            Debug.LogError($"Task {id} can't start: not found");
        }
    }

    protected void CompleteTask(string id)
    {
        if (_tasks.TryGetValue(id, out var task))
        {
            task.CompleteTask();
        }
        else
        {
            Debug.LogError($"Task {id} can't complete: not found");
        }
    }

    public void RestartTask(string id)
    {
        if (_tasks.TryGetValue(id, out var task))
        {
            task.RestartTask();
        }
    }

    public void AppendActiveTask(Task task)
    {
        activeTasks.Add(task);
        PassDataUI();
        if (task.id != "FixRight" && task.id != "FixLeft")
        {
            PopUpUIHandler.Instance.ShowPopUp();
        }
    }

    public void RemoveActiveTask(Task task)
    {
        activeTasks.Remove(task);
        PassDataUI();
    }

    // called whenever active tasks are changed
    private void PassDataUI()
    {
        List<string> taskNames = new List<string>();

        foreach (Task task in activeTasks)
        {
            taskNames.Add(task.title);
        }

        BrainUIHandler.Instance.UpdateTasks(taskNames);
    }

    public Task GetTaskData(string name)
    {
        // get data of active task from name
        foreach (Task task in activeTasks)
        {
            if (task.title == name)
            {
                return task;
            }
        }

        return null;
    }

    public List<Task> GetActiveTasks()
    {
        return activeTasks;
    }

}
