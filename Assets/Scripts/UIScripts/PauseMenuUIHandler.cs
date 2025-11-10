using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuUIHandler : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;
    
    private VisualElement root;

    private Button _returnToGameButton;
    private Button _returnToLevelSelectButton;
    private Button _quitGameButton;
    
    private const int NumPlayers = 3;
    
    // Player colors
    private Color[] playerColors = new Color[NumPlayers];
    
    // Player Sensitivity Settings
    private Slider[] _playerLookSensitivities = new Slider[NumPlayers];

    // The list of elements that update the current active player (player inputting to UI)
    private List<VisualElement> borderElements;
    private VisualElement _playerColorBorderElement;

    // The player color of the most recent player who was inputting to the pause menu
    private StyleColor _currentActivePlayerColor;
    
    private void Awake()
    {
        root = uiDoc.rootVisualElement;
        
        SetupButtonClickables();
        
        _playerLookSensitivities[0] = root.Query<Slider>("Player1InputSensitivity").First();
        _playerLookSensitivities[1] = root.Query<Slider>("Player2InputSensitivity").First();
        _playerLookSensitivities[2] = root.Query<Slider>("Player3InputSensitivity").First();

        // Setup player colored border callbacks to update the border color on focus or slider change.
        _playerColorBorderElement = root.Query<VisualElement>("PauseMenuPlayerColorOverlay").First();
        // This happens on every button and slider
        borderElements = root.Query(className: "custom-image-button").ToList();
        foreach (var slider in _playerLookSensitivities)
        {
            borderElements.Add(slider);
        }
        foreach (var element in borderElements)
        {
            var slider = element as Slider;
            slider?.RegisterValueChangedCallback((evt) =>
            {
                // Set active player color
                _playerColorBorderElement.style.unityBackgroundImageTintColor = _currentActivePlayerColor;
            });
        
            element.RegisterCallback<FocusInEvent>(ctx =>
            {
                // Set active player color
                _playerColorBorderElement.style.unityBackgroundImageTintColor = _currentActivePlayerColor;
            });
        }
        
        HidePauseMenu();
    }

    private void ReturnToGameButtonHandler()
    {
        GlobalLevelManager.Instance.LoadLevelSelectScreen();
    }

    private void QuitGameButtonHandler()
    {
        Debug.Log("QuitButtonPressed");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit(0);
#endif
    }
    
    // Setup the clickables for our custom buttons
    private void SetupButtonClickables()
    {
        _returnToGameButton = root.Query<Button>("ReturnToGameButton").First();
        _returnToLevelSelectButton = root.Query<Button>("LevelSelectButton").First();
        _quitGameButton = root.Query<Button>("QuitGameButton").First();
        
        // Return to game handler is setup by each player later on (RegisterPlayerSettingsCallback) as it needs to pass specific data to each player
        
        // Setup return to level select button callback
        _returnToLevelSelectButton.clicked += ReturnToGameButtonHandler;
        
        // Setup quit game button callback
        _quitGameButton.clicked += QuitGameButtonHandler;
    }

    /// <summary>
    /// Set's the player's color so that those player's settings can be color coded for visual clarity
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="playerColor"></param>
    public void SetPlayerColor(int playerIndex, Color playerColor)
    {
        playerColors[playerIndex] = playerColor;
        UpdatePlayerColoredElements();
    }

    // Update static elements that should be player colored
    private void UpdatePlayerColoredElements()
    {
        for (var i = 0; i < NumPlayers; i++)
        {
            _playerLookSensitivities[i].labelElement.style.color = playerColors[i];
        }
    }

    public void SetPlayerSettings(int playerIndex, PlayerSettingsUI settings)
    {
        if (playerIndex > NumPlayers)
        {
            Debug.LogError("Player index out of range");
            return;
        } 
        
        _playerLookSensitivities[playerIndex].value = settings.LookSensitivity;
    }

    /// <summary>
    /// Register the callback for a particular player's updated settings. Intended to register one callback per player
    /// that runs when the pause menu is closed.
    /// </summary>
    /// <param name="playerIndex">The index of the player's callback</param>
    /// <param name="callback">A callback function taking the index of the player and the new settings for this player</param>
    public void RegisterPlayerSettingsCallback(int playerIndex, Action<int, PlayerSettingsUI> callback)
    {
        _returnToGameButton.clicked += () =>
        {
            var updatedSettings = new PlayerSettingsUI()
            {
                LookSensitivity = _playerLookSensitivities[playerIndex].value,
            };
            callback(playerIndex, updatedSettings);
        };
    }

    /// <summary>
    /// Close the pause menu and save all settings
    /// </summary>
    public void ClosePauseMenu()
    {
        // Janky hack to click the button to trigger proper callbacks
        using var e = new NavigationSubmitEvent();
        e.target = _returnToGameButton;
        _returnToGameButton.SendEvent(e);
    }

    // Enable settings panels for player <playerIndex>
    public void ShowPlayerSettings(int playerIndex)
    {
        if (playerIndex > NumPlayers)
        {
            Debug.LogError("Player index out of range");
            return;
        } 
        
        _playerLookSensitivities[playerIndex].style.display = DisplayStyle.Flex;
    }
    
    // Disable settings panels for player <playerIndex>
    public void HidePlayerSettings(int playerIndex)
    {
        if (playerIndex > NumPlayers)
        {
            Debug.LogError("Player index out of range");
            return;
        } 
        
        _playerLookSensitivities[playerIndex].style.display = DisplayStyle.None;
    }

    public void ShowPauseMenu()
    {
        root.style.display = DisplayStyle.Flex;
    }

    public void HidePauseMenu()
    {
        root.style.display = DisplayStyle.None;
    }
    
    // Attempt to put focus on this UI doc (specifically the close pause menu button). Necessary for gamepad focus
    public void FocusPanel()
    {
        _returnToGameButton.Focus();
    }

    /// <summary>
    /// Set the color of the current active player so that the pause menu can distinguish who is currently controlling the UI.
    /// </summary>
    /// <param name="color"></param>
    public void SetCurrentActivePlayerColor(Color color)
    {
        _currentActivePlayerColor = color;
    }
}

public struct PlayerSettingsUI
{
    // Camera look sensitivity as a plorp
    public float LookSensitivity;
}