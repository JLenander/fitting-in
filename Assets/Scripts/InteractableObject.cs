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
    private HandMovement handMovement;
    private Hand hand;
    [SerializeField] protected Transform interactPopUp;
    public Transform _robotHead;

    public virtual void Start()
    {
        DisableOutline();
        if (interactPopUp != null)
        {
            interactPopUp.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("Hand") && canPickup)
        {
            EnableOutline();
            canInteract = true;
            hand = other.GetComponent<Hand>();
            if (hand != null)
            {
                handMovement = hand.GetHandMovement();
                handMovement.SetCurrentInteractableObject(gameObject, true);

                if (interactPopUp != null && _robotHead != null)
                {
                    interactPopUp.LookAt(_robotHead);
                    interactPopUp.Rotate(0f, 180f, 0f);
                    GameObject currPlayer = handMovement.GetCurrPlayer();
                    if (currPlayer != null)
                    {
                        if (interactPopUp.GetComponent<TextMeshProUGUI>() != null)
                        {
                            interactPopUp.GetComponent<TextMeshProUGUI>().color = currPlayer.GetComponent<Player>().GetPlayerColor();
                        }
                    }

                    interactPopUp.gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.CompareTag("Hand") && canPickup)
        {
            DisableOutline();
            canInteract = false;
            if (handMovement != null)
                handMovement.SetCurrentInteractableObject(null, false);
            if (interactPopUp != null)
            {
                interactPopUp.gameObject.SetActive(false);
            }
        }
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }

    public void EnableOutline()
    {
        outline.enabled = true;
    }

    public virtual void InteractWithHand(Transform wrist, HandMovement target)
    {
    }

    public virtual void StopInteractWithHand(HandMovement target)
    {
    }

    public void SetCanInteractTrue()
    {
        canInteract = true;
    }

    public void SetCanInteractFalse()
    {
        canInteract = false;
    }
}
