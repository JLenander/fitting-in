using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// The globall level manager is in charge of the playable levels information, such as the name, the status as to whether
/// it's locked or has been completed, and what level it unlocks next if any.
///
/// Also handles the level select scene.
/// Once a level has been completed, mark it as such with the Complete method (do so before returning to the level select scene).
/// </summary>
public class GlobalLevelManager : MonoBehaviour
{
    public static GlobalLevelManager Instance { get; private set; }

    [SerializeField] private UIDocument loadingScreen;
    private VisualElement _loadingScreenRoot;
    private AsyncOperation asyncLevelLoad;
    
    private Dictionary<string, int> _sceneNameToLevelIndexMap;

    public void Awake()
    {
        // Only allow one level manager
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        } else 
        {
            Instance = this;
        }

        _loadingScreenRoot = loadingScreen.rootVisualElement;
        HideLoadingScreen();

        SceneManager.sceneLoaded += SceneLoadHandler;
        
        DontDestroyOnLoad(this);
    }
    
    public void Start()
    {
        _sceneNameToLevelIndexMap = new Dictionary<string, int>();
        for (var i = 0; i < GameConfig.Levels.Length; i++)
        {
            _sceneNameToLevelIndexMap[GameConfig.Levels[i].sceneName] = i;
        }
            
        SanityCheckSceneNames();
    }

    public void SceneLoadHandler(Scene oldScene, LoadSceneMode mode)
    {
        HideLoadingScreen();
    }

    public Level[] GetLevels()
    {
        return GameConfig.Levels;
    }

    /// <summary>
    /// Call to mark a level as complete, unlocking any levels it is guarding.
    /// </summary>
    /// <param name="sceneName">The name of the scene corresponding to the level to mark as complete</param>
    public void CompleteLevel(string sceneName)
    {
        if (!_sceneNameToLevelIndexMap.TryGetValue(sceneName, out var index))
        {
            Debug.LogWarning("Scene " + sceneName + " completed but not in levels array");
            return;
        }

        // Complete this level
        GameConfig.Levels[index].status = LevelStatus.Completed;
        // Unlock next levels if such a relationship exists
        foreach (var unlockedScene in GameConfig.Levels[index].unlocksScenes)
        {
            if (_sceneNameToLevelIndexMap.TryGetValue(unlockedScene, out var unlockedIndex))
            {
                GameConfig.Levels[unlockedIndex].status = LevelStatus.Unlocked;
                Debug.Log("Level " + GameConfig.Levels[index].sceneName + " completion unlocks " + GameConfig.Levels[unlockedIndex].sceneName);
            }
        }
    }

    /// <summary>
    /// Load the Level Select screen, running all pre-scene loading handlers and handling the loading screen
    /// </summary>
    public void LoadLevelSelectScreen()
    {
        LoadScene(SceneConstants.LevelSelectScene);
    }
    
    /// <summary>
    /// Load a scene, running all pre-level loading handlers (TODO and handling the loading screen)
    /// <br />
    /// The Level Select manager handles loading <i>game levels</i> (and their locked/unlocked status) through the <see cref="StartLevel"/> method.
    /// Do not use this method for <i>game levels</i> unless you want to bypass the level select screen and ignore level locked status.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load. Do not use magic strings, see <see cref="SceneConstants"/></param>
    public void LoadScene(string sceneName)
    {
        ShowLoadingScreen();
        GlobalPlayerManager.Instance?.PrepareAllPlayersForSceneChange();
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Start a level based on the levelIndex. Used in the level select screen / level select manager.
    /// </summary>
    /// <param name="levelIndex"></param>
    public void StartLevel(int levelIndex)
    {
        Debug.Log("Starting Level at index " + levelIndex + " (" + GameConfig.Levels[levelIndex].sceneName + ")");
        if (levelIndex >= GameConfig.Levels.Length)
        {
            Debug.LogError("Level index out of range");
            return;
        }
        
        var level = GameConfig.Levels[levelIndex];
        
        if (level.status != LevelStatus.Locked)
        {
            LoadScene(level.sceneName);
        }
        else
        {
            Debug.Log("Level at index " + levelIndex + " is locked");
        }
    }

    private void ShowLoadingScreen()
    {
        _loadingScreenRoot.style.display = DisplayStyle.Flex;
    }

    private void HideLoadingScreen()
    {
        _loadingScreenRoot.style.display = DisplayStyle.None;
    }

    private void SanityCheckSceneNames()
    {
        string[] sceneNames = new string[SceneManager.sceneCountInBuildSettings];
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var regex = new Regex(@"(?<=/)\w+(?=\.unity)");
            sceneNames[i] = regex.Match(SceneUtility.GetScenePathByBuildIndex(i)).ToString();
        }

        for (int i = 0; i < GameConfig.Levels.Length; i++)
        {
            if (!sceneNames.Contains(GameConfig.Levels[i].sceneName))
            {
                Debug.LogError("Level at index " + i + " (Display Name: \"" + GameConfig.Levels[i].displayName + "\") has scene name \"" + GameConfig.Levels[i].sceneName + "\" but it's not in the scene list.");
                Debug.Log("Scene List: " + string.Join(", ", sceneNames));
            }
        }
    }
}

public struct Level
{
    public string displayName;
    public string sceneName;
    public LevelStatus status;
    public string levelArtSpriteName;
    // The name of the scene corresponding to the level to unlock after this level is completed.
    public string[] unlocksScenes;

    public Level(string displayName, string sceneName, LevelStatus status, string[] unlocksScenes = null, string levelArtSpriteName = "default_level")
    {
        if (levelArtSpriteName == "")
        {
            levelArtSpriteName = "default_level";
        }
            
        this.displayName = displayName;
        this.sceneName = sceneName;
        this.status = status;
        if (unlocksScenes == null)
        {
            unlocksScenes = Array.Empty<string>();
        }
        this.unlocksScenes = unlocksScenes;
        this.levelArtSpriteName = "LevelSelect/" + levelArtSpriteName;
    }

    public Sprite GetLevelArtSprite()
    {
        // For some reason Resources.Load<Sprite> doesn't work
        var texture = Resources.Load<Texture2D>(levelArtSpriteName);
        var resource = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        return resource;
    }
}

public enum LevelStatus
{
    Locked,
    Unlocked,
    Started,
    Completed
}