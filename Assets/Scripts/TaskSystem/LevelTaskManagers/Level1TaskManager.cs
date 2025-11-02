using UnityEngine;

public class Level1TaskManager : TaskManager
{
    /// <summary>
    /// Use this instance of the Level0TaskManager for tasks that are specific to level 0 (or for base class methods) <br />
    /// For example: The Phone needs to start the Swipe task so it calls Level0TaskManager.StartTaskGoToPhone() (the Phone is already specific to level 0)
    /// See <see cref="TaskManager.GenericInstance"/> for a level agnostic TaskManager Instance example.
    /// </summary>
    public static Level1TaskManager Instance;

    public new void Awake()
    {
        base.Awake();
        Instance = this;
    }
    
    // Level 1 specific tasks
    // Start Level 1 (level 1 intro)
    public static void StartTaskLevel1Intro() { Instance.StartTask("Start1"); }
    public static void CompleteTaskLevel1Intro() { Instance.CompleteTask("Start1"); }
    // Pour Coffee pot
    public static void StartTaskPourCoffee() { Instance.StartTask("Coffee"); }
    public static void CompleteTaskPourCoffee() { Instance.CompleteTask("Coffee"); }
    // Eat food
    public static void StartTaskEatFood() { Instance.StartTask("Food"); }
    public static void CompleteTaskEatFood() { Instance.CompleteTask("Food"); }
    // Pickup evidence
    public static void StartTaskPickupEvidence() { Instance.StartTask("Evidence"); }
    public static void CompleteTaskPickupEvidence() { Instance.CompleteTask("Evidence"); }

    // Throw food in garbage can
    public static void StartTaskDiscardFood() { Instance.StartTask("DiscardFood"); }
    public static void CompleteTaskDiscardFood() { Instance.CompleteTask("DiscardFood"); }

    // Sit down after walking
    public static void StartTaskSitBackDown() { Instance.StartTask("SitBackDown"); }
    public static void ResetTaskSitBackDown() { Instance.RestartTask("SitBackDown"); }
    public static void CompleteTaskSitBackDown() { Instance.CompleteTask("SitBackDown"); }

    // Leave the cafe
    public static void StartTaskLeaveCafe() { Instance.StartTask("Leave"); }
    public static void CompleteTaskLeaveCafe() { Instance.CompleteTask("Leave"); }
}
