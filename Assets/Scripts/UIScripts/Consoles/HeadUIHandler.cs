using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HeadUIHandler : TerminalUIHandler
{
    public static HeadUIHandler Instance;
    public void Awake()
    {
        Instance = this;
    }
    
    /// <summary>
    /// Show the terminal Ui for the specified player (virtual so ArmUIHandlers can override)
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
                ButtonIconManager.ControlsImage.EyeTerminalControls
            );
            splitscreenUIHandler.SetTerminalUIForPlayer(playerId, root);
        }
        else
        {
            Debug.LogError("Null player passed to terminal ui handler");
        }
    }
}
