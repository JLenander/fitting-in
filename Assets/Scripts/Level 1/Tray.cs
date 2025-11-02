using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Tray : MonoBehaviour
{
    public AttachPoint leftAttach;
    public AttachPoint rightAttach;

    public GameObject drinkPrefab;
    public Transform leftDrinkSpawn;
    public Transform rightDrinkSpawn;
    private GameObject leftDrinkInstance;
    private GameObject rightDrinkInstance;
    
    public Transform traySpawnPoint;
    private Rigidbody rb;
    private bool isTwoHanded;
    private Transform ogParent;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        ogParent = transform.parent;
        
        StartCoroutine(SpawnCupsSmooth()); // initial spawn
    }
    
    public void OnAttachPointGrabbed()
    {
        if (leftAttach.isHeld && rightAttach.isHeld)
        {
            leftAttach.currentHand.attachedCheckGrapple();
            rightAttach.currentHand.attachedCheckGrapple();
            StartTwoHandControl();
        }
    }

    public void OnAttachPointReleased()
    {
        // If either hand released, stop tray motion
        if (isTwoHanded)
        {
            StopTwoHandControl();
        }
    }

    private void StartTwoHandControl()
    {
        isTwoHanded = true;
        rb.isKinematic = true;
        // Unfreeze both hands so they can move freely while carrying
        leftAttach.currentHand.FreezeWristPosition(false);
        rightAttach.currentHand.FreezeWristPosition(false);

        StartCoroutine(FollowHands());
        Debug.Log("Tray picked up by both hands");
    }

    private void StopTwoHandControl()
    {
        isTwoHanded = false;
        StopAllCoroutines();
        rb.isKinematic = false;
        transform.parent = ogParent;
        leftAttach.LetGoCurrentHand();
        rightAttach.LetGoCurrentHand();
        Debug.Log("Tray released");
    }

    IEnumerator FollowHands()
    {
        while (isTwoHanded)
        {
            Vector3 leftPos = leftAttach.currentHand.transform.position;
            Vector3 rightPos = rightAttach.currentHand.transform.position;

            Vector3 rightDir = (rightPos - leftPos).normalized;
            Vector3 forwardDir = Vector3.Cross(Vector3.up, rightDir).normalized; // forward along tray's depth
            Vector3 upDir = Vector3.Cross(rightDir, forwardDir).normalized;      // recompute up based on both hands

            Quaternion targetRot = Quaternion.LookRotation(forwardDir, upDir); // target rotation based on hands
            Quaternion newRot = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            transform.rotation = newRot;

            Vector3 mid = (leftPos + rightPos) / 2f;

            // get attach points midpoint in the tray's local space (robust even if attach points aren't direct children)
            Vector3 leftLocal = transform.InverseTransformPoint(leftAttach.transform.position);
            Vector3 rightLocal = transform.InverseTransformPoint(rightAttach.transform.position);
            Vector3 attachMidLocal = (leftLocal + rightLocal) * 0.5f;

            // place the tray so the attach-mid (in local space rotated by newRot) maps to the hands' midpoint
            Vector3 desiredPos = mid - (newRot * attachMidLocal);

            // Make side movement heavier
            float horizontalResponsiveness = 2f;
            // Lerp less  on X/Z, normal on Y
            Vector3 newPos = transform.position;
            newPos.x = Mathf.Lerp(newPos.x, desiredPos.x, Time.deltaTime * horizontalResponsiveness);
            newPos.z = Mathf.Lerp(newPos.z, desiredPos.z, Time.deltaTime * horizontalResponsiveness);
            newPos.y = Mathf.Lerp(newPos.y, desiredPos.y, Time.deltaTime * 10f);

            transform.position = newPos;
            
            // Note: can't freeze xy because then moving body doesnt move tray

            yield return null;
        }
    }
    
    private void SpawnCups()
    {
        // Destroy any old cups
        if (leftDrinkInstance) Destroy(leftDrinkInstance);
        if (rightDrinkInstance) Destroy(rightDrinkInstance);
        
        StartCoroutine(SpawnCupsSmooth());
    }
    
    private IEnumerator SpawnCupsSmooth()
    {
        // Temporarily freeze tray
        rb.isKinematic = true;
        
        // Spawn new cups
        leftDrinkInstance = Instantiate(drinkPrefab, leftDrinkSpawn.position, Quaternion.identity, transform);
        rightDrinkInstance = Instantiate(drinkPrefab, rightDrinkSpawn.position, Quaternion.identity, transform);

        Rigidbody leftRb = leftDrinkInstance.GetComponent<Rigidbody>();
        Rigidbody rightRb = rightDrinkInstance.GetComponent<Rigidbody>();

        leftRb.isKinematic = true;
        rightRb.isKinematic = true;
        
        // Wait for physics to settle
        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(0.1f);

        leftDrinkInstance.GetComponent<Drink>().Init(this);
        rightDrinkInstance.GetComponent<Drink>().Init(this);

        // Wait one physics tick
        yield return new WaitForFixedUpdate();

        // Unfreeze everything
        leftRb.isKinematic = false;
        rightRb.isKinematic = false;
        rb.isKinematic = false;
    }

    public void OnCupFell()
    {
        Debug.Log("A cup fell! Resetting tray task...");
        
        // TODO: enable reset when cup falling not too hard
        // StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        // Small delay for clarity
        yield return new WaitForSeconds(1f);

        // Reset tray position
        transform.parent = ogParent;
        transform.position = traySpawnPoint.position;
        transform.rotation = traySpawnPoint.rotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;

        // Respawn cups
        SpawnCups();

        // Detach hands if still attached
        if (isTwoHanded)
        {
            StopTwoHandControl();
        }

        Debug.Log("Tray reset complete");
    }
    
    public bool IsTwoHanded()
    {
        return isTwoHanded;
    }

    public void TrySnapToTable(Transform snapPoint)
    {
        StopTwoHandControl();

        // Snap tray safely
        rb.isKinematic = true;
        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;

        Debug.Log("Tray placed on table!");
        var outline = GetComponent<Outline>();
        if (outline) outline.enabled = false;
    }
}
    