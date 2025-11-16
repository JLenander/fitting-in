using UnityEngine;

public class CakeSlice : InteractableObject
{
    public Collider triggerCollider;
    // The offset to place the object in on pickup for the hand
    public Vector3 handOffset = new Vector3(-0.66f, 3.7f, -1.58f);
    public CakeManager cakeManager;

    private Rigidbody rb;
    private Transform ogParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        DisableOutline();
        ogParent = transform.parent;
    }

    public override void InteractWithHand(Transform obj, HandMovement target)
    {
        if (canInteract && canPickup)
        {
            // remove from cake list
            cakeManager.GrabbedSlice(this);

            // move to hand
            transform.parent = obj;
            transform.localPosition = handOffset;
            transform.localRotation = Quaternion.Euler(-88f, 10f, 0f);
            canPickup = false;

            rb.isKinematic = true;
            triggerCollider.enabled = false;

            target.handAnimator.SetTrigger("Grab"); // sets current hand to hold anim
            target.SetTargetCurrentObject(this);
        }
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        canPickup = true;
        rb.isKinematic = false;
        transform.parent = ogParent;

        // Move object down slightly to avoid it bouncinng on your hand
        transform.position += Vector3.down * 1.5f;

        target.handAnimator.SetTrigger("Neutral"); // sets the current hand back to neutral
        triggerCollider.enabled = true;
        DisableOutline();
    }
}
