using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BrainUIHandler : TerminalUIHandler
{
    public static BrainUIHandler Instance;
    public PopUpUIHandler popUpUIHandler;
    private VisualElement doorUI;
    private VisualElement taskUI;
    private Label leftDoorText, rightDoorText, l2, r2;
    private Color neutralColour, redColour, blackColour;

    private List<Label> tasks = new List<Label>();
    private Label taskDescription, terminalDesc, urgencyDesc;
    private string activeTitle;
    private List<string> visibleTitles = new List<string>();

    public const int NumTasks = 5;

    public void Awake()
    {
        Instance = this;
    }
    
    protected override void Start()
    {
        base.Start();
        doorUI = root.Query<VisualElement>("DoorWindow").First();
        taskUI = root.Query<VisualElement>("TaskWindow").First();
        leftDoorText = root.Query<Label>("LeftDoorDesc").First();
        rightDoorText = root.Query<Label>("RightDoorDesc").First();
        l2 = root.Query<Label>("L2").First();
        r2 = root.Query<Label>("R2").First();

        InitTaskVisualElements();
        taskDescription = root.Query<Label>("DescText").First();

        terminalDesc = root.Query<Label>("TerminalDesc").First();
        urgencyDesc = root.Query<Label>("UrgencyDesc").First();

        ColorUtility.TryParseHtmlString("#2BD575", out neutralColour);
        ColorUtility.TryParseHtmlString("#D52B30", out redColour);
        ColorUtility.TryParseHtmlString("#1B1B1B", out blackColour);

        doorUI.visible = false;
        activeTitle = null;
        ClearDetails();
    }

    // switch between door and task UI
    public void SwitchScreen()
    {
        doorUI.visible = !doorUI.visible;
        taskUI.visible = !taskUI.visible;
    }

    // lock one of the doors
    public void LockDoor(bool left, int seconds)
    {
        StartCoroutine(DoorCountdownRoutine(left, seconds));
    }

    public void UpdateTasks(List<string> taskNames)
    {
        if (taskNames == null || taskNames.Count == 0)
        {
            // no more tasks, empty everything
            activeTitle = null;
            ClearDetails();
            return;
        }

        // no active before, set as newest task
        if (activeTitle == null || !taskNames.Contains(activeTitle))
        {
            activeTitle = taskNames.LastOrDefault();

            if (activeTitle == null)
            {
                ClearDetails();
                return;
            }
        }

        visibleTitles = taskNames;

        // display the data in the list of tasks
        // empty out list
        for (int i = 0; i < tasks.Count; i++)
        {
            tasks[i].text = "";
        }

        // put in task names, 
        int index = 0;
        for (int i = taskNames.Count - 1; i >= 0; i--)
        {
            tasks[index].text = taskNames[i];
            index++;
        }

        RefreshTitles();
    }

    // called by brain console to scroll up or down
    public void ChangeActiveTask(bool down)
    {
        if (activeTitle == null) return;

        // go through visible names to find

        for (int i = 0; i < visibleTitles.Count; i++)
        {
            if (visibleTitles[i] == activeTitle)
            {
                if (down)
                {
                    // next title 
                    if (i != (visibleTitles.Count - 1))
                        activeTitle = visibleTitles[i + 1];
                }
                else
                {
                    // prev title
                    if (i != 0)
                        activeTitle = visibleTitles[i - 1];
                }

                RefreshTitles();
                return;
            }
        }
    }

    // only highlight the active title
    void RefreshTitles()
    {
        foreach (Label task in tasks)
        {
            if (task.text == activeTitle)
            {
                task.style.backgroundColor = neutralColour;
                task.style.color = blackColour;
                UpdateTaskInfo();
            }
            else
            {
                task.style.color = neutralColour;
                task.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            }
        }
    }

    void ClearDetails()
    {
        taskDescription.text = "";
        terminalDesc.text = "";
        urgencyDesc.text = "";

        tasks[0].text = "No tasks!";
        tasks[0].style.color = neutralColour;
        tasks[0].style.backgroundColor = new Color(0f, 0f, 0f, 0f);

        for (int i = 1; i < tasks.Count; i++)
        {
            tasks[i].text = "";
            tasks[i].style.backgroundColor = new Color(0f, 0f, 0f, 0f);
        }
    }

    public void UpdateTaskInfo()
    {
        //string desc, string terminal, string urgency
        Task task = TaskManager.GenericInstance.GetTaskData(activeTitle);

        taskDescription.text = task.description;
        terminalDesc.text = task.location;

        string urgencyText = task.urgency;
        urgencyDesc.text = urgencyText;

        if (urgencyText == "High")
            urgencyDesc.style.color = redColour;
        else
            urgencyDesc.style.color = neutralColour;
    }

    IEnumerator DoorCountdownRoutine(bool left, int seconds)
    {
        int currSeconds = seconds;
        if (left)
        {
            leftDoorText.style.color = neutralColour;
            l2.visible = false;
        }
        else
        {
            rightDoorText.style.color = neutralColour;
            r2.visible = false;
        }

        while (currSeconds >= 0)
        {
            string content = "UNLOCKED\n---\nTIME 0:0" + currSeconds;

            if (left)
            {
                leftDoorText.text = content;
            }
            else
            {
                rightDoorText.text = content;
            }

            currSeconds -= 1;

            yield return new WaitForSeconds(1);
        }

        if (left)
        {
            leftDoorText.text = "LOCKED";
            leftDoorText.style.color = redColour;
            l2.visible = true;
        }
        else
        {
            rightDoorText.text = "LOCKED";
            rightDoorText.style.color = redColour;
            r2.visible = true;
        }
    }

    void InitTaskVisualElements()
    {
        for (int i = 1; i <= NumTasks; i++)
        {
            Label task = root.Query<Label>("TaskTitle" + i).First();
            tasks.Add(task);

            task.text = "";
        }
    }
}
