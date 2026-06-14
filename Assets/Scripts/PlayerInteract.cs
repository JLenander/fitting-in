using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    // [SerializeField] private GameObject hudPrefab;
    public float reach = 1f;
    private Camera fpsCam;
    public Interactable currentItem;

    private InputAction _interactAction;
    private InputAction _returnAction;

    public Interactable interacting;
    private int playerId;

    // Variables to enable interact and return to share an action
    private bool _actionLock;
    private LayerMask _interactLayerMask;

    void Awake()
    {
        var input = GetComponent<PlayerInput>();
        _interactAction = InputActionMapper.GetPlayerInteractAction(input);
        _returnAction = InputActionMapper.GetPlayerReturnAction(input);
        fpsCam = input.camera;

        interacting = null;

        _actionLock = false;

        playerId = input.playerIndex;

        int selfIgnoreLayer = LayerMask.NameToLayer("Player" + (playerId + 1) + "IgnoreRender");
        _interactLayerMask = ~(1 << selfIgnoreLayer);
    }

    // Update is called once per frame
    void Update()
    {
        CheckInteraction();

        // Disable interaction or return if the interact/return action has not been released yet.
        // This prevents the shared interact action from immediately leaving the interaction after
        // starting an interaction with something and vice versa
        if (_actionLock)
        {
            if (!_interactAction.WasPressedThisFrame() && !_returnAction.WasPressedThisFrame())
            {
                _actionLock = false;
            }
            else
            {
                return;
            }
        }

        if (IsInteracting())
        {
            // Check for return or interact action to leave the current interactable
            if (_returnAction.WasPressedThisFrame() || _interactAction.WasPressedThisFrame())
            {
                _actionLock = true;

                interacting?.Return(gameObject);
                interacting = null;
            }
        }
        else
        {
            // Check for interact action to interact with the current interactable
            if (_interactAction.WasPressedThisFrame() && currentItem != null && interacting == null)
            {
                if (currentItem.CanInteract())
                {
                    _actionLock = true;

                    interacting = currentItem;
                    currentItem.Interact(gameObject);
                    DisableCurrInteractable();
                }
                // Play reject sound if cannot interact
                // do here because terminal Interact() doesnt actally get called
                else
                {
                    RuntimeManager.PlayOneShot("event:/SFX/Interior/terminal_reject");
                }
            }
        }
    }

    void CheckInteraction()
    {
        if (interacting != null)
        {
            SetReturnText(interacting);
            return;
        }

        RaycastHit hit;

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawRay(ray.origin, ray.direction * reach, Color.red);

        if (Physics.Raycast(ray, out hit, reach, _interactLayerMask))
        {
            if (hit.collider.tag == "interactable")
            {
                Interactable newInteractable = hit.collider.GetComponent<Interactable>();

                // Null interact
                if (!newInteractable)
                {
                    return;
                }

                if (currentItem && newInteractable != currentItem)
                {
                    currentItem.DisableOutline();
                }

                if (newInteractable.enabled)
                {
                    SetNewCurrInteractable(newInteractable);
                }
                else
                {
                    DisableCurrInteractable();
                }
            }
            else
            {
                DisableCurrInteractable();
            }
        }
        else
        {
            DisableCurrInteractable();
        }
    }

    void SetReturnText(Interactable currItem)
    {
        // This interaction prompt may not be necessary and currently blocks the terminal UI. Instead, hide the interaction prompt when interacting
        if (GlobalPlayerUIManager.Instance != null)
        {
            GlobalPlayerUIManager.Instance.DisableInteractionText(playerId);
            // GlobalPlayerUIManager.Instance.EnableInteractionText(playerId, "To Return", currItem.msgColour, "UI/KeysPNG/PS4KEYS_BnW/Circle");
        }
    }

    /// <summary>
    /// Visually enable the interaction prompt for the Interactable object newInteractable that this player is looking at
    /// </summary>
    /// <param name="newInteractable"></param>
    void SetNewCurrInteractable(Interactable newInteractable)
    {
        currentItem = newInteractable;
        currentItem.EnableOutline();

        if (GlobalPlayerUIManager.Instance != null)
            GlobalPlayerUIManager.Instance.EnableInteractionText(playerId, currentItem.hoverMessage, currentItem.msgColour, ButtonIconManager.GamepadButton.ButtonSouth);
    }

    /// <summary>
    /// Visually disable the current interaction prompt for this player
    /// </summary>
    void DisableCurrInteractable()
    {
        if (GlobalPlayerUIManager.Instance != null)
            GlobalPlayerUIManager.Instance.DisableInteractionText(playerId);

        if (currentItem)
        {
            currentItem.DisableOutline();
            currentItem = null;
        }
    }

    /// <summary>
    /// Leave the current Interactable object this Player is interacting with (if any)
    /// (used for interactable objects that the player enters and stays in like terminals)
    /// </summary>
    public void LeaveCurrInteractable()
    {
        interacting?.Return(gameObject);
        interacting = null;
    }

    /// <summary>
    /// Return true iff this player is currently interacting with something
    /// </summary>
    public bool IsInteracting()
    {
        return interacting != null;
    }
}
