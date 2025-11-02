using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;


public class BrainConsole : Interactable
{
    public int unlockDuration = 3;
    private bool _canInteract = true;
    [SerializeField] GameObject leftDoorObj;
    [SerializeField] GameObject rightDoorObj;

    [SerializeField] GameObject leftEscapeDoorObj;
    [SerializeField] GameObject rightEscapeDoorObj;

    public AudioSource interactSound;
    public AudioSource buttonSound;

    private Door leftDoor, rightDoor, leftEscapeDoor, rightEscapeDoor;
    private InputAction _leftTriggerAction, _rightTriggerAction,
                        _leftBumperAction, _rightBumperAction, _lookAction;

    private BrainUIHandler uIHandler;

    private bool task;

    private bool leftLock, rightLock;

    private bool wasBumperPressed = false;
    private int lastStickDir = 0;

    private void Start()
    {
        DisableOutline();
        leftDoor = leftDoorObj.GetComponent<Door>();
        rightDoor = rightDoorObj.GetComponent<Door>();
        leftEscapeDoor = leftEscapeDoorObj.GetComponent<Door>();
        rightEscapeDoor = rightEscapeDoorObj.GetComponent<Door>();

        StartCoroutine(WaitForBrainUIHandler());

        task = true;
        leftLock = true;
        rightLock = true;
    }

    IEnumerator WaitForBrainUIHandler()
    {
        yield return new WaitUntil(() => BrainUIHandler.Instance != null);
        uIHandler = BrainUIHandler.Instance;
    }

    public override void Interact(GameObject player)
    {
        player.GetComponent<Player>().TurnOff();
        _canInteract = false;

        var input = player.GetComponent<PlayerInput>();
        _leftTriggerAction = input.actions.FindAction("LeftTrigger");
        _rightTriggerAction = input.actions.FindAction("RightTrigger");

        _leftBumperAction = input.actions.FindAction("LeftBumper");
        _rightBumperAction = input.actions.FindAction("RightBumper");

        uIHandler.ShowContainer(player);

        _lookAction = input.actions.FindAction("Look");

        if (interactSound != null)
            interactSound.Play();

        // hide popup
        PopUpUIHandler.Instance.HideNewTaskPopUp();

        //playerTaskPanel.SetActive(true);
        //UpdateTaskList();
    }
    public override void Return(GameObject player)
    {
        player.GetComponent<Player>().TurnOn();
        //playerTaskPanel.SetActive(false);
        _canInteract = true;

        uIHandler.HideContainer(player);
    }

    private void Update()
    {
        if (_canInteract) return;   // no one is on the console

        bool bumperPress =
                (_leftBumperAction != null && _leftBumperAction.ReadValue<float>() > 0.1f) ||
                (_rightBumperAction != null && _rightBumperAction.ReadValue<float>() > 0.1f);

        if (bumperPress && !wasBumperPressed)
        {
            task = !task;
            uIHandler.SwitchScreen();
        }

        wasBumperPressed = bumperPress;

        if (!task)
        {
            // unlock left door 
            if (_leftTriggerAction != null && _leftTriggerAction.ReadValue<float>() > 0.1f && leftLock)
            {
                if (leftDoor != null && leftEscapeDoor != null)
                {
                    leftDoor.UnlockDoor();
                    leftEscapeDoor.UnlockDoor();

                    leftLock = false;

                    StartCoroutine(LockDoorRoutine(true));
                }
            }

            // unlock right door 
            if (_rightTriggerAction != null && _rightTriggerAction.ReadValue<float>() > 0.1f && rightLock)
            {
                if (rightDoor != null && rightEscapeDoor != null)
                {
                    rightDoor.UnlockDoor();
                    rightEscapeDoor.UnlockDoor();

                    rightLock = false;

                    StartCoroutine(LockDoorRoutine(false));
                }
            }
        }
        else
        {
            // handle task UI switching
            float rightInput = _lookAction.ReadValue<Vector2>().y;

            int dir = 0;
            if (rightInput > 0.5)
            {
                dir = -1;
            }
            else if (rightInput < -0.5)
            {
                dir = 1;
            }

            if (dir != 0 && lastStickDir == 0)
            {
                if (buttonSound != null)
                    buttonSound.Play();
                uIHandler.ChangeActiveTask(dir == -1);
            }

            lastStickDir = dir;
        }
    }

    IEnumerator LockDoorRoutine(bool left)
    {
        Debug.Log("Left door coroutine");
        uIHandler.LockDoor(left, unlockDuration);

        yield return new WaitForSeconds(unlockDuration);

        if (left)
        {
            if (leftDoor != null && leftEscapeDoor != null)
            {
                leftDoor.LockDoor();
                leftEscapeDoor.LockDoor();

                leftLock = true;
            }
        }
        else
        {
            if (rightDoor != null && rightEscapeDoor != null)
            {
                rightDoor.LockDoor();
                rightEscapeDoor.LockDoor();

                rightLock = true;
            }
        }
    }

    public override bool CanInteract()
    {
        return _canInteract;
    }

    //private void UpdateTaskList()
    //{
    //    // Get all active tasks for the player
    //    var activeTasks = TaskManager.Instance.GetAllActiveTasks();

    //    if (activeTasks.Count == 0)
    //    {
    //        tasksList.text = "No active tasks";
    //        return;
    //    }

    //    string taskText = "";
    //    foreach (var task in activeTasks)
    //    {
    //        taskText += $"{task.title}: {task.currentProgress}/{task.targetProgress}\n";
    //    }

    //    tasksList.text = taskText;
    //}
}
