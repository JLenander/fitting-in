using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Bag : InteractableObject
{
    public Collider grappleCollider;
    private Transform ogParent;
    private Rigidbody rb;
    [SerializeField] private Transform popUp;
    [SerializeField] private Transform graphic;
    public float floatSpeed = 1f;
    public int score = 5;

    public override void Start()
    {
        base.Start();
        DisableOutline();
        canPickup = false;
        rb = GetComponent<Rigidbody>();
        popUp.gameObject.SetActive(false);
        graphic.gameObject.SetActive(true);
        StartCoroutine(WaitForScoreKeeper());
    }

    IEnumerator WaitForScoreKeeper()
    {
        yield return new WaitUntil(() => ScoreKeeper.Instance != null);
        ScoreKeeper.Instance.AddScoring("Discarded food", 5, false, true, 0);
    }

    public override void InteractWithHand(Transform obj, HandMovement target)
    {
        if (canInteract && canPickup)
        {
            // move to hand
            DisableOutline();
            transform.parent = obj;
            transform.localPosition = new Vector3(5f, 0f, -4f);
            transform.localRotation = Quaternion.Euler(60f, 0f, 80f);

            Debug.Log(transform.rotation);
            canPickup = false;
            rb.isKinematic = true;
            grappleCollider.enabled = false;

            target.SetWristRotation(new Vector3(0, -10f, -10f));

            Debug.Log("pickup success");

            target.SetTargetCurrentObject(this);
            target.handAnimator.SetTrigger("Pot"); // sets current hand to pot anim
        }
    }

    public override void StopInteractWithHand(HandMovement target)
    {
        // return to original position
        Quaternion currRotation = transform.rotation;
        transform.parent = ogParent;
        Vector3 currPos = transform.localPosition;
        transform.localPosition = new Vector3(currPos.x, 5.14f, currPos.z);

        transform.localRotation = Quaternion.Euler(0f, currRotation.y, 0f);

        canPickup = true;
        rb.isKinematic = false;
        grappleCollider.enabled = true;
        target.handAnimator.SetTrigger("Neutral"); // sets the opposite hand back to neutral
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (audioSource != null)
        //    audioSource.Play();

        if (collision.gameObject.CompareTag("Bag"))
        {
            Debug.Log("Discard food pont");
            StartCoroutine(DisappearRoutine());
            ScoreKeeper.Instance.IncrementScoring("Discarded food");
            Level1TaskManager.CompleteTaskDiscardFood();
            NovaLevel1Manager novaLevel1Manager = NovaLevel1Manager.Instance;
            novaLevel1Manager.bagDiscarded = true;
        }
    }

    IEnumerator DisappearRoutine()
    {
        float duration = 2f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        rb.isKinematic = true;
        popUp.LookAt(_robotHead);
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
