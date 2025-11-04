using System.Collections;
using TMPro;
using UnityEngine;

public class Evidence : InteractableObject, IPooledObject
{
    public int score = 1; // 1 - 3, or 10 depending on obj
    [SerializeField] private Transform popUp;
    [SerializeField] private Transform graphic;
    public float floatSpeed = 1f;
    public AudioSource claimSource;
    private EvidenceSpawner _evidenceSpawner;
    private bool grabbed;
    private Rigidbody rb;

    public override void Start()
    {
        base.Start();
        grabbed = false;
        popUp.gameObject.SetActive(false);
        rb = GetComponent<Rigidbody>();
        EnableOutline();

    }
    public void OnSpawn()
    {
        // enable object
        popUp.gameObject.SetActive(false);
        graphic.gameObject.SetActive(true);
        gameObject.SetActive(true);
        grabbed = false;
    }

    public override void InteractWithHand(Transform obj, HandMovement target)
    {
        if (!grabbed)
        {
            grabbed = true;
            // disintegrate it (play animation)

            NovaLevel1Manager novaLevel1Manager = NovaLevel1Manager.Instance;

            if (novaLevel1Manager != null)
            {
                if (novaLevel1Manager.grabbed == false)
                {
                    novaLevel1Manager.grabbed = true;
                }
            }

            // give a score
            ScoreKeeper.Instance.ModifyScore(score);
            ScoreKeeper.Instance.IncrementEvidence();

            if (claimSource != null)
                claimSource.Play();

            // cancel interaction so it doesnt think we're holding it 
            // (maybe set timer later when we have a disappear anim)
            target.StopInteractingWithObject(this);

            // tell spawner it was dealt
            _evidenceSpawner.ReduceCount();

            StartCoroutine(DisappearRoutine());
        }

    }

    public void SetEvidenceSpawner(EvidenceSpawner evidenceSpawner)
    {
        _evidenceSpawner = evidenceSpawner;
        _robotHead = _evidenceSpawner.robotHead;
    }

    // public override void StopInteractWithHand(HandMovement target)
    // {
    //     // nothing
    // }

    IEnumerator DisappearRoutine()
    {
        float duration = 2f;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        rb.isKinematic = true;
        popUp.LookAt(_evidenceSpawner.robotHead);
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
