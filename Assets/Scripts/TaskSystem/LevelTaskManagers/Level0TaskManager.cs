using UnityEngine;

public class Level0TaskManager : TaskManager
{
    /// <summary>
    /// Use this instance of the Level0TaskManager for tasks that are specific to level 0 (or for base class methods) <br />
    /// For example: The Phone needs to start the Swipe task so it calls Level0TaskManager.StartTaskGoToPhone() (the Phone is already specific to level 0)
    /// See <see cref="TaskManager.GenericInstance"/> for a level agnostic TaskManager Instance example.
    /// </summary>
    public static Level0TaskManager Instance;

    public new void Awake()
    {
        base.Awake();
        Instance = this;
    }

    // Level 0 specific tasks
    // 1. Go to Phone
    public static void StartTaskGoToPhone() { Instance.StartTask("GoPhone"); }
    public static void CompleteTaskGoToPhone() { Instance.CompleteTask("GoPhone"); }
    // 2. Pickup Phone
    public static void StartTaskPickupPhone() { Instance.StartTask("PickupPhone"); }
    public static void CompleteTaskPickupPhone() { Instance.CompleteTask("PickupPhone"); }
    // 3. Swipe on Phone
    public static void StartTaskSwipe() { Instance.StartTask("Swipe"); }
    public static void CompleteTaskSwipe() { Instance.CompleteTask("Swipe"); }

    public static void StartTaskUnlock() { Instance.StartTask("Unlock"); }
    public static void CompleteTaskUnlock() { Instance.CompleteTask("Unlock"); }
    // 4. Leave Phone
    public static void StartTaskLeavePhone() { Instance.StartTask("LeavePhone"); }
    public static void CompleteTaskLeavePhone() { Instance.CompleteTask("LeavePhone"); }
    
    public static void ClearAllLevel1Tasks() { Instance.ClearActiveTasks(); }
}
