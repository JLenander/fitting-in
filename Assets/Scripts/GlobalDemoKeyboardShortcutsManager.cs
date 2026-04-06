using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalDemoKeyboardShortcutsManager : MonoBehaviour
{
    public static GlobalDemoKeyboardShortcutsManager Instance;
    private Keyboard _keyboard;

    private const float ShortcutMinInterval = 4f;

    private float _mainMenuDelayTimer = ShortcutMinInterval;
    private float _levelSelectDelayTimer = ShortcutMinInterval;
    
    private PauseMenuUIHandler pauseMenuUIHandler;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(this);
        
        _mainMenuDelayTimer = 0f;
        
        _keyboard = InputSystem.GetDevice<Keyboard>();
        if (_keyboard == null)
        {
            Debug.LogError("Keyboard not found");
        }

        FindPauseMenuUIHandler();
    }

    private void Update()
    {
        if (pauseMenuUIHandler == null)
        {
            FindPauseMenuUIHandler();
        }
        else
        {
            BackToMainMenuCheck();
            BackToLevelSelectCheck();
        }
    }

    private void FindPauseMenuUIHandler()
    {
        pauseMenuUIHandler = FindAnyObjectByType<PauseMenuUIHandler>();
    }

    private void BackToMainMenuCheck()
    {
        if (_mainMenuDelayTimer > 0f)
        {
            _mainMenuDelayTimer -= Time.deltaTime;
            return;
        }
        
        bool shortcutPressed = _keyboard.mKey.isPressed && _keyboard.aKey.isPressed && _keyboard.iKey.isPressed;

        if (shortcutPressed)
        {
            _mainMenuDelayTimer = ShortcutMinInterval;
            pauseMenuUIHandler.ReturnToMainMenuButtonHandler();
            Debug.Log("Returning to main menu by keyboard shortcut");
        }
    }

    private void BackToLevelSelectCheck()
    {
        if (_levelSelectDelayTimer > 0f)
        {
            _levelSelectDelayTimer -= Time.deltaTime;
            return;
        }
        
        bool shortcutPressed = _keyboard.bKey.isPressed && _keyboard.lKey.isPressed && _keyboard.sKey.isPressed;

        if (shortcutPressed)
        {
            _levelSelectDelayTimer = ShortcutMinInterval;
            pauseMenuUIHandler.ReturnToLevelButtonHandler();
            Debug.Log("Returning to level select by keyboard shortcut");
        }
    }
}
