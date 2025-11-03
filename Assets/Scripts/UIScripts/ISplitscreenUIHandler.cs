using UnityEngine;

/// <summary>
/// A UI handler for the Splitscreen player UI
/// </summary>
public interface ISplitscreenUIHandler
{
    /// <summary>
    /// Enable a player's UI overlay
    /// </summary>
    /// <param name="playerIndex">The index of the player to enable</param>
    public void EnablePlayerOverlay(int playerIndex);

    /// <summary>
    /// Disable a player's UI overlay (Player UI overlays are disabled by default)
    /// </summary>
    /// <param name="playerIndex">The index of the player to disable</param>
    public void DisablePlayerOverlay(int playerIndex);

    /// <summary>
    /// Enable (show) and set the color and content of the text that appears when a player hovers over an interactable.
    /// </summary>
    /// <param name="playerIndex">The index of the player to set the interaction text for</param>
    /// <param name="content">The text to set</param>
    /// <param name="msgColour">The color of the text</param>
    public void EnablePlayerInteractionText(int playerIndex, string content, Color msgColour, string buttonPath);

    /// <summary>
    /// Disable (hide) the player's interaction text
    /// </summary>
    /// <param name="playerIndex">The player to hide the interaction text for</param>
    public void DisablePlayerInteractionText(int playerIndex);

    /// <summary>
    /// Dim player's screen 
    /// </summary>
    /// <param name="playerIndex">The player to hide the interaction text for</param>
    public void EnablePlayerScreenGreyscale(int playerIndex);

    /// <summary>
    /// Undim player's screen 
    /// </summary>
    /// <param name="playerIndex">The player to hide the interaction text for</param>
    public void DisablePlayerScreenGreyscale(int playerIndex);

    /// <summary>
    /// Show the outside camera (or eye camera)
    /// </summary>
    public void ShowOutsideCamera();

    /// <summary>
    /// Hide the outside camera (or eye camera)
    /// <param name="animationSeconds">The number of seconds for the transition animation</param>
    /// </summary>
    public void HideOutsideCamera(float animationSeconds);

    /// <summary>
    /// Show dialogue box
    /// </summary>
    public void InitializeDialogue();

    /// <summary>
    /// Writes text to screen
    /// </summary>
    /// <param name="content"></param>
    public void WriteDialogueText(string content);

    /// <summary>
    /// Change talking dialogue sprite
    /// </summary>
    /// <param name="sprite"></param>
    public void ChangeDialogueSprite(Sprite sprite);

    /// <summary>
    /// hides dialogue ui container
    /// </summary>
    public void HideDialogue();

    /// <summary>
    /// for when reticle hits a grapple stop collider
    /// </summary>
    public void ReticleHit();


    /// <summary>
    /// neutralize the reticle when nothing is hit
    /// </summary>
    public void ReticleNeutral();

    /// <summary>
    /// enable burn overlay
    /// </summary>
    public void EnablePlayerBurnOverlay(int playerIndex);

    /// <summary>
    /// disable burn overlay
    /// </summary>
    public void DisablePlayerBurnOverlay(int playerIndex);
}
