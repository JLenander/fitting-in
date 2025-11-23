using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class ButtonIconManager : MonoBehaviour
{
    public static ButtonIconManager Instance;

    [SerializeField] private Texture2D missingIconTexture;
    private Dictionary<GamepadButton, GamepadButtonSprites> _iconCache = new();
    
    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        
        DontDestroyOnLoad(this);
        
        LoadGamepadButtonTextures();
    }

    private void LoadGamepadButtonTextures()
    {
        _iconCache.Clear();
        _iconCache.Add(GamepadButton.ButtonNorth, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_ButtonNorth"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_ButtonNorth")
        ));
        _iconCache.Add(GamepadButton.ButtonSouth, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_ButtonSouth"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_ButtonSouth")
        ));
        _iconCache.Add(GamepadButton.ButtonEast, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_ButtonEast"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_ButtonEast")
        ));
        _iconCache.Add(GamepadButton.ButtonWest, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_ButtonWest"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_ButtonWest")
        ));
        _iconCache.Add(GamepadButton.LeftShoulder, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_LShoulder"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_LShoulder")
        ));
        _iconCache.Add(GamepadButton.RightShoulder, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_RShoulder"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_RShoulder")
        ));
        _iconCache.Add(GamepadButton.LeftTrigger, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_LTrigger"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_LTrigger")
        ));
        _iconCache.Add(GamepadButton.RightTrigger, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_RTrigger"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_RTrigger")
        ));
        _iconCache.Add(GamepadButton.Start, new GamepadButtonSprites(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_Option"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_Option")
        ));
        
        // The following are shared between platforms
        Texture2D texture;
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Neutral");
        _iconCache.Add(GamepadButton.LeftStickNeutral, new GamepadButtonSprites(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Up");
        _iconCache.Add(GamepadButton.LeftStickUp, new GamepadButtonSprites(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Down");
        _iconCache.Add(GamepadButton.LeftStickDown, new GamepadButtonSprites(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Left");
        _iconCache.Add(GamepadButton.LeftStickLeft, new GamepadButtonSprites(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Right");
        _iconCache.Add(GamepadButton.LeftStickRight, new GamepadButtonSprites(texture, texture));
        
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Neutral");
        _iconCache.Add(GamepadButton.RightStickNeutral, new GamepadButtonSprites(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Up");
        _iconCache.Add(GamepadButton.RightStickUp, new GamepadButtonSprites(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Down");
        _iconCache.Add(GamepadButton.RightStickDown, new GamepadButtonSprites(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Left");
        _iconCache.Add(GamepadButton.RightStickLeft, new GamepadButtonSprites(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Right");
        _iconCache.Add(GamepadButton.RightStickRight, new GamepadButtonSprites(texture, texture));
    }

    public Texture2D GetButtonIconForPlayer(int playerIndex, GamepadButton button)
    {
        if (playerIndex < 0 || playerIndex >= GlobalPlayerManager.Instance.Players.Length)
        {
            Debug.LogError("PlayerIndex out of range");
            return missingIconTexture;
        }
        
        var player = GlobalPlayerManager.Instance.Players[playerIndex];
        if (!player.Valid)
        {
            Debug.LogError("Player not valid");
            return missingIconTexture;
        }

        var gamepad = player.Input.GetDevice<Gamepad>();
        switch (gamepad)
        {
            case null:
                // default to playstation if gamepad can't be found (mouse and keyboard for development or other)
                Debug.Log("Defaulting to playstation. Can't find gamepad for player " + playerIndex);
                return GetButtonIcon(button, GamepadType.Playstation);
            // It's possible XInputController is not an xbox controller exactly as XInput is a protocol
            // This also may not work on a macos build
            case XInputController:
                return GetButtonIcon(button, GamepadType.Xbox);
            case DualShockGamepad:
                return GetButtonIcon(button, GamepadType.Playstation);
            default:
                // If we can't recognize this as a playstation or xbox controller then default to playstation
                Debug.LogWarning("Couldn't recognize gamepad format. Defaulting to playstation.");
                return GetButtonIcon(button, GamepadType.Playstation);
        }
    }

    /// <summary>
    /// Return a Texture2D for the requested button icon.
    /// </summary>
    /// <param name="button">The button on the gamepad to get the icon for</param>
    /// <param name="gamepadType">The platform to get the button icon from</param>
    /// <returns>Texture2D for the icon. This does not need to be released after use as it's cached</returns>
    public Texture2D GetButtonIcon(GamepadButton button, GamepadType gamepadType)
    {
        if (!_iconCache.TryGetValue(button, out var icons))
        {
            Debug.LogError("Gamepad button icon not found for button: " + button);
            return missingIconTexture;
        }

        switch (gamepadType)
        {
            case GamepadType.Playstation:
                return icons.PlaystationTexture;
            case GamepadType.Xbox:
                return icons.XboxTexture;
            default:
                Debug.LogError("Unhandled gamepadType");
                return missingIconTexture;
        }
    }
        
    public enum GamepadButton{
        /// <summary> PS - Triangle | XBOX - Y </summary>
        ButtonNorth,
        /// <summary> PS - X | XBOX - A </summary>
        ButtonSouth,
        /// <summary> PS - Circle | XBOX - B </summary>
        ButtonEast,
        /// <summary> PS - Square | XBOX - X </summary>
        ButtonWest,
        /// <summary> PS - L1 | XBOX - LB </summary>
        LeftShoulder,
        /// <summary> PS - L2 | XBOX - LT </summary>
        LeftTrigger,
        LeftStickNeutral,
        LeftStickUp,
        LeftStickDown,
        LeftStickLeft,
        LeftStickRight,
        /// <summary> PS - R1 | XBOX - RB </summary>
        RightShoulder,
        /// <summary> PS - R2 | XBOX - RT </summary>
        RightTrigger,
        RightStickNeutral,
        RightStickUp,
        RightStickDown,
        RightStickLeft,
        RightStickRight,
        // /// <summary> PS - share (left button) | XBOX - Select (left button) </summary>
        // Select,
        /// <summary> PS - options (right button) | XBOX - Start (right button) </summary>
        Start
    }

    public enum GamepadType
    {
        Playstation,
        Xbox
    }
    
    private record GamepadButtonSprites(Texture2D PlaystationTexture, Texture2D XboxTexture)
    {
        public Texture2D PlaystationTexture { get; set; } = PlaystationTexture;
        public Texture2D XboxTexture { get; set; } = XboxTexture;
    }
}
