using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns the evidence and shoots it, might be more than 1 in the scene
/// </summary>
public class EvidenceSpawner : MonoBehaviour
{
    public int maxEvidence = 5;
    public float spawnInterval = 5f;
    public Transform[] spawnAnchors;
    public float launchForce = 10f;
    public Transform robotHead;
    public AudioSource audioSource;

    public GameObject uniqueEvidence;

    public float numSeconds = 7;
    public int numTimes = 10;
    // private float spawnTimer;
    private int evidenceCount; // keep track of num evidences in scene
    private ObjectPooler objectPooler;
    private string[] evidenceTypes = { "Notepad", "Map", "Polaroid" }; // types of evidence
    private bool disabled = true;

    void Start()
    {
        objectPooler = ObjectPooler.Instance;
        evidenceCount = 0;
        // spawnTimer = 0; // start the timer
        uniqueEvidence.SetActive(false);
    }

    // Update is called once per frame
    // Disable for memory leak
    // TODO: investigate
    // void Update()
    // {
    //     if (disabled) return;
    //     spawnTimer += Time.deltaTime;
    //     if (spawnTimer >= spawnInterval)
    //     {
    //         spawnTimer = 0;
    //         // if half grabbed, respawn evidence
    //         if (evidenceCount < maxEvidence) // make sure it is less than the max
    //         {
    //             EvidenceBurst();
    //         }
    //     }
    // }

    public void SpawnTempSpecial()
    {
        if (audioSource != null)
            audioSource.Play();

        uniqueEvidence.SetActive(true);
        Evidence evidence = uniqueEvidence.GetComponent<Evidence>();
        evidence.SetEvidenceSpawner(this);
    }

    public void EvidenceBurst()
    {
        if (NovaLevel1Manager.Instance.talking)
            StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        float delay = numSeconds / numTimes;

        for (int i = 0; i < numTimes; i++)
        {
            SpawnEvidence();
            yield return new WaitForSeconds(delay);
        }
    }
    void SpawnEvidence()
    {
        if (audioSource != null)
            audioSource.Play();
        // pick a random evidence
        string randomType = evidenceTypes[Random.Range(0, evidenceTypes.Length)];

        // pick a random anchor
        Transform anchor = spawnAnchors[Random.Range(0, spawnAnchors.Length)];

        GameObject evidenceObj = objectPooler.SpawnFromPool(randomType, anchor.position, anchor.rotation);

        Evidence evidence = evidenceObj.GetComponent<Evidence>();

        // give reference to spawn to evidence
        if (evidence != null)
        {
            evidence.SetEvidenceSpawner(this);
        }

        // get rb refernece to launch
        Rigidbody rb = evidenceObj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // reset before applying force
            rb.AddForce(anchor.forward * launchForce, ForceMode.Impulse);
        }


        evidenceCount++;
    }

    public void ReduceCount()
    {
        evidenceCount--;
        if (disabled)
        {
            disabled = false;
        }
    }
}
