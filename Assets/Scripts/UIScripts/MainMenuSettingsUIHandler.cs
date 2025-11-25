using System;
using FMODUnity;
using UIScripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MainMenuSettingsUIHandler : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;

    private VisualElement root;
    
    private Button _returnButton;
    private Slider _masterAudioSlider;
    private Toggle _fullscreenToggle;
    
    // Player Sensitivity Settings and related inputs
    private const float IncreasedStepAmount = 2.0f;
    private const float StepIntervalIncreaseDelay = 1.0f;
    // (Inputs are necessary for custom smooth slider logic)
    private InputAction _playerNavAction;
    // Whether current focus is on a slider
    private bool focusOnSlider;
    private bool isHoldingSlider;
    // Time since we started holding
    private double _timeSinceFirstSliderInput;
    
    private void Awake()
    {
        root = uiDoc.rootVisualElement;
        
        _returnButton = root.Query<Button>("ReturnButton").First();
        _masterAudioSlider = root.Query<Slider>("MasterAudioSlider").First();
        _fullscreenToggle = root.Query<Toggle>("FullscreenToggle").First();
        
        // Intialize to negative value to prevent false positive on quick startup
        _timeSinceFirstSliderInput = -10.0f;

        // Default Project Wide UI Nav action
        _playerNavAction = InputSystem.actions.FindAction("Navigate");

        SetupCustomSliders();
        SetupSliderOptimization();
        SetupUISFX();
        
        HideSettingsPanel();
    }

    private void Start()
    {
        // Setup fullscreen toggle
        _fullscreenToggle.RegisterValueChangedCallback(evt =>
        {
            GlobalGameSettingsManager.Instance.SetFullscreen(_fullscreenToggle.value);
        });
        
        // Setup audio slider logic
        _masterAudioSlider.RegisterValueChangedCallback(evt =>
        {
            // Master audio volume
            float masterVolume = Mathf.InverseLerp(0.0f, _masterAudioSlider.highValue, _masterAudioSlider.value);
            GlobalGameSettingsManager.Instance.SetMasterVolume(masterVolume);
        });
        
        // Setup return to main menu button
        _returnButton.clicked += () =>
        {
            HideSettingsPanel();
        };
        
        UpdateUIState();
    }
    
    /// <summary>
    /// Setup the custom sliders to have smoother movement by increasing the step interval after a few seconds.
    /// Code in Update() checks for if the slider inputs are neutral which resets the timeSinceSliderLastHeld
    /// </summary>
    private void SetupCustomSliders()
    {
        // Handler for when the value is changed, increasing the change if input has been held enough.
        _masterAudioSlider.RegisterValueChangedCallback((evt) =>
        {
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
                _masterAudioSlider.SetValueWithoutNotify(_masterAudioSlider.value + (direction * IncreasedStepAmount));
            }
        });
    }

    // Add the nav callbacks to make the custom slider logic optimization
    private void SetupSliderOptimization()
    {
        _masterAudioSlider.RegisterCallback<FocusInEvent>(evt =>
        {
            // Optimization to prevent some checks on update when not on a slider.
            focusOnSlider = true; 
        });
        _returnButton.RegisterCallback<FocusInEvent>(evt =>
        {
            focusOnSlider = false;
        });
        _fullscreenToggle.RegisterCallback<FocusInEvent>(evt =>
        {
            focusOnSlider = false;
        });
    }

    private void SetupUISFX()
    {
        _returnButton.clicked += () =>
        {
            RuntimeManager.PlayOneShot("event:/SFX/UI/choose");
        };
        _fullscreenToggle.RegisterValueChangedCallback(evt =>
        {
            RuntimeManager.PlayOneShot("event:/SFX/UI/choose");
        });
        
        _returnButton.RegisterCallback<FocusInEvent>(evt => 
        {
            if (evt.relatedTarget == null)
            {
                return;
            }
            RuntimeManager.PlayOneShot("event:/SFX/UI/move");
        });
        _masterAudioSlider.RegisterCallback<FocusInEvent>(evt => 
        {
            RuntimeManager.PlayOneShot("event:/SFX/UI/move");
        });
        _fullscreenToggle.RegisterCallback<FocusInEvent>(evt => 
        {
            RuntimeManager.PlayOneShot("event:/SFX/UI/move");
        });
    }
    
    public void Update()
    {
        // Perform a check when on a slider for neutral input in order to enable the sliding interval ("Page Size" in settings)
        // to increase after holding the input for a moment.
        if (root.style.display == DisplayStyle.Flex && focusOnSlider && _playerNavAction != null)
        {
            var allSliderInputsNeutral = true;

            var navValue = _playerNavAction.ReadValue<Vector2>();
            if (navValue.x != 0.0f)
            {
                allSliderInputsNeutral = false;
            }

            if (allSliderInputsNeutral)
            {
                isHoldingSlider = false;
            }
        }
    }

    public void RegisterReturnToMainMenuCallback(Action callback)
    {
        _returnButton.clicked += callback;
    }

    /// <summary>
    /// Update UI state to match current settings.
    /// </summary>
    private void UpdateUIState()
    {
        _fullscreenToggle.value = GlobalGameSettingsManager.Instance.GetFullscreen();
        _masterAudioSlider.value = Mathf.Lerp(0f, 100f, GlobalGameSettingsManager.Instance.GetMasterVolume());
    }

    /// <summary>
    /// Show and bring focus to the settings panel
    /// </summary>
    public void ShowSettingsPanel()
    {
        UpdateUIState();
        root.style.display = DisplayStyle.Flex;
        FocusPanel();
    }
    
    /// <summary>
    /// Hide the settings panel
    /// </summary>
    public void HideSettingsPanel()
    {
        root.style.display = DisplayStyle.None;
    }
    
    private void FocusPanel()
    {
        _returnButton.Focus();    
    }
}
