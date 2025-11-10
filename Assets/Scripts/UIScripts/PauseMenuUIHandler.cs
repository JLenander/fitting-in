using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    
    // Player Sensitivity Settings and related inputs
    private const float IncreasedStepAmount = 2.0f;
    private const float StepIntervalIncreaseDelay = 1.0f;
    // (Inputs are necessary for custom smooth slider logic)
    private Slider[] _playerLookSensitivities = new Slider[NumPlayers];
    private List<InputAction> _playerNavActions;
    // Whether current focus is on a slider
    private bool focusOnSlider;
    private bool isHoldingSlider;
    // Time since we started holding
    private double _timeSinceFirstSliderInput;
    
    // The list of elements that update the current active player (player inputting to UI)
    private List<VisualElement> customButtonElements;
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

        // Intialize to negative value to prevent false positive on quick startup
        _timeSinceFirstSliderInput = -10.0f;

        // Setup player colored border callbacks to update the border color on focus or slider change.
        // This happens on every button and slider
        _playerColorBorderElement = root.Query<VisualElement>("PauseMenuPlayerColorOverlay").First();
        customButtonElements = root.Query(className: "custom-image-button").ToList();
        SetupCustomSliders();
        SetupCustomButtons();

        SceneManager.activeSceneChanged += PauseSceneChangeHandler;
        
        HidePauseMenu();
    }

    /// <summary>
    /// Setup the custom sliders to have smoother movement by increasing the step interval after a few seconds.
    /// Code in Update() checks for if the slider inputs are neutral which resets the timeSinceSliderLastHeld
    /// </summary>
    private void SetupCustomSliders()
    {
        for (int i = 0; i < _playerLookSensitivities.Length; i++)
        {
            var slider = _playerLookSensitivities[i];
            // Handler for when the value is changed, increasing the change if input has been held enough.
            slider.RegisterValueChangedCallback((evt) =>
            {
                // Set active player color
                _playerColorBorderElement.style.unityBackgroundImageTintColor = _currentActivePlayerColor;
                
                // Handle custom slider step interval logic
                var currTime = Time.unscaledTimeAsDouble;
                if (!isHoldingSlider)
                {
                    isHoldingSlider = true;
                    _timeSinceFirstSliderInput = currTime;
                }
                // Take the difference in realtime to see if we've held the button long enough to trigger the increased step
                if (currTime - _timeSinceFirstSliderInput > StepIntervalIncreaseDelay)
                {
                    var direction = (evt.newValue > evt.previousValue) ? 1.0f : -1.0f;
                    slider.SetValueWithoutNotify(slider.value + (direction * IncreasedStepAmount));
                }
                
            });
            
            slider.RegisterCallback<FocusInEvent>(evt =>
            {
                // Optimization to prevent some checks on update when not on a slider.
                focusOnSlider = true; 
                
                // Set active player color
                _playerColorBorderElement.style.unityBackgroundImageTintColor = _currentActivePlayerColor;
            });
        }
    }
    
    /// <summary>
    /// Scene change handler for Pause Menu UI handler.
    /// Initializes the player navigation actions if not done so after loading a level.
    /// </summary>
    private void PauseSceneChangeHandler(Scene oldScene, Scene newScene)
    {
        if (!SceneConstants.IsCharacterSelectScene() && !SceneConstants.IsLevelSelectScene())
        {
            InitalizePlayerNavActions();
        }
    }
    
    /// <summary>
    /// Initialize the player navigation actions if not already done so.
    /// </summary>
    private void InitalizePlayerNavActions()
    {
        if (_playerNavActions == null)
        {
            _playerNavActions = new List<InputAction>();
            foreach (var playerData in GlobalPlayerManager.Instance.Players)
            {
                if (playerData.Valid)
                {
                    var playerInput = playerData.Input;
                    var prevActionMap = playerInput.currentActionMap.name;
                    playerInput.SwitchCurrentActionMap(InputActionMapper.UIActionMapName);
                    _playerNavActions.Add(InputActionMapper.GetUINavigateAction(playerInput));
                    playerInput.SwitchCurrentActionMap(prevActionMap);
                }
            }
        }
    }

    /// <summary>
    /// Setup custom image buttons with an image border to color the pause menu border on focus.
    /// Coloring of the button border to indicate highlight is done in USS.
    /// </summary>
    private void SetupCustomButtons()
    {
        foreach (var element in customButtonElements)
        {
            element.RegisterCallback<FocusInEvent>(ctx =>
            {
                // Optimization to prevent some checks on update when not on a slider.
                focusOnSlider = false;
                
                // Set active player color
                _playerColorBorderElement.style.unityBackgroundImageTintColor = _currentActivePlayerColor;
            });
        }
    }

    public void Update()
    {
        // Perform a check when on a slider for neutral input in order to enable the sliding interval ("Page Size" in settings)
        // to increase after holding the input for a moment.
        if (root.style.display == DisplayStyle.Flex && focusOnSlider && _playerNavActions != null)
        {
            var allSliderInputsNeutral = true;
            foreach (var navAction in _playerNavActions)
            {
                var navValue = navAction.ReadValue<Vector2>();
                if (navValue.x != 0.0f)
                {
                    allSliderInputsNeutral = false;
                }
            }

            if (allSliderInputsNeutral)
            {
                isHoldingSlider = false;
            }
        }
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
        
        _playerLookSensitivities[playerIndex].SetValueWithoutNotify(settings.LookSensitivity);
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