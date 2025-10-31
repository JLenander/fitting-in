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
    
    public void OnAttachPointGrabbed(HandMovement hand)
    {
        if (leftAttach.isHeld && rightAttach.isHeld)
        {
            hand.attachedCheckGrapple();
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

            Vector3 mid = (leftPos + rightPos) / 2f;
            transform.position = Vector3.Lerp(transform.position, mid, Time.deltaTime * 10f);

            Vector3 dir = rightPos - leftPos;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);

            yield return null;
        }
    }
}
    