using System.Collections;
using UnityEngine;

public class HurtDateChecker : MonoBehaviour
{
    [SerializeField] private Transform spineTarget;
    [SerializeField] private float bendDistance = 0.2f;
    [SerializeField] private float returnSpeed = 0.5f;

    // [SerializeField] private DialogueScriptableObj hurtDialogue;

    public AudioSource audioSource;
    private Vector3 original;
    private Quaternion startRot;
    private Coroutine bendRoutine;
    private ScoreKeeper scoreKeeper;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spineTarget != null)
        {
            original = spineTarget.localPosition;
            startRot = spineTarget.localRotation;
        }


        scoreKeeper = ScoreKeeper.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("Hand"))
        {         // Find direction from the character to the hit
            Vector3 hitDir = (other.transform.position - spineTarget.position).normalized;

            // Bend target opposite the hit direction
            Vector3 bendOffset = -hitDir * bendDistance;
            bendOffset.y = 0; // optional: ignore vertical bend

            if (bendRoutine != null) StopCoroutine(bendRoutine);
            bendRoutine = StartCoroutine(BendOpposite(bendOffset));

            if (audioSource != null)
                audioSource.Play();

            // deduct points from player
            scoreKeeper.IncrementHurtDate();
            scoreKeeper.ModifyScore(-1);

            // GlobalPlayerUIManager.Instance.LoadText(hurtDialogue);
        }
    }

    IEnumerator BendOpposite(Vector3 bendOffset)
    {
        Vector3 targetPos = original + bendOffset;
        float t = 0;

        Quaternion targetRot = startRot * Quaternion.Euler(bendOffset.z * 5f, -bendOffset.x * 5f, 0f);

        // Move to bent position
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;

            spineTarget.localPosition = Vector3.Lerp(spineTarget.localPosition, targetPos, t);
            spineTarget.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // Return to neutral
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            spineTarget.localPosition = Vector3.Lerp(spineTarget.localPosition, original, t);
            spineTarget.localRotation = Quaternion.Slerp(targetRot, startRot, t);
            yield return null;
        }

        spineTarget.localPosition = original;
        spineTarget.localRotation = startRot;
    }
}
