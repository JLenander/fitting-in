using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TerminalUIHandler : MonoBehaviour
{
    public VisualTreeAsset terminalUIAsset;
    
    protected VisualElement root;
    
    protected ISplitscreenUIHandler splitscreenUIHandler;

    protected virtual void Start()
    {
        root = terminalUIAsset.CloneTree();;
        
        splitscreenUIHandler = FindAnyObjectByType<SplitscreenUIHandler>();
    }

    /// <summary>
    /// Hide the terminal Ui for the specified player
    /// </summary>
    /// <param name="player"></param>
    public void HideUI(GameObject player)
    {
        if (player != null)
        {
            var playerId = player.GetComponent<PlayerInput>().playerIndex;
            splitscreenUIHandler.ClearTerminalUIForPlayer(playerId);
        }
        else
        {
            Debug.LogError("Null player passed to terminal ui handler");
        }
    }
    
    public void ShowUI(GameObject player)
    {
        if (player != null) 
        {
            var playerId = player.GetComponent<PlayerInput>().playerIndex;
            splitscreenUIHandler.SetTerminalUIForPlayer(playerId, root);
        }
        else
        {
            Debug.LogError("Null player passed to terminal ui handler");
        }
    }
}
