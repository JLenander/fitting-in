using UnityEngine;

public class AttachPoint : InteractableObject
{
    [SerializeField] private Tray tray;
    public bool isHeld;
    public HandMovement currentHand;

    public override void InteractWithHand(Transform wrist, HandMovement target)
    {
        if (!canInteract || isHeld) return;

        isHeld = true;
        currentHand = target;
        currentHand.disableGrapple(true);
        DisableOutline();
           
        // Disable interactivity while held
        canInteract = false;
        canPickup = false;

        // Tell tray a hand grabbed this attach point
        target.SetTargetCurrentObject(this);
        target.handAnimator.SetTrigger("Pot");
        target.FreezeWristPosition(true);
        tray.OnAttachPointGrabbed();
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        Debug.Log("Stop ATTACH and  " + target);
        if (!isHeld || currentHand != target) return;

        target.SetTargetCurrentObject(null);
        target.FreezeWristPosition(false);
        // currentHand.attachedCheckGrapple();
        target.handAnimator.SetTrigger("Neutral");
        
        currentHand.disableGrapple(false);
        isHeld = false;
        currentHand = null;

        // Re-enable interaction
        canInteract = true;
        canPickup = true;
        EnableOutline();

        // Notify tray
        tray.OnAttachPointReleased();
    }
    
    // need this instead of calling stopinteractwithhand because can call without currenthand reference
    public void LetGoCurrentHand()
    {
        if (currentHand != null)
        {
            currentHand.SetTargetCurrentObject(null);
            currentHand.FreezeWristPosition(false);
            currentHand.disableGrapple(false);
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

