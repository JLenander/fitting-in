using System.Collections;
using UnityEngine;

public class CoffeePot : InteractableObject
{
    public Collider triggerCollider;
    public Collider grappleCollider;
    private Quaternion ogRotation;
    private Transform ogParent;
    private Rigidbody rb;

    public float rayLength = 40f;
    public float pourThresholdAngle = 60f; // degrees below horizontal

    public AudioSource audioSource;
    [SerializeField] private HandConsole leftConsole;
    [SerializeField] private HandConsole rightConsole;

    [SerializeField] private DialogueScriptableObj burnDialogue;

    private bool isPouring = false;

    [SerializeField] private Transform spoutTip;          // assign in Inspector
    [SerializeField] private Transform coffeePour;
    private ParticleSystem coffeePourEffect;

    //[SerializeField] private float volume; // TODO: limit the pot
    private FillCup cup;
    private LayerMask layerMask;
    private bool first = true;

    void Awake()
    {
        layerMask = LayerMask.GetMask("Cup");
    }

    public override void Start()
    {
        base.Start();
        ogParent = transform.parent;
        ogRotation = transform.localRotation;

        rb = GetComponent<Rigidbody>();

        coffeePourEffect = coffeePour.GetComponent<ParticleSystem>();
        Debug.Log(coffeePourEffect);
        coffeePourEffect.Stop(); // don�t play immediately
    }

    private void Update()
    {
        Vector3 origin = spoutTip.position;
        Vector3 direction = isPouring ? Vector3.down : transform.forward.normalized;

        // visualize the spout ray
        Debug.DrawRay(spoutTip.position, direction * rayLength, isPouring ? Color.green : Color.red);

        // is it tilted downward enough
        float downwardDot = Vector3.Dot(transform.forward, Vector3.down);
        bool pouringNow = downwardDot > Mathf.Cos(pourThresholdAngle * Mathf.Deg2Rad);


        // detect transition from not-pouring to pouring
        //if (pouringNow && !isPouring && volume > 0)
        if (pouringNow && !isPouring)
        {
            isPouring = true;
            OnStartPour();
        }
        // detect transition from pouring to not-pouring
        //else if ((!pouringNow && isPouring) || volume <= 0)
        else if (!pouringNow && isPouring)
        {
            isPouring = false;
            OnStopPour();
        }

        // optional raycast visualization
        if (isPouring)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, rayLength, layerMask))
            {
                Debug.DrawLine(origin, hit.point, Color.cyan);
                if (hit.collider.CompareTag("Cup"))
                {
                    cup = hit.collider.GetComponent<FillCup>();
                    cup.AddCoffee();
                }
                else
                {
                    if (cup != null) cup.DisableOutline();
                }
            }
            //volume--;
        }
    }

    private void OnStartPour()
    {
        Debug.Log("Started pouring!");
        coffeePourEffect.Play();
        if (audioSource != null)
            audioSource.Play();
    }

    private void OnStopPour()
    {
        Debug.Log("Stopped pouring!");
        coffeePourEffect.Stop();

        if (audioSource != null)
            audioSource.Stop();
    }

    public override void InteractWithHand(Transform obj, HandMovement target)
    {
        if (canInteract && canPickup)
        {
            // move to hand
            DisableOutline();
            transform.parent = obj;
            transform.localPosition = new Vector3(0.39f, 2.27f, -7.49f);
            transform.localRotation = Quaternion.Euler(20.629f, 176.069f, -87.425f);

            Debug.Log(transform.rotation);
            canPickup = false;

            target.SetWristRotation(new Vector3(0, -10f, -10f));

            rb.isKinematic = true;
            triggerCollider.enabled = false;
            Debug.Log("pickup success");

            target.SetTargetCurrentObject(this);
            target.handAnimator.SetTrigger("Pot"); // sets current hand to pot anim

            grappleCollider.enabled = false;

            if (first) StartCoroutine(BurnArm());
        }
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        // return to original position
        transform.parent = ogParent;
        canPickup = true;
        rb.isKinematic = false;
        triggerCollider.enabled = true;
        target.handAnimator.SetTrigger("Neutral"); // sets the opposite hand back to neutral
        grappleCollider.enabled = true;
        DisableOutline();
    }

    IEnumerator BurnArm()
    {
        yield return new WaitForSeconds(5);

        // start fire
        FireManager.Instance.StartFireArea("lower");
        leftConsole.DisableInteract();
        rightConsole.DisableInteract();
        Level1TaskManager.StartTaskPutOutFires();
        Debug.Log("fire start");

        // output dialogue
        GlobalPlayerUIManager.Instance.LoadText(burnDialogue);
        first = false;
    }
}
