using System.Collections;
using UnityEngine;

public class Phone : InteractableObject
{
    public Collider triggerCollider;
    public HandMovement leftHandTarget;
    public HandMovement rightHandTarget;
    public Collider grappleCollider;
    private Vector3 ogPosition;
    private Quaternion ogRotation;
    private Transform parent;
    private Rigidbody rg;

    private TaskManager taskManager;

    private bool first;

    public override void Start()
    {
        base.Start();
        ogPosition = transform.localPosition;
        ogRotation = transform.localRotation;
        parent = transform.parent;

        rg = GetComponent<Rigidbody>();

        first = true;
    }

    public override void InteractWithHand(Transform obj, HandMovement target)
    {
        if (canInteract && canPickup)
        {
            // move to hand
            DisableOutline();

            transform.parent = obj;
            transform.localPosition = new Vector3(0.0f, 5.2f, -1.0f);
            transform.localRotation = Quaternion.Euler(-88f, 10f, 0f);
            canPickup = false;

            rg.isKinematic = true;
            triggerCollider.enabled = false;
            Debug.Log("pickup success");

            target.SetTargetCurrentObject(this);

            target.oppositeHandAnimator.SetTrigger("Point"); // sets the opposite hand to point
            target.handAnimator.SetTrigger("Hold"); // sets current hand to hold anim


            // hard code method for moving Z
            if (target.gameObject.name == "ArmRigTargetR")
            {
                // right hand phone and left hand point
                target.MoveTargetZ(4.23f);
                leftHandTarget.MoveTargetZ(12f);
            }
            else
            {
                target.MoveTargetZ(4.23f);
                rightHandTarget.MoveTargetZ(12f);
            }

            if (first)
            {
                first = false;
                Level0TaskManager.CompleteTaskPickupPhone();
                Level0TaskManager.StartTaskUnlock();
            }

            grappleCollider.enabled = false;
        }
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        // return to original position
        transform.parent = parent;

        // hard code revert
        leftHandTarget.RevertTargetZ();
        rightHandTarget.RevertTargetZ();

        // transform.localPosition = ogPosition;
        // transform.localRotation = ogRotation;
        canPickup = true;

        rg.isKinematic = false;
        triggerCollider.enabled = true;
        target.oppositeHandAnimator.SetTrigger("Neutral"); // sets the opposite hand back to neutral
        target.handAnimator.SetTrigger("Neutral"); // sets the current hand back to neutral

        grappleCollider.enabled = true;
    }
}
