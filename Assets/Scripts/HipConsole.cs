using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class HipConsole : Interactable
{
    private bool _canInteract = true;
    [SerializeField] Transform robotBody;
    public StudioEventEmitter enterSfx;

    private TerminalUIHandler uIHandler;

    [SerializeField] Transform playerChair;
    private TriggerSeat triggerSeat;
    private GoKartTriggerSeat goKartTriggerSeat;
    void Start()
    {
        DisableOutline();
        uIHandler = LegUIHandler.Instance;
        if (playerChair)
        {
            triggerSeat = playerChair.GetComponent<TriggerSeat>();
            goKartTriggerSeat = playerChair.GetComponent<GoKartTriggerSeat>();
        }

    }
    public override void Interact(GameObject player)
    {
        if (!_canInteract)
        {
            return;
        }

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.interactLegTerminal = true;
        }

        player.GetComponent<Player>().TurnOff();
        player.GetComponent<Player>().switchToLegs(robotBody);

        _canInteract = false;
        if (enterSfx != null)
            enterSfx.Play();
        uIHandler.ShowUI(player);

        // this is only for level 1
        if (triggerSeat != null && triggerSeat.PlayerInsideSeat())
        {
            triggerSeat.StandRobot();
            if (Level1TaskManager.Instance.GetTaskData("Leave") == null && playerChair)
            {
                Collider collider = triggerSeat.GetComponent<Collider>();
                collider.enabled = true;
            }
        }

        // Level 2 gokart exit: leg console teleports player back to trigger entry point.
        if (goKartTriggerSeat != null && goKartTriggerSeat.PlayerInsideSeat())
        {
            goKartTriggerSeat.ExitRobotToEntry();
        }
    }

    public override void Return(GameObject player)
    {
        RuntimeManager.PlayOneShot("event:/SFX/Interior/terminal_exit");

        player.GetComponent<Player>().TurnOn();
        player.GetComponent<Player>().switchOffLegs();

        _canInteract = true;
        if (enterSfx != null)
            enterSfx.Stop();
        uIHandler.HideUI(player);
        if (triggerSeat != null)
        {
            if (playerChair && triggerSeat.PlayerInsideSeat())
            {
                triggerSeat.SeatRobot();
            }
        }

        GlobalPlayerUIManager.Instance.StopWalkingShake();
    }


    public override bool CanInteract()
    {
        return _canInteract;
    }

    public void DisableInteract()
    {
        _canInteract = false;
        hoverMessage = "[LEGS DISABLED]";
        msgColour = new Color(1, 0, 0, 1);
        outlineColour = new Color(1, 0, 0, 1);
    }

    public void EnableInteract()
    {
        _canInteract = true;
        hoverMessage = "Control Legs";
        msgColour = new Color(1, 1, 1, 1);
        outlineColour = new Color(1, 1, 1, 1);
    }
}
