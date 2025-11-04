using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class FoodBite : InteractableObject, IPooledObject
{
    private Food _foodBiteSpawner;
    private Transform ogParent;
    private Rigidbody rb;
    private Bag bag;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform popUp;
    [SerializeField] private Transform graphic;
    public float floatSpeed = 1f;
    public int score = 2;

    public AudioSource audioSource;
    
    // The offset to place the object in on pickup for the hand
    public Vector3 handOffset = new Vector3(0f, 3.5f, -1.7f);

    public override void Start()
    {
        base.Start();
        popUp.gameObject.SetActive(false);
        rb = GetComponent<Rigidbody>();
    }

    public void OnSpawn()
    {
        popUp.gameObject.SetActive(false);
        graphic.gameObject.SetActive(true);
        gameObject.SetActive(true);
        ogParent = transform.parent;

        rb = GetComponent<Rigidbody>();
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
            Debug.Log("pickup success");

            target.handAnimator.SetTrigger("Hold"); // sets current hand to hold anim
            target.SetTargetCurrentObject(this);

            if (bag != null) bag.EnableOutline();
        }
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        // return to original position
        transform.parent = ogParent;
        canPickup = true;
        rb.isKinematic = false;
        target.handAnimator.SetTrigger("Neutral"); // sets the current hand back to neutral
        if (bag != null) bag.DisableOutline();
        DisableOutline();
    }

    public void SetFoodBiteSpawner(Food foodBiteSpawner)
    {
        _foodBiteSpawner = foodBiteSpawner;
        _robotHead = _foodBiteSpawner._robotHead;
    }

    public void SetBag(GameObject bagObj)
    {
        bag = bagObj.GetComponent<Bag>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (audioSource != null)
            audioSource.Play();

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("Hit ground!");
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            transform.position = collision.contacts[0].point + Vector3.up * 0.01f;

            canInteract = false;
        }
        else if (collision.gameObject.CompareTag("Bag"))
        {
            if (NovaLevel1Manager.Instance.ate) 
            {
                ScoreKeeper.Instance.ModifyScore(score);
                ScoreKeeper.Instance.IncrementScoring("Spaghetti completion");
            }
            Debug.Log("Eating point");
            NovaLevel1Manager.Instance.ate = true;
            StartCoroutine(DisappearRoutine());
        }
    }

    IEnumerator DisappearRoutine()
    {
        float duration = 2f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        rb.isKinematic = true;
        popUp.LookAt(_foodBiteSpawner._robotHead);
        popUp.Rotate(0f, 180f, 0f);
        popUp.gameObject.SetActive(true);
        graphic.gameObject.SetActive(false);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Move upward
            popUp.position = startPos + Vector3.up * (elapsed * floatSpeed);

            yield return null;
        }

        // disable the object (delayed to play animation)
        rb.isKinematic = false;
        gameObject.SetActive(false);
    }
}
