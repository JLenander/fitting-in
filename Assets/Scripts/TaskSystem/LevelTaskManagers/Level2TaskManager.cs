using UnityEngine;

public class Level2TaskManager : TaskManager
{
    /// <summary>
    /// Use this instance of the Level2TaskManager for tasks that are specific to level 2 (or for base class methods) <br />
    /// For example: The Phone needs to start the Swipe task so it calls Level2TaskManager.StartTaskGoToPhone()
    /// See <see cref="TaskManager.GenericInstance"/> for a level agnostic TaskManager Instance example.
    /// </summary>
    public static Level2TaskManager Instance;

    public new void Awake()
    {
        base.Awake();
        Instance = this;
    }
    
    // Start Level 2 (level 2 intro)
    public static void StartTaskLevel2Intro() { Instance.StartTask("Start2"); }
    public static void CompleteTaskLevel2Intro() { Instance.CompleteTask("Start2"); }
    
    public static void ClearAllLevel2Tasks() { Instance.ClearActiveTasks(); }
}
