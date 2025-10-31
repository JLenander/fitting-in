using UnityEngine;

public class AttachPoint : InteractableObject
{
    [SerializeField] private Tray tray;
    public bool isHeld = false;
    public HandMovement currentHand;

    public override void InteractWithHand(Transform wrist, HandMovement target)
    {
        if (!canInteract || isHeld) return;

        isHeld = true;
        currentHand = target;
        DisableOutline();
        
        // Disable interactivity while held
        canInteract = false;
        canPickup = false;

        // Tell tray a hand grabbed this attach point
        target.SetTargetCurrentObject(this);
        target.handAnimator.SetTrigger("Pot");
        target.FreezeWristPosition(true);
        tray.OnAttachPointGrabbed(target);
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        if (!isHeld || currentHand != target) return;

        target.SetTargetCurrentObject(null);
        target.FreezeWristPosition(false);
        target.handAnimator.SetTrigger("Neutral");
        isHeld = false;
        currentHand = null;

        // Re-enable interaction
        canInteract = true;
        canPickup = true;
        EnableOutline();

        // Notify tray
        tray.OnAttachPointReleased();
    }
    
    public void LetGoCurrentHand()
    {
        if (currentHand != null)
        {
            currentHand.SetTargetCurrentObject(null);
            currentHand.FreezeWristPosition(false);
            currentHand.handAnimator.SetTrigger("Neutral");
            isHeld = false;
            currentHand = null;

            // Re-enable interaction
            canInteract = true;
            canPickup = true;
            EnableOutline();
        }
    }
}

