using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LegUIHandler : TerminalUIHandler
{
    public static LegUIHandler Instance;
    public void Awake()
    {
        Instance = this;
    }
    
    /// <summary>
    /// Show the terminal Ui for the specified player
    /// </summary>
    /// <param name="player"></param>
    public override void ShowUI(GameObject player)
    {
        if (player != null) 
        {
            var playerId = player.GetComponent<PlayerInput>().playerIndex;

            var img = root.Query<VisualElement>("ControlsImg").First();
            img.style.backgroundImage = ButtonIconManager.Instance.GetControlsImageForPlayer(
                playerId,
                ButtonIconManager.ControlsImage.LegTerminalControls
            );
            splitscreenUIHandler.SetTerminalUIForPlayer(playerId, root);
        }
        else
        {
            Debug.LogError("Null player passed to terminal ui handler");
        }
    }
}
