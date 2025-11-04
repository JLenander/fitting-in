using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;

public class HipConsole : Interactable
{
    private bool _canInteract = true;
    [SerializeField] Transform robotBody;
    public AudioSource audioSource;
    public StudioEventEmitter enterSfx;

    private OverlayUIHandler uIHandler;

    [SerializeField] Transform playerChair;
    private TriggerSeat triggerSeat;
    void Start()
    {
        DisableOutline();
        uIHandler = LegUIHandler.Instance;
        if (playerChair) triggerSeat = playerChair.GetComponent<TriggerSeat>();

    }
    public override void Interact(GameObject player)
    {
        player.GetComponent<Player>().TurnOff();
        player.GetComponent<Player>().switchToLegs(robotBody);

        _canInteract = false;
        if (enterSfx != null)
            enterSfx.Play();
        uIHandler.ShowContainer(player);

        // this is only for level 1
        if (triggerSeat != null)
        {
            triggerSeat.StandRobot();
            if (Level1TaskManager.Instance.GetTaskData("Leave") == null && playerChair)
            {
                triggerSeat.sceneExitDoor.enabled = false;
                Collider collider = triggerSeat.GetComponent<Collider>();
                collider.enabled = true;
            }
        }
    }

    public override void Return(GameObject player)
    {
        player.GetComponent<Player>().TurnOn();
        player.GetComponent<Player>().switchOffLegs();

        _canInteract = true;
        if (enterSfx != null)
            enterSfx.Stop();
        uIHandler.HideContainer(player);
        if (playerChair && triggerSeat.PlayerInsideSeat())
        {
            triggerSeat.SeatRobot();
        }
    }


    public override bool CanInteract()
    {
        return _canInteract;
    }

    public void DisableInteract()
    {
        hoverMessage = "[LEGS DISABLED]";
        msgColour = new Color(1, 0, 0, 1);
        outlineColour = new Color(1, 0, 0, 1);
        // _currPlayer = null;
    }

    public void EnableInteract()
    {
        _canInteract = true;
        hoverMessage = "Control Legs";
        msgColour = new Color(1, 1, 1, 1);
        outlineColour = new Color(1, 1, 1, 1);
    }
}
