using System;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSelectUIHandler : MonoBehaviour, ILevelSelectUIHandler
{
    private VisualElement _root;
    [SerializeField] private VisualTreeAsset levelTemplate;
    private VisualElement[] _levelElements;

    // Needs to initialize before Start
    public void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
    }
    
    public void SetupLevelSelectScreen(Level[] levels, Action<int> levelStartHandler)
    {
        var levelsRoot = _root.Query<VisualElement>("Levels").First();
        _levelElements = new VisualElement[levels.Length];
        
        for (int i = 0; i < levels.Length; i++)
        {
            // Create template element
            VisualElement level = levelTemplate.CloneTree();

            // Register click handler (mouse) and nav submit handler (gamepad)
            // Copy the int to pass to the event handler
            var levelIndex = i;
            level.AddManipulator(new Clickable(evt => levelStartHandler(levelIndex)));
            level.RegisterCallback<NavigationSubmitEvent>(evt => levelStartHandler(levelIndex));
            
            // Populate fields
            VisualElement levelArtImg = level.Query<VisualElement>("LevelImage").First();
            levelArtImg.style.backgroundImage = new StyleBackground(levels[i].GetLevelArtSprite());
            
            Label levelDisplayName = level.Query<Label>("LevelDisplayName").First();
            levelDisplayName.text = levels[i].displayName;
            
            // Visually indicate level state
            VisualElement lockedOverlay = level.Query<VisualElement>("LockedOverlay").First();
            switch (levels[i].status)
            {
                case  LevelStatus.Unlocked:
                    lockedOverlay.visible = false;
                    break;
                case  LevelStatus.Locked:
                    lockedOverlay.visible = true;
                    break;
                case LevelStatus.Started:
                    // TODO indicate started
                    lockedOverlay.visible = false;
                    break;
                case LevelStatus.Completed:
                    // TODO indicate Completed
                    lockedOverlay.visible = false;
                    break;
                default:
                    Debug.LogWarning("Unknown level status for level " + levels[i].sceneName);
                    break;
            }
            
            // Add to levels display
            levelsRoot.Add(level);
            _levelElements[i] = level;
        }
        
        FocusFirstLevel();
    }

    public void FocusFirstLevel()
    {
        if (_levelElements != null && _levelElements.Length > 0)
        {
            var first = _levelElements[0].Query<VisualElement>("LevelContainer").First();
            first?.Focus();    
        }
    }
}

public interface ILevelSelectUIHandler
{
    /// <summary>
    /// Intializes the level select screen which is dynamic based on the levels array
    /// </summary>
    /// <param name="levels">The array of level information from the level manager to set up</param>
    /// <param name="levelStartHandler">The handler for starting a level, passed the index of the level based on the levels array</param>
    public void SetupLevelSelectScreen(Level[] levels, Action<int> levelStartHandler);

    /// <summary>
    /// Bring focus to the first level element
    /// </summary>
    public void FocusFirstLevel();
}


