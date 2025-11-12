using FMODUnity;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class HandConsole : Interactable
{
    public HandMovement target;
    public HeadConsole headConsole; // for grapple arm jamming
    private bool _canInteract = true;
    public bool left;

    public AudioSource audioSource;
    public StudioEventEmitter enterSfx;

    private GameObject _currPlayer;

    private TerminalUIHandler uIHandler;

    void Start()
    {
        DisableOutline();
        if (left)
        {
            uIHandler = LeftArmUIHandler.Instance;
        }
        else
        {
            uIHandler = RightArmUIHandler.Instance;
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
            TutorialManager.Instance.interactArmTerminal = true;
        }

        // if interactable
        player.GetComponent<Player>().TurnOff();
        target.TurnOn(player);
        target.headConsole = headConsole;
        _canInteract = false;
        _currPlayer = player;

        if (enterSfx != null)
            enterSfx.Play();
        uIHandler.ShowUI(player);
    }

    public override void Return(GameObject player)
    {
        player.GetComponent<Player>().TurnOn();
        target.TurnOff(player);
        _canInteract = true; // current player leaves

        _currPlayer = null;
        uIHandler.HideUI(player);
    }

    public override bool CanInteract()
    {
        return _canInteract;
    }

    public void DisableInteract()
    {
        hoverMessage = "[ARM DISABLED]";
        msgColour = new Color(1, 0, 0, 1);
        outlineColour = new Color(1, 0, 0, 1);

        if (_currPlayer != null)
        {
            PlayerInteract playerInteract = _currPlayer.GetComponent<PlayerInteract>();
            if (playerInteract != null)
            {
                if (target.currObj != null)
                {
                    target.StopInteractingWithObject(target.currObj);
                }
                playerInteract.LeaveCurrInteractable();
            }
        }

        _canInteract = false;
        // handRigTarget.GetComponent<HandMovement>().JamArm(true);
        // headConsole.JamArm(left, true);
    }

    public void EnableInteract()
    {
        _canInteract = true;
        hoverMessage = "Control Arm";
        msgColour = new Color(1, 1, 1, 1);
        outlineColour = new Color(1, 1, 1, 1);
        _canInteract = true;
        // handRigTarget.GetComponent<HandMovement>().JamArm(false);
        // headConsole.JamArm(left, false);
    }
}
