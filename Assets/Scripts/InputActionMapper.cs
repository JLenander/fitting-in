using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Static class to abstract away all the magic strings when retrieving input system InputActions.
/// These methods throw an error if used on an invalid mapping (this would only happen if you are retrieving an action that doesn't belong to the current input actions mapping).
/// </summary>
public static class InputActionMapper
{
    public static InputAction GetCharacterSelectSubmitAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("Submit", throwIfNotFound: true);
    }
    
    public static InputAction GetCharacterSelectCancelAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("Cancel", throwIfNotFound: true);
    }

    public static InputAction GetPlayerMoveAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("Move", throwIfNotFound: true);
    }

    public static InputAction GetPlayerLookAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("Look", throwIfNotFound: true);
    }

    public static InputAction GetPlayerInteractAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("Interact", throwIfNotFound: true);
    }

    public static InputAction GetPlayerReturnAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("Return", throwIfNotFound: true);
    }

    public static InputAction GetPlayerItemInteractAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("ItemInteract", throwIfNotFound: true);
    }

    public static InputAction GetPlayerLeftTriggerAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("LeftTrigger", throwIfNotFound: true);
    }

    public static InputAction GetPlayerRightTriggerAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("RightTrigger", throwIfNotFound: true);
    }

    public static InputAction GetPlayerLeftBumperAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("LeftBumper", throwIfNotFound: true);
    }

    public static InputAction GetPlayerRightBumperAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("RightBumper", throwIfNotFound: true);
    }
    
    public static InputAction GetPlayerOpenPauseMenuAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("OpenPauseMenu", throwIfNotFound: true);
    }
    
    public static InputAction GetUINavigateAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("Navigate", throwIfNotFound: true);
    }
    
    public static InputAction GetUIClosePauseMenuAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("ClosePauseMenu", throwIfNotFound: true);
    }
    
    public static InputAction GetCharacterSelectLeftAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("LeftBumper", throwIfNotFound: true);
    }
    
    public static InputAction GetCharacterSelectRightAction(PlayerInput playerInput)
    {
        return playerInput.actions.FindAction("RightBumper", throwIfNotFound: true);
    }
    
    public const string CharacterSelectActionMapName = "CharacterSelect";
    public const string LevelSelectActionMapName = "LevelSelect";
    public const string PlayerActionMapName = "Player";
    public const string UIActionMapName = "UI";
}
