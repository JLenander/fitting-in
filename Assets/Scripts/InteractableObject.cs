using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public abstract class InteractableObject : MonoBehaviour
{
    public Outline outline;
    public bool canInteract = true;
    public bool canPickup = true;
    public bool canDrop = true;
    public HandMovement handMovement;
    private Hand hand;
    public Transform _robotHead;
    [SerializeField] private bool inHand = false;

    public virtual void Start()
    {
        DisableOutline();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("Hand") && canPickup)
        {
            EnableOutline();
            canInteract = true;
            hand = other.GetComponent<Hand>();
            if (hand != null)
            {
                handMovement = hand.GetHandMovement();
                handMovement.AddInteractableObject(this);
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other != null && other.CompareTag("Hand") && canPickup)
        {
            DisableOutline();
            canInteract = false;
            if (handMovement != null)
                handMovement.RemoveInteractableObject(this);
        }
    }

    public void DisableOutline()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    public void EnableOutline()
    {
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    /// <summary>
    /// Interact with the robot arm's hand in some way. Returns the object the hand should consider it is interacting with.
    /// </summary>
    /// <param name="wrist">The transform of the hand base (wrist)</param>
    /// <param name="target">The hand this object is interacting with</param>
    /// <returns>The object the hand is interacting with after triggering an interact on this object.</returns>
    public virtual InteractableObject InteractWithHand(Transform wrist, HandMovement target)
    {
        inHand = true;
        return null;
    }

    public virtual void StopInteractWithHand(HandMovement target)
    {
        inHand = false;
    }

    public void EnableCanInteract()
    {
        canInteract = true;
    }

    public void DisableCanInteract()
    {
        canInteract = false;
    }

    public void EnableCanPickup()
    {
        canPickup = true;
    }

    public void DisableCanPickup()
    {
        canPickup = false;
    }

    public bool isInHand()
    {
        return inHand;
    }
}
