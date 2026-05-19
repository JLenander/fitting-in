using UnityEngine;
using UnityEngine.Serialization;

public class CakeSlice : InteractableObject
{
    public Collider triggerCollider;
    // The offset to place the object in on pickup for the hand
    [FormerlySerializedAs("handOffset")] public Vector3 leftHandOffset = new Vector3(-5.12f, 5.43f, -0.44f);
    public Vector3 rightHandOffset = new Vector3(3.72f,3.89f,-2.77f);
    public CakeManager cakeManager;

    private Rigidbody rb;
    private Transform ogParent;
    private bool first;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        DisableOutline();
        ogParent = transform.parent;
        first = true;
    }

    public override InteractableObject InteractWithHand(Transform obj, HandMovement target)
    {
        if (canInteract && canPickup)
        {
            base.InteractWithHand(obj, target);
            // remove from cake list
            if (first)
            {
                cakeManager.GrabbedSlice(this);
                first = false;
            }
            
            // move to hand
            transform.parent = obj;
            transform.localPosition = target.left ? leftHandOffset : rightHandOffset;
            transform.localRotation = Quaternion.Euler(-88f, 10f, 0f);
            canPickup = false;

            rb.isKinematic = true;
            triggerCollider.enabled = false;

            target.handAnimator.SetTrigger("Grab"); // sets current hand to hold anim

            return this;
        }

        return null;
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        base.StopInteractWithHand(target);
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
