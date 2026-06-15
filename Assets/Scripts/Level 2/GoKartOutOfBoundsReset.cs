using UnityEngine;

public class GoKartOutOfBoundsReset : MonoBehaviour
{
    [Header("Kart")]
    [SerializeField] private GoKartController goKartController;
    [SerializeField] private Rigidbody kartRigidbody;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool useKartStartAsRespawn = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        if (goKartController == null)
        {
            goKartController = FindAnyObjectByType<GoKartController>();
        }

        if (kartRigidbody == null && goKartController != null)
        {
            kartRigidbody = goKartController.GetComponent<Rigidbody>();
        }

        if (useKartStartAsRespawn && goKartController != null)
        {
            startPosition = goKartController.transform.position;
            startRotation = goKartController.transform.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsKartCollider(other))
        {
            return;
        }

        TeleportKartToStart();
    }

    public void TeleportKartToStart()
    {
        if (goKartController == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[{name}] Cannot reset kart: GoKartController is not assigned.");
            }
            return;
        }

        Vector3 targetPosition;
        Quaternion targetRotation;

        if (respawnPoint != null)
        {
            targetPosition = respawnPoint.position;
            targetRotation = respawnPoint.rotation;
        }
        else
        {
            targetPosition = startPosition;
            targetRotation = startRotation;
        }

        if (kartRigidbody != null)
        {
            kartRigidbody.linearVelocity = Vector3.zero;
            kartRigidbody.angularVelocity = Vector3.zero;
        }

        goKartController.transform.SetPositionAndRotation(targetPosition, targetRotation);

        if (enableDebugLogs)
        {
            Debug.Log($"[{name}] Kart reset to {targetPosition}.");
        }
    }

    private bool IsKartCollider(Collider other)
    {
        if (other == null || goKartController == null)
        {
            return false;
        }

        Transform kartTransform = goKartController.transform;
        Transform otherTransform = other.transform;

        if (otherTransform == kartTransform || otherTransform.IsChildOf(kartTransform) || kartTransform.IsChildOf(otherTransform))
        {
            return true;
        }

        if (kartRigidbody != null && other.attachedRigidbody == kartRigidbody)
        {
            return true;
        }

        return false;
    }
}
