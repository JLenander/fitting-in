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
    private readonly Dictionary<GamepadButton, PlatformBasedTextures> _iconCache = new();
    private readonly Dictionary<ControlsImage, PlatformBasedTextures> _controlsImgCache = new();
    
    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        
        DontDestroyOnLoad(this);
        
        LoadGamepadButtonTextures();
        LoadControlsImages();
    }

    private void LoadGamepadButtonTextures()
    {
        _iconCache.Clear();
        _iconCache.Add(GamepadButton.ButtonNorth, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_ButtonNorth"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_ButtonNorth")
        ));
        _iconCache.Add(GamepadButton.ButtonSouth, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_ButtonSouth"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_ButtonSouth")
        ));
        _iconCache.Add(GamepadButton.ButtonEast, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_ButtonEast"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_ButtonEast")
        ));
        _iconCache.Add(GamepadButton.ButtonWest, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_ButtonWest"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_ButtonWest")
        ));
        _iconCache.Add(GamepadButton.LeftShoulder, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_LShoulder"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_LShoulder")
        ));
        _iconCache.Add(GamepadButton.RightShoulder, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_RShoulder"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_RShoulder")
        ));
        _iconCache.Add(GamepadButton.LeftTrigger, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_LTrigger"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_LTrigger")
        ));
        _iconCache.Add(GamepadButton.RightTrigger, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_RTrigger"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_RTrigger")
        ));
        _iconCache.Add(GamepadButton.Start, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/PS_Option"),
            Resources.Load<Texture2D>("UI/GamepadButtons/Buttons/XBOX_Option")
        ));
        
        // The following are shared between platforms
        Texture2D texture;
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Neutral");
        _iconCache.Add(GamepadButton.LeftStickNeutral, new PlatformBasedTextures(texture, texture));
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Up");
        _iconCache.Add(GamepadButton.LeftStickUp, new PlatformBasedTextures(texture, texture));
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Down");
        _iconCache.Add(GamepadButton.LeftStickDown, new PlatformBasedTextures(texture, texture));
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Left");
        _iconCache.Add(GamepadButton.LeftStickLeft, new PlatformBasedTextures(texture, texture));
        texture =  Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/L_Right");
        _iconCache.Add(GamepadButton.LeftStickRight, new PlatformBasedTextures(texture, texture));
        
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Neutral");
        _iconCache.Add(GamepadButton.RightStickNeutral, new PlatformBasedTextures(texture, texture));
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Up");
        _iconCache.Add(GamepadButton.RightStickUp, new PlatformBasedTextures(texture, texture));
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Down");
        _iconCache.Add(GamepadButton.RightStickDown, new PlatformBasedTextures(texture, texture));
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Left");
        _iconCache.Add(GamepadButton.RightStickLeft, new PlatformBasedTextures(texture, texture));
        texture = Resources.Load<Texture2D>("UI/GamepadButtons/Sticks/R_Right");
        _iconCache.Add(GamepadButton.RightStickRight, new PlatformBasedTextures(texture, texture));
    }

    private void LoadControlsImages()
    {
        _controlsImgCache.Add(ControlsImage.ArmTerminalControls, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/arm_controls_text"),
            Resources.Load<Texture2D>("UI/arm_controls_xbox")
        ));
        _controlsImgCache.Add(ControlsImage.LegTerminalControls, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/leg_controls_text"),
            Resources.Load<Texture2D>("UI/leg_controls_xbox")
        ));
        _controlsImgCache.Add(ControlsImage.EyeTerminalControls, new PlatformBasedTextures(
            Resources.Load<Texture2D>("UI/eye_controls_text"),
            Resources.Load<Texture2D>("UI/eye_controls_xbox")
        ));
    }

    /// <summary>
    /// Return the platform of the gamepad for the requested player.
    /// Returns GamepadType.Invalid if arguments are invalid (ex. index out of range)
    /// </summary>
    /// <param name="playerIndex">The index of the player matching the GlobalPlayerManager's players list</param>
    /// <returns></returns>
    public GamepadType GetGamepadTypeForPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= GlobalPlayerManager.Instance.Players.Length)
        {
            Debug.LogError("PlayerIndex out of range");
            return GamepadType.Invalid;
        }
        
        var player = GlobalPlayerManager.Instance.Players[playerIndex];
        if (!player.Valid)
        {
            Debug.LogError("Player not valid");
            return GamepadType.Invalid;
        }

        var gamepad = player.Input.GetDevice<Gamepad>();
        switch (gamepad)
        {
            case null:
                // default to playstation if gamepad can't be found (mouse and keyboard for development or other)
                Debug.Log("Defaulting to playstation. Can't find gamepad for player " + playerIndex);
                return GamepadType.Playstation;
            // It's possible XInputController is not an xbox controller exactly as XInput is a protocol
            // This also may not work on a macos build
            case XInputController:
                return GamepadType.Xbox;
            case DualShockGamepad:
                return GamepadType.Playstation;
            default:
                // If we can't recognize this as a playstation or xbox controller then default to playstation
                Debug.LogWarning("Couldn't recognize gamepad format. Defaulting to playstation.");
                return GamepadType.Playstation;
        }
    }

    /// <summary>
    /// Return the button icon for the requested player's gamepad platform
    /// </summary>
    /// <param name="playerIndex">The index of the player matching the GlobalGameManager's list of players</param>
    /// <param name="button"></param>
    /// <returns></returns>
    public Texture2D GetButtonIconForPlayer(int playerIndex, GamepadButton button)
    {
        var gamepadType = GetGamepadTypeForPlayer(playerIndex);
        return GetButtonIcon(button, gamepadType);
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
            case GamepadType.Invalid:
                return missingIconTexture;
            default:
                Debug.LogError("Unhandled gamepadType");
                return missingIconTexture;
        }
    }
    
    /// <summary>
    /// Return a Texture2D for the requested controls image for the requested player
    /// </summary>
    /// <param name="playerIndex">The index of the player matching the GlobalGameManager's list of players</param>
    /// <param name="image"></param>
    /// <returns></returns>
    public Texture2D GetControlsImageForPlayer(int playerIndex, ControlsImage image)
    {
        var gamepadType = GetGamepadTypeForPlayer(playerIndex);
        return GetControlsImage(image, gamepadType);
    }

    /// <summary>
    /// Return a Texture2D for the requested controls image for the requested gamepad platform
    /// </summary>
    /// <param name="image"></param>
    /// <param name="gamepadType"></param>
    /// <returns></returns>
    public Texture2D GetControlsImage(ControlsImage image, GamepadType gamepadType)
    {
        if (!_controlsImgCache.TryGetValue(image, out var icons))
        {
            Debug.LogError("Controls Image not found for request: " + image);
            return missingIconTexture;
        }

        switch (gamepadType)
        {
            case GamepadType.Playstation:
                return icons.PlaystationTexture;
            case GamepadType.Xbox:
                return icons.XboxTexture;
            case GamepadType.Invalid:
                return missingIconTexture;
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
        Xbox,
        Invalid
    }

    public enum ControlsImage
    {
        ArmTerminalControls,
        LegTerminalControls,
        EyeTerminalControls
    }
    
    private record PlatformBasedTextures(Texture2D PlaystationTexture, Texture2D XboxTexture)
    {
        public Texture2D PlaystationTexture { get; set; } = PlaystationTexture;
        public Texture2D XboxTexture { get; set; } = XboxTexture;
    }
}
