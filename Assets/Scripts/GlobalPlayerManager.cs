using FMODUnity;
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

    // Minimum number of players to proceed through character select
    public const int MinPlayers = 2;

    private int _playerLimit;
    private PlayerData[] _players;
    private GlobalPlayerUIManager _uiManager; // use to aggregate player UI
    // The UI handler for the character select screen
    [SerializeField] private GameObject characterSelectScreen;
    private ICharacterSelectScreen _characterSelectScreen;
    private PauseMenuUIHandler _pauseMenuUIHandler;

    // Delegate for actions related to closing the pause menu. (Declared here as this class is responsible for setting timescale)
    private Action _closePauseMenuDelegate;

    // To replace by colors player pick - to reference for conflict or pass to PlayerData when all ready
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
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this);
    }

    private void DeregisterPlayerCallbacks(PlayerInput playerInput, int playerIdx)
    {
        InputActionMapper.GetPlayerOpenPauseMenuAction(playerInput).started -= Players[playerIdx].PauseMenuDelegate;
        InputActionMapper.GetUIClosePauseMenuAction(playerInput).started -= Players[playerIdx].UIClosePauseMenuDelegate;
        InputActionMapper.GetCharacterSelectSubmitAction(playerInput).started -= _players[playerIdx].SubmitActionDelegate;
        InputActionMapper.GetCharacterSelectCancelAction(playerInput).started -= _players[playerIdx].CancelActionDelegate;
        InputActionMapper.GetCharacterSelectLeftAction(playerInput).started -= _players[playerIdx].LeftActionDelegate;
        InputActionMapper.GetCharacterSelectRightAction(playerInput).started -= _players[playerIdx].RightActionDelegate;
        InputActionMapper.GetUINavigateAction(playerInput).performed -= _players[playerIdx].NavigateColorActionDelegate;

        Debug.Log("Callbacks deregistered for player " + playerIdx);
    }

    /// <summary>Destroy this singleton leaving no instance left (and destroy players)</summary>
    private void DestroySingleton()
    {
        // Destroy all players
        foreach (var player in _players)
        {
            if (!player.Valid) continue;

            // Remove the registered callbacks if registered
            var idx = player.Index;
            if (Players[idx].InputActionDelegatesRegistered)
            {
                var playerInput = Players[idx].Input;
                DeregisterPlayerCallbacks(playerInput, idx);
            }

            Destroy(player.PlayerObject);
        }

        // Deregister callbacks
        // WARNING this MUST happen after destroying / making the players leave in order to successfully deregister each player's callbacks
        SceneManager.activeSceneChanged -= Instance.ActiveSceneChanged;
        PlayerInputManager.instance.onPlayerJoined -= Instance.OnPlayerJoined;
        PlayerInputManager.instance.onPlayerLeft -= Instance.OnPlayerLeft;
        _pauseMenuUIHandler.DeregisterClosePauseMenuHandler(_closePauseMenuDelegate);
        Debug.Log("Non player specific globalplayermanager callbacks deregistered");
        Instance = null;

        Destroy(gameObject);
    }

    void Start()
    {
        _characterSelectScreen = characterSelectScreen.GetComponent<ICharacterSelectScreen>();
        _pauseMenuUIHandler = FindAnyObjectByType<PauseMenuUIHandler>();

        // declare callback for closing pause menu (unpausing game time)
        _closePauseMenuDelegate = () =>
        {
            // Resume game on pause menu open
            Time.timeScale = 1;
            _pauseMenuUIHandler.HidePauseMenu();
            //stop lowpass audio
            RuntimeManager.StudioSystem.setParameterByName("pauseLPF", 0f);
        };
        // assign pause menu close delegate for the return to game button in the pause menu.
        _pauseMenuUIHandler.RegisterClosePauseMenuHandler(_closePauseMenuDelegate);

        // initialize player data
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
    /// This method adds a bunch of event subscriptions that depend on the player index as a closure (so they're declared here)
    /// </summary>
    /// <param name="playerInput"></param>
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        if (SceneConstants.IsCharacterSelectScene())
        {
            // Ignore join from non-Player
            if (playerInput.gameObject.GetComponent<Player>() == null)
            {
                return;
            }

            var idx = playerInput.playerIndex;
            Debug.Log("Player " + idx + " Joined - Character Select Scene");
            _players[idx].Input = playerInput;
            _players[idx].PlayerObject = playerInput.gameObject; // This might change so it's a separate field.
            _players[idx].PlayerGraphic = playerInput.gameObject.GetComponent<PlayerSetup>().playerGraphic;
            _players[idx].Player = _players[idx].PlayerObject.GetComponent<Player>();
            _players[idx].Player.SetPlayerID(playerInput.playerIndex);
            _players[idx].Valid = true;

            // Add player to the character selection screen so they can start selecting their character.
            _characterSelectScreen.AddPlayer(idx, playerInput);

            // register callbacks for the character select screen color change actions
            _players[idx].LeftActionDelegate = ctx => _characterSelectScreen.ChangeColor(idx, -1);
            _players[idx].RightActionDelegate = ctx => _characterSelectScreen.ChangeColor(idx, +1);

            // register callback for when the player navigates in order to set the current border color in the pause menu
            _players[idx].NavigateColorActionDelegate = ctx =>
            {
                var playerColor = _players[idx].PlayerColor;
                _pauseMenuUIHandler.SetCurrentActivePlayerColor(playerColor);
            };

            // register callback for opening the pause menu (pausing game time)
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
                //lowpass audio
                RuntimeManager.StudioSystem.setParameterByName("pauseLPF", 1f);
                _pauseMenuUIHandler.SetCurrentActivePlayerColor(_players[idx].PlayerColor);
                // Show and focus the pause menu.
                _pauseMenuUIHandler.ShowPauseMenu();
                _pauseMenuUIHandler.FocusPanel();

                // Pause game on pause menu open
                Time.timeScale = 0;
            };

            // register callbacks for the character select screen actions.
            _players[idx].SubmitActionDelegate = ctx =>
            {
                if (AllPlayersReady())
                {
                    // Verify at least 2 players have joined the game before starting
                    if (NumPlayersJoined() < MinPlayers)
                    {
                        Debug.Log("Tried to start with too few players");
                        // TODO: show warning to players
                        return;
                    }

                    // All players are ready and someone pressed the submit action so we load level select
                    Debug.Log("All players ready - starting");

                    SetupAndStartGame();
                }
                else
                {
                    MarkPlayerReady(idx);
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
                    DeregisterPlayerCallbacks(playerInput, idx);
                    Destroy(playerInput.gameObject);
                }
            };

            // assign pause menu open/close input action delegates
            Players[idx].UIClosePauseMenuDelegate = ctx => { _pauseMenuUIHandler.ClosePauseMenu(); };

            InputActionMapper.GetPlayerOpenPauseMenuAction(playerInput).started += Players[idx].PauseMenuDelegate;
            InputActionMapper.GetUIClosePauseMenuAction(playerInput).started += Players[idx].UIClosePauseMenuDelegate;
            InputActionMapper.GetCharacterSelectSubmitAction(playerInput).started += _players[idx].SubmitActionDelegate;
            InputActionMapper.GetCharacterSelectCancelAction(playerInput).started += _players[idx].CancelActionDelegate;
            InputActionMapper.GetCharacterSelectLeftAction(playerInput).started += _players[idx].LeftActionDelegate;
            InputActionMapper.GetCharacterSelectRightAction(playerInput).started += _players[idx].RightActionDelegate;
            InputActionMapper.GetUINavigateAction(playerInput).performed += _players[idx].NavigateColorActionDelegate;

            // mark delegates registered
            Players[idx].InputActionDelegatesRegistered = true;

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
        var idx = playerInput.playerIndex;
        if (SceneConstants.IsCharacterSelectScene())
        {
            Debug.Log("Player " + idx + " Left - Character Select Scene");
        }
        else
        {
            Debug.Log("Player Left - Other Scene");
        }
    }

    /// <summary>
    /// Setup player data and start the game
    /// </summary>
    private void SetupAndStartGame()
    {
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

                // Inform pause menu of player colors
                _pauseMenuUIHandler.SetPlayerColor(i, _players[i].PlayerColor);
            }
            else
            {
                // Invalid so hide player settings in menu
                _pauseMenuUIHandler.HidePlayerSettings(i);
            }
        }

        _characterSelectScreen.DestroyPlorps();

        // pass these players to UI manager
        GlobalPlayerUIManager.Instance.PassPlayers(_players);

        // minimap initialize player dots *removed*

        // Load level select screen
        GlobalLevelManager.Instance.LoadLevelSelectScreen();
    }

    private void MarkPlayerReady(int playerIdx)
    {
        // If player already ready, ignore
        if (_players[playerIdx].Ready) return;

        // If current color taken, do not allow ready
        // else assign color and ready up
        var currentColor = playerColorSelector[playerIdx];
        for (int i = 0; i < _playerLimit; i++)
        {
            if (i != playerIdx && _players[i].Valid && _players[i].Ready && _players[i].PlayerColor == currentColor)
            {
                Debug.Log("Player " + playerIdx + " attempted to ready with color taken by Player " + i);
                _characterSelectScreen.ShowColorConflictWarning(playerIdx, i);
                return;
            }
        }
        Debug.Log("Player " + playerIdx + " ready");
        // hide any previous warning, need do before ReadyPlayer, that uses warning area to show ready text
        _characterSelectScreen.HideColorConflictWarning(playerIdx);
        _characterSelectScreen.ReadyPlayer(playerIdx);
        _players[playerIdx].Ready = true;
        _players[playerIdx].PlayerColor = currentColor;
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

        // Undo the pause menu audio effect in case we were paused.
        RuntimeManager.StudioSystem.setParameterByName("pauseLPF", 0f);
    }

    /// <summary>
    /// Handler for managing players when the scene changes
    /// </summary>
    /// <param name="oldScene"></param>
    /// <param name="newScene"></param>
    private void ActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // Kill this object if we go back to the main menu (done this way to enable return to main menu easily)
        // This object could be refactored to work in the main scene (and keep player state)
        // and not need this but this is faster and guaranteed to work.
        if (SceneConstants.IsMainMenuScene())
        {
            DestroySingleton();
            return; // Destroy is async, don't continue to logic below
        }

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
                player.PlayerObject.GetComponent<Player>().ResetLook();
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

    /// <returns>Returns the number of players who have joined</returns>
    private int NumPlayersJoined()
    {
        return _players.Count(player => player.Valid);
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
    public Color PlayerColor { get; set; }

    // Delegates for this player registered here. (In general these should have at least 3 usages:
    // being 1. declared, 2. registered, and most importantly 3. deregistered when the player is destroyed by this object)
    public bool InputActionDelegatesRegistered { get; set; }
    public Action<InputAction.CallbackContext> SubmitActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> CancelActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> LeftActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> RightActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> PauseMenuDelegate { get; set; }
    public Action<InputAction.CallbackContext> NavigateColorActionDelegate { get; set; }
    public Action<InputAction.CallbackContext> UIClosePauseMenuDelegate { get; set; }

}

public interface ICharacterSelectScreen
{
    /// <summary>
    /// Add a player to the character selection screen to allow them to select their character.
    /// </summary>
    /// <param name="playerIndex"></param>
    public void AddPlayer(int playerIndex, PlayerInput playerInput);

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
    /// <param name="playerIndex">The index of the player changing their color</param>
    public void ChangeColor(int playerIndex, int direction);

    public void ShowColorConflictWarning(int playerIndex, int otherIndex);

    public void HideColorConflictWarning(int playerIndex);

    public void DestroyPlorps();
}
