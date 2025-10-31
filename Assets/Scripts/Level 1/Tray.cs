using System.Collections;
using UnityEngine;

public class Tray : MonoBehaviour
{
    public AttachPoint leftAttach;
    public AttachPoint rightAttach;

    private Rigidbody rb;
    private bool isTwoHanded = false;
    private Transform ogParent;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        ogParent = transform.parent;
        // trayActive = false;
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
            leftAttach.LetGoCurrentHand();
            rightAttach.LetGoCurrentHand();
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

            Quaternion targetRot = Quaternion.LookRotation(forwardDir, upDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

            Vector3 mid = (leftPos + rightPos) / 2f;

            // Slight offset so tray not in chest
            float forwardOffset = 0.10f;
            Vector3 offset = forwardDir * forwardOffset;
            Vector3 targetPos = mid + offset;

            // Make side movement heavier
            float horizontalResponsiveness = 2f;
            // Lerp less  on X/Z, normal on Y
            Vector3 newPos = transform.position;
            newPos.x = Mathf.Lerp(newPos.x, targetPos.x, Time.deltaTime * horizontalResponsiveness);
            newPos.z = Mathf.Lerp(newPos.z, targetPos.z, Time.deltaTime * horizontalResponsiveness);
            newPos.y = Mathf.Lerp(newPos.y, targetPos.y, Time.deltaTime * 10f);

            transform.position = newPos;
            
            // Note: can't freeze xy because then moving body doesnt move tray

            yield return null;
        }
    }
}
    