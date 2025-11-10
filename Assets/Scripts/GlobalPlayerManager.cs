using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// This script is intended to be on a persistent object handling player input across scenes.
/// This script also manages pre and post scene change code to prepare the player for the scene change.
/// </summary>
public class GlobalPlayerManager : MonoBehaviour
{
    public static GlobalPlayerManager Instance;

    private int _playerLimit;
    private PlayerData[] _players;
    private GlobalPlayerUIManager uiManager; // use to aggregate player UI
    // The UI handler for the character select screen
    [SerializeField] private GameObject characterSelectScreen;
    private ICharacterSelectScreen _characterSelectScreen;
    private PauseMenuUIHandler _pauseMenuUIHandler;

    // To replace by colors player pick - to ference for conflict or pass to PlayerData when all ready
    public Color[] playerColorSelector =
    {
        Color.clear,      // Player 1
        Color.clear,     // Player 2
        Color.clear,   // Player 3
    };

    public void Awake()
    {
        // Only allow one Global Player Manager
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this);
    }

    void Start()
    {
        _characterSelectScreen = characterSelectScreen.GetComponent<ICharacterSelectScreen>();
        _pauseMenuUIHandler = FindAnyObjectByType<PauseMenuUIHandler>();

        // initalize player data
        _playerLimit = PlayerInputManager.instance.maxPlayerCount;
        _players = new PlayerData[_playerLimit];
        for (int i = 0; i < _playerLimit; i++)
        {
            _players[i].Index = i;
        }

        // Register handlers for when a player joins or leaves
        PlayerInputManager.instance.onPlayerJoined += Instance.OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft += Instance.OnPlayerLeft;

        // Register handler for when the scene changes
        SceneManager.activeSceneChanged += Instance.ActiveSceneChanged;
    }

    /// <summary>
    /// Handler method for when a player joins.
    /// </summary>
    /// <param name="playerInput"></param>
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (SceneConstants.IsCharacterSelectScene())
        {
            var idx = playerInput.playerIndex;
            Debug.Log("Player " + idx + " Joined - Character Select Scene");
            _players[idx].Input = playerInput;
            _players[idx].PlayerObject = playerInput.gameObject; // This might change so it's a separate field.
            _players[idx].PlayerGraphic = playerInput.gameObject.GetComponent<PlayerSetup>().playerGraphic;
            _players[idx].Player = _players[idx].PlayerObject.GetComponent<Player>();
            _players[idx].Player.SetPlayerID(playerInput.playerIndex);
            _players[idx].Valid = true;

            // Add player to the character selection screen so they can start selecting their character.
            _characterSelectScreen.AddPlayer(idx);

            // register callbacks for the character select screen color change actions
            _players[idx].LeftActionDelegate = ctx => _characterSelectScreen.ChangeColor(idx, -1);
            _players[idx].RightActionDelegate = ctx => _characterSelectScreen.ChangeColor(idx, +1);

            // register callback for when the player navigates in order to set the current border color in the pause menu
            playerInput.actions.FindAction("Navigate").performed += ctx =>
            {
                var playerColor = _players[idx].PlayerColor;
                _pauseMenuUIHandler.SetCurrentActivePlayerColor(playerColor);
            };

            // register callback for opening and closing pause menu
            _players[idx].PauseMenuDelegate = ctx =>
            {
                // Set all players in UI
                for (var i = 0; i < _players.Length; i++)
                {
                    if (_players[i].Valid)
                    {
                        _players[i].Input.SwitchCurrentActionMap("UI");
                        _players[i].Player.SetInPauseMenu();
                    }
                }

                _pauseMenuUIHandler.SetCurrentActivePlayerColor(_players[idx].PlayerColor);
                // Show and focus the pause menu.
                _pauseMenuUIHandler.ShowPauseMenu();
                _pauseMenuUIHandler.FocusPanel();
                
                Time.timeScale = 0;
            };

            // register callbacks for the character select screen actions.
            _players[idx].SubmitActionDelegate = ctx =>
            {
                if (AllPlayersReady())
                {
                    // All players are ready and someone pressed the submit action so we load level select
                    Debug.Log("All players ready - starting");

                    // Assign player colors from selector to player data
                    for (int i = 0; i < _playerLimit; i++)
                    {
                        if (_players[i].Valid)
                        {
                            _players[i].PlayerColor = playerColorSelector[i];
                            // outline here instead of Player.cs Start(),
                            // so that that script no need reference _players
                            var outline = _players[i].PlayerGraphic.GetComponent<Outline>();
                            if (outline != null)
                            {
                                outline.OutlineColor = _players[i].PlayerColor;
                            }

                            // Register settings UI callback and set default UI settings
                            _pauseMenuUIHandler.SetPlayerSettings(i, new PlayerSettingsUI()
                            {
                                // Downscale by 10
                                LookSensitivity = _players[i].Player.GetLookSensitivity() * 10.0f
                            });
                            _pauseMenuUIHandler.RegisterPlayerSettingsCallback(i, UpdatePlayerSettings);
                            _pauseMenuUIHandler.ShowPlayerSettings(i);

                            // assign pause menu open/close delegates
                            InputActionMapper.GetPlayerOpenPauseMenuAction(_players[i].Input).started += Players[i].PauseMenuDelegate;
                            InputActionMapper.GetUIClosePauseMenuAction(_players[i].Input).started += ctx =>
                            {
                                Time.timeScale = 1;
                                _pauseMenuUIHandler.ClosePauseMenu();
                            };

                            // Inform pause menu of player colors
                            _pauseMenuUIHandler.SetPlayerColor(i, _players[i].PlayerColor);
                        }
                        else
                        {
                            // Invalid so hide player settings in menu
                            _pauseMenuUIHandler.HidePlayerSettings(i);
                        }
                    }

                    // pass these players to UI manager
                    GlobalPlayerUIManager.Instance.PassPlayers(_players);

                    // minimap initialize player dots *removed*

                    // Load level select screen
                    GlobalLevelManager.Instance.LoadLevelSelectScreen();
                }
                else
                {
                    // If player already ready, ignore
                    if (_players[idx].Ready) return;

                    // If current color taken, do not allow ready
                    // else assign color and ready up
                    var currentColor = playerColorSelector[idx];
                    for (int i = 0; i < _playerLimit; i++)
                    {
                        if (i != idx && _players[i].Valid && _players[i].Ready && _players[i].PlayerColor == currentColor)
                        {
                            Debug.Log("Player " + idx + " attempted to ready with color taken by Player " + i);
                            _characterSelectScreen.ShowColorConflictWarning(idx, i);
                            return;
                        }
                    }
                    Debug.Log("Player " + idx + " ready");
                    // hide any previous warning, need do before ReadyPlayer, that uses warning area to show ready text
                    _characterSelectScreen.HideColorConflictWarning(idx);
                    _characterSelectScreen.ReadyPlayer(idx);
                    _players[idx].Ready = true;
                    _players[idx].PlayerColor = currentColor;
                }
                Debug.Log("submit action");
            };
            _players[idx].CancelActionDelegate = ctx =>
            {
                // Unready a player or remove them if they're already unready.
                if (_players[idx].Ready)
                {
                    Debug.Log("Player " + idx + " not ready");
                    _characterSelectScreen.UnreadyPlayer(idx);
                    _players[idx].Ready = false;
                    var previousColor = _players[idx].PlayerColor;
                    _players[idx].PlayerColor = Color.clear; // make player color free

                    // Hide any warnings for other players that were blocked by this color
                    for (int i = 0; i < _playerLimit; i++)
                    {
                        if (_players[i].Valid && playerColorSelector[i] == previousColor)
                        {
                            _characterSelectScreen.HideColorConflictWarning(i);
                        }
                    }
                }
                else
                {
                    Debug.Log("Player " + idx + " leaving");
                    _characterSelectScreen.RemovePlayer(idx);
                    Destroy(playerInput.gameObject);
                }
            };
            InputActionMapper.GetCharacterSelectSubmitAction(playerInput).started += _players[idx].SubmitActionDelegate;
            InputActionMapper.GetCharacterSelectCancelAction(playerInput).started += _players[idx].CancelActionDelegate;
            InputActionMapper.GetCharacterSelectLeftAction(playerInput).started += _players[idx].LeftActionDelegate;
            InputActionMapper.GetCharacterSelectRightAction(playerInput).started += _players[idx].RightActionDelegate;

            // Ensure player is on the character select screen action map and disable by default
            playerInput.SwitchCurrentActionMap(InputActionMapper.CharacterSelectActionMapName);
            _players[idx].Player.TurnOff();
        }
        else
        {
            Debug.LogWarning("Player attempted to join - Other Scene");
        }
    }

    /// <summary>
    /// Handler method for when a player leaves
    /// </summary>
    /// <param name="playerInput"></param>
    private void OnPlayerLeft(PlayerInput playerInput)
    {
        if (SceneConstants.IsCharacterSelectScene())
        {
            Debug.Log("Player " + playerInput.playerIndex + " Left - Character Select Scene");

            // Remove the registered callbacks
            InputActionMapper.GetCharacterSelectSubmitAction(playerInput).started -= _players[playerInput.playerIndex].SubmitActionDelegate;
            InputActionMapper.GetCharacterSelectCancelAction(playerInput).started -= _players[playerInput.playerIndex].CancelActionDelegate;
            InputActionMapper.GetCharacterSelectLeftAction(playerInput).started -= _players[playerInput.playerIndex].LeftActionDelegate;
            InputActionMapper.GetCharacterSelectRightAction(playerInput).started -= _players[playerInput.playerIndex].RightActionDelegate;
        }
        else
        {
            Debug.Log("Player Left - Other Scene");
        }
    }

    /// <summary>
    /// Prepare all players for a scene change: <br />
    /// - Kick players off terminal if they are currently interacting with one
    /// - Leave Pause menu and player UI state if in one (changes are not saved)
    /// </summary>
    public void PrepareAllPlayersForSceneChange()
    {
        foreach (var player in _players)
        {
            if (player.Valid)
            {
                player.PlayerObject.GetComponent<PlayerInteract>().LeaveCurrInteractable();
                // Leave UI state when changing scene.
                player.Player.SetNotInPauseMenu();
            }
        }

        // Close Pause Menu UI if in it
        _pauseMenuUIHandler.HidePauseMenu();
        
        // Reset timescale to 1 if we are paused
        Time.timeScale = 1;
    }

    /// <summary>
    /// Handler for managing players when the scene changes
    /// </summary>
    /// <param name="oldScene"></param>
    /// <param name="newScene"></param>
    private void ActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        foreach (var player in _players)
        {
            if (player.Valid)
            {
                // Find player's spawn anchor for this scene
                // TODO handle this in the level manager? At least make it more efficient.
                var spawnAnchor = GameObject.Find("Player" + (player.Index + 1) + "Spawn");

                // Teleport player to their spawn anchor for this new scene
                var charController = player.PlayerObject.GetComponent<CharacterController>();
                var prevState = charController.enabled;
                charController.enabled = false;
                Debug.Log("Attempting scene change player " + player.Index + " teleport to anchor for new scene " + newScene.name);
                player.PlayerObject.transform.position = spawnAnchor.transform.position;
                charController.enabled = prevState;

                // Switch action map to player action map if not character selection screen
                if (SceneConstants.IsCharacterSelectScene())
                {
                    player.Input.SwitchCurrentActionMap(InputActionMapper.CharacterSelectActionMapName);
                    // Disable the player control
                    player.Player.TurnOff();
                    Cursor.lockState = CursorLockMode.None;
                }
                else if (SceneConstants.IsLevelSelectScene())
                {
                    player.Input.SwitchCurrentActionMap(InputActionMapper.LevelSelectActionMapName);
                    // Disable the player control
                    player.Player.TurnOff();
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    player.Input.SwitchCurrentActionMap(InputActionMapper.PlayerActionMapName);
                    // enable player if not the character select scene or the level select scene
                    player.Player.TurnOn();
                    Cursor.lockState = CursorLockMode.Locked;
                }

            }
        }

        // disable joining if not in the character select scene
        if (!SceneConstants.IsCharacterSelectScene())
        {
            PlayerInputManager.instance.DisableJoining();
        }
    }

    /// <returns>True iff all valid players are ready and at least one player is valid</returns>
    private bool AllPlayersReady()
    {
        return _players.All(player => !player.Valid || player.Ready) && _players.Any(player => player.Valid);
    }

    // declare here so other scripts can readonly it
    public PlayerData[] Players => _players;

    /// <summary>
    /// Callback for the settings UI (from the pause menu) to update a particular player's settings and return to game
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="playerSettings">The struct of new settings for this player</param>
    public void UpdatePlayerSettings(int playerIndex, PlayerSettingsUI playerSettings)
    {
        _players[playerIndex].Player.SetLookSensitivity(playerSettings.LookSensitivity / 10.0f);
        _players[playerIndex].Input.SwitchCurrentActionMap("Player");
        _players[playerIndex].Player.SetNotInPauseMenu();
        _pauseMenuUIHandler.HidePauseMenu();
    }
}

