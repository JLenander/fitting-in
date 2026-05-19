using System;
using UnityEditor.PackageManager;
using UnityEngine;

public class AttachPoint : InteractableObject
{
    [SerializeField] private Tray tray;
    public bool isHeld;
    public HandMovement currentHand;

    public void Awake()
    {
        // Is old script and has been modified without test.
        throw new NotImplementedException();
    }

    public override InteractableObject InteractWithHand(Transform wrist, HandMovement target)
    {
        base.InteractWithHand(wrist, target);
        if (!canInteract || isHeld) return null;

        isHeld = true;
        currentHand = target;
        currentHand.DisableGrapple(true);
        DisableOutline();
           
        // Disable interactivity while held
        canInteract = false;
        canPickup = false;

        // Tell tray a hand grabbed this attach point
        target.handAnimator.SetTrigger("Pot");
        target.FreezeWristPosition(true);
        tray.OnAttachPointGrabbed();

        return this;
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        base.StopInteractWithHand(target);
        Debug.Log("Stop ATTACH and  " + target);
        if (!isHeld || currentHand != target) return;

        target.FreezeWristPosition(false);
        // currentHand.attachedCheckGrapple();
        target.handAnimator.SetTrigger("Neutral");
        
        currentHand.DisableGrapple(false);
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
            currentHand.FreezeWristPosition(false);
            currentHand.DisableGrapple(false);
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

