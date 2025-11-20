using FMODUnity;
using UnityEngine;

public class Ball : InteractableObject
{
    public Collider triggerCollider;
    public Collider grappleCollider;
    private Transform parent;
    private Rigidbody rg;
    public StudioEventEmitter hoopSfx;
    public StudioEventEmitter groundSfx;
    float velocity;
    float minSpeed = 0f;
    float maxSpeed = 10f;

    private float globalGravity = -9.81f;
    private float gravityScale = 5.0f;
    private bool applyGravity = true;

    public override void Start()
    {
        base.Start();
        parent = transform.parent;

        rg = GetComponent<Rigidbody>();
        rg.useGravity = false;

        hoopSfx.SetParameter("ballspeed", velocity);
    }

    void FixedUpdate()
    {
        if (applyGravity)
        {
            Vector3 gravity = globalGravity * gravityScale * Vector3.up;
            rg.AddForce(gravity, ForceMode.Acceleration);

        }
    }

    public override void InteractWithHand(Transform obj, HandMovement target)
    {
        if (canInteract && canPickup)
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.grabBall = true;
            }

            // move to hand
            DisableOutline();
            applyGravity = false;
            transform.parent = obj;
            transform.localPosition = new Vector3(-1.12f, 1.88f, -7.67f);
            //transform.localRotation = Quaternion.Euler(-88f, 10f, 0f);
            canPickup = false;

            rg.isKinematic = true;
            rg.interpolation = RigidbodyInterpolation.None;
            triggerCollider.enabled = false;
            Debug.Log("pickup success");

            target.SetTargetCurrentObject(this);

            target.handAnimator.SetTrigger("Grab"); // sets current hand to hold anim

            grappleCollider.enabled = false;
        }
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        // return to original position
        applyGravity = true;
        transform.parent = parent;

        canPickup = true;

        rg.isKinematic = false;
        rg.interpolation = RigidbodyInterpolation.Interpolate;
        triggerCollider.enabled = true;
        target.handAnimator.SetTrigger("Neutral"); // sets the current hand back to neutral

        grappleCollider.enabled = true;
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.dropBall = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Hoop")
        {
            // hit the hoop
            velocity = rg.linearVelocity.magnitude;
            velocity = Mathf.InverseLerp(minSpeed, maxSpeed, velocity);
            hoopSfx.SetParameter("ballspeed", velocity);
            hoopSfx.Play();

        }
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Velocity on contact
            velocity = rg.linearVelocity.magnitude;
            velocity = Mathf.InverseLerp(minSpeed, maxSpeed, velocity);
            groundSfx.SetParameter("ballspeed", velocity);
            groundSfx.Play();
            // TODO: add sound for ground hit
        }
    }
}