public struct PlayerData
{
    // True if the player is a valid playerdata object with an active player input
    public bool Valid { get; set; }
    // True if the player is ready to start the game (used in the character select screen)
    public bool Ready { get; set; }
    public int Index { get; set; }
    public PlayerInput Input { get; set; }
    public Player Player { get; set; }
    public GameObject PlayerObject { get; set; }
    public GameObject PlayerGraphic { get; set; }
    public Action<InputAction.CallbackContext> SubmitActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> CancelActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> LeftActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> RightActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> PauseMenuDelegate { get; set; }
    public Color PlayerColor { get; set; }
}

public interface ICharacterSelectScreen
{
    /// <summary>
    /// Add a player to the character selection screen to allow them to select their character.
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayer(int playerIndex);

    /// <summary>
    /// Remove a player by index from the character selection screen.
    /// </summary>
    /// <param name="playerIndex"></param>
    public void RemovePlayer(int playerIndex);

    /// <summary>
    /// A player has readied up and has confirmed their selection.
    /// </summary>
    /// <param name="playerIndex">The index of the player who readied up</param>
    public void ReadyPlayer(int playerIndex);

    /// <summary>
    /// A player has unreadied and can interact with the character selection again.
    /// </summary>
    /// <param name="playerIndex"></param>
    public void UnreadyPlayer(int playerIndex);

    /// <summary>
    /// Change the color selection for a player.
    /// </summary>
    /// <param name="playerIndex">The index of the player changing their color</param
    public void ChangeColor(int playerIndex, int direction);

    public void ShowColorConflictWarning(int playerIndex, int otherIndex);

    public void HideColorConflictWarning(int playerIndex);
}