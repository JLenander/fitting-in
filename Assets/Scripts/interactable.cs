using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// Interactable is a PLORP interactable
/// </summary>
public class Interactable : MonoBehaviour
{
    // The message that shows on the screen when a player hovers over this interactable
    [FormerlySerializedAs("message")] public string hoverMessage;
    public Color msgColour = new Color(1f, 1f, 1f, 1f);
    public Outline outline;

    public Color outlineColour = new Color(1f, 1f, 1f, 1f);

    public UnityEvent onInteraction;
    public UnityEvent onReturn;
    public bool showOverlay = false;

    private Outline.Mode _originalMode;
    [SerializeField] private bool forceOutline = false;

    void Awake()
    {
        _originalMode = outline.OutlineMode;
    }
    
    void Start()
    {
        DisableOutline();
    }

    public virtual void Interact(GameObject player)
    {
        onInteraction.Invoke();

        // By default an Interactable Object is a one time interaction.
        // Override this and don't call `playerInteract.LeaveCurrInteractable()` if you want the interaction to require a leave action.
        // TODO one time interactable vs continous interactable class?
        PlayerInteract playerInteract = player.GetComponent<PlayerInteract>();
        playerInteract.LeaveCurrInteractable();
    }

    public virtual void Return(GameObject player)
    {
        onReturn.Invoke();
    }

    public void DisableOutline()
    {
        // Prevent disabling outline if outline is forced.
        if (forceOutline) return;
        outline.enabled = false;
    }

    public void EnableOutline()
    {
        outline.OutlineColor = outlineColour;
        outline.enabled = true;
    }

    /// <summary>
    /// Change the outline mode for this interactable.
    /// Primarily used by tutorial to change terminal outlines to be silhouette for better nav before reverting to normal
    /// </summary>
    public void ChangeOutlineMode(Outline.Mode mode)
    {
        outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
        outline.OutlineColor = outlineColour;
        outline.enabled = true;
    }

    /// <summary>
    /// Resets the outline mode to the setting this Interactable had when initialized.
    /// </summary>
    public void ResetOutlineModeToAssetSetting()
    {
        outline.OutlineMode = _originalMode;
    }

    /// <summary>
    /// Forces the outline to remain on always until DisableForceOutline is called.
    /// Used to prevent player looking a terminal from turning it off.
    /// Does NOT turn ON the outline. Call EnableOutline().
    /// </summary>
    public void EnableForceOutline()
    {
        forceOutline = true;
    }

    /// <summary>
    /// Disables forcing an outline on. Does not disable the outline
    /// (call DisableOutline() after disabling force outline to stop the outline).
    /// </summary>
    public void DisableForceOutline()
    {
        forceOutline = false;
    }

    public virtual bool InteractionSuccess()
    {
        return true;
    }
    public virtual bool CanInteract()
    {
        return true;
    }
}
