using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class BrainUIHandler : TerminalUIHandler
{
    public static BrainUIHandler Instance;
    public PopUpUIHandler popUpUIHandler;
    private VisualElement _doorUI;
    private VisualElement _taskUI;
    private Label _leftDoorText, _rightDoorText, _l2, _r2;
    private Color _neutralColour, _redColour, _blackColour;

    private List<Label> _tasks = new List<Label>();
    private Label _taskDescription;
    private VisualElement _terminalDesc;
    private string _activeTitle;
    private List<string> _visibleTitles = new List<string>();
    private Dictionary<string, Texture2D> _imageCache = new();

    public const int NumTasks = 5;

    public void Awake()
    {
        Instance = this;
    }
    
    protected override void Start()
    {
        base.Start();
        _doorUI = root.Query<VisualElement>("DoorWindow").First();
        _taskUI = root.Query<VisualElement>("TaskWindow").First();
        _leftDoorText = root.Query<Label>("LeftDoorDesc").First();
        _rightDoorText = root.Query<Label>("RightDoorDesc").First();
        _l2 = root.Query<Label>("L2").First();
        _r2 = root.Query<Label>("R2").First();

        InitTaskVisualElements();
        _taskDescription = root.Query<Label>("DescText").First();
        _terminalDesc = root.Query<VisualElement>("TerminalDesc").First();
        
        ColorUtility.TryParseHtmlString("#2BD575", out _neutralColour);
        ColorUtility.TryParseHtmlString("#D52B30", out _redColour);
        ColorUtility.TryParseHtmlString("#1B1B1B", out _blackColour);

        _doorUI.visible = false;
        _activeTitle = null;
        ClearDetails();
    }

    // switch between door and task UI
    public void SwitchScreen()
    {
        _doorUI.visible = !_doorUI.visible;
        _taskUI.visible = !_taskUI.visible;
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
            _activeTitle = null;
            ClearDetails();
            return;
        }

        // no active before, set as newest task
        if (_activeTitle == null || !taskNames.Contains(_activeTitle))
        {
            _activeTitle = taskNames.LastOrDefault();

            if (_activeTitle == null)
            {
                ClearDetails();
                return;
            }
        }

        _visibleTitles = taskNames;

        // display the data in the list of tasks
        // empty out list
        for (int i = 0; i < _tasks.Count; i++)
        {
            _tasks[i].text = "";
        }

        // put in task names, 
        int index = 0;
        for (int i = taskNames.Count - 1; i >= 0; i--)
        {
            _tasks[index].text = taskNames[i];
            index++;
        }

        RefreshTitles();
    }

    // called by brain console to scroll up or down
    public void ChangeActiveTask(bool down)
    {
        if (_activeTitle == null) return;

        // go through visible names to find

        for (int i = 0; i < _visibleTitles.Count; i++)
        {
            if (_visibleTitles[i] == _activeTitle)
            {
                if (down)
                {
                    // next title 
                    if (i != (_visibleTitles.Count - 1))
                        _activeTitle = _visibleTitles[i + 1];
                }
                else
                {
                    // prev title
                    if (i != 0)
                        _activeTitle = _visibleTitles[i - 1];
                }

                RefreshTitles();
                return;
            }
        }
    }

    // only highlight the active title
    void RefreshTitles()
    {
        foreach (Label task in _tasks)
        {
            if (task.text == _activeTitle)
            {
                task.style.backgroundColor = _neutralColour;
                task.style.color = _blackColour;
                UpdateTaskInfo();
            }
            else
            {
                task.style.color = _neutralColour;
                task.style.backgroundColor = new Color(0f, 0f, 0f, 0f);
            }
        }
    }

    void ClearDetails()
    {
        _taskDescription.text = "";
        _terminalDesc.style.backgroundImage = new StyleBackground();

        _tasks[0].text = "No tasks!";
        _tasks[0].style.color = _neutralColour;
        _tasks[0].style.backgroundColor = new Color(0f, 0f, 0f, 0f);

        for (int i = 1; i < _tasks.Count; i++)
        {
            _tasks[i].text = "";
            _tasks[i].style.backgroundColor = new Color(0f, 0f, 0f, 0f);
        }
    }

    public void UpdateTaskInfo()
    {
        //string desc, string terminal, string urgency
        Task task = TaskManager.GenericInstance.GetTaskData(_activeTitle);

        _taskDescription.text = task.description;
        UpdateLocationImage(task.location);
    }
    
    private void UpdateLocationImage(string location)
    {
        string imagePath = location switch
        {
            "Legs" => "UI/Terminals/feet",
            "Arms" => "UI/Terminals/both_hands",
            "Right Arm Interior" => "UI/Terminals/r_hand",
            "Left Arm Interior" => "UI/Terminals/l_hand",
            _ => "UI/Terminals/brain"
        };
        Texture2D tex = GetTex(imagePath);
        var background = new StyleBackground(tex);
        _terminalDesc.style.backgroundImage = background;
    }
    
    private Texture2D GetTex(string path)
    {
        if (_imageCache.TryGetValue(path, out var tex))
            return tex;

        tex = Resources.Load<Texture2D>(path);
        _imageCache[path] = tex;
        return tex;
    }


    IEnumerator DoorCountdownRoutine(bool left, int seconds)
    {
        int currSeconds = seconds;
        if (left)
        {
            _leftDoorText.style.color = _neutralColour;
            _l2.visible = false;
        }
        else
        {
            _rightDoorText.style.color = _neutralColour;
            _r2.visible = false;
        }

        while (currSeconds >= 0)
        {
            string content = "UNLOCKED\n---\nTIME 0:0" + currSeconds;

            if (left)
            {
                _leftDoorText.text = content;
            }
            else
            {
                _rightDoorText.text = content;
            }

            currSeconds -= 1;

            yield return new WaitForSeconds(1);
        }

        if (left)
        {
            _leftDoorText.text = "LOCKED";
            _leftDoorText.style.color = _redColour;
            _l2.visible = true;
        }
        else
        {
            _rightDoorText.text = "LOCKED";
            _rightDoorText.style.color = _redColour;
            _r2.visible = true;
        }
    }

    void InitTaskVisualElements()
    {
        for (int i = 1; i <= NumTasks; i++)
        {
            Label task = root.Query<Label>("TaskTitle" + i).First();
            _tasks.Add(task);

            task.text = "";
        }
    }
}
