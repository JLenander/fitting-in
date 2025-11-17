using UnityEngine;

public class GrabbableObject : InteractableObject
{
    public Collider triggerCollider;
    public Collider grappleCollider;
    // The offset to place the object in on pickup for the hand
    public Vector3 handOffset = new Vector3(-0.66f, 3.7f, -1.58f);

    private Rigidbody rb;
    private Transform ogParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        rb = GetComponent<Rigidbody>();
        DisableOutline();
        ogParent = transform.parent;
    }

    public override void InteractWithHand(Transform obj, HandMovement target)
    {
        if (canInteract && canPickup)
        {
            // move to hand
            transform.parent = obj;
            transform.localPosition = handOffset;
            transform.localRotation = Quaternion.Euler(-88f, 10f, 0f);
            canPickup = false;

            rb.isKinematic = true;

            if (grappleCollider != null)
                grappleCollider.enabled = false;
            triggerCollider.enabled = false;

            target.handAnimator.SetTrigger("Grab"); // sets current hand to hold anim
            target.SetTargetCurrentObject(this);

            handMovement = target;
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
        if (grappleCollider != null)
            grappleCollider.enabled = true;
        triggerCollider.enabled = true;
        DisableOutline();
    }
}
