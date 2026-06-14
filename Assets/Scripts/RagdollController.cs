using System.Collections;
using UnityEngine;

// used to manage ragdoll
public class RagdollController : Interactable
{
    public float strength = 10f;
    [SerializeField] private Transform headBone; // used to attach camera to ragdoll
    private Player currPlayer;
    private PlayerInteract currPlayerInteract;
    private Transform playerCamera;
    private Transform cameraParent;
    private Vector3 originalCameraLocalPosition;
    private Collider ragdollCollider;
    private CharacterController characterController;

    void Start()
    {
        currPlayer = gameObject.GetComponentInParent<Player>();
        currPlayerInteract = currPlayer.GetComponent<PlayerInteract>();
        characterController = currPlayer.GetComponent<CharacterController>();
        ragdollCollider = GetComponent<Collider>();
        ragdollCollider.gameObject.layer = currPlayer.GetComponent<PlayerSetup>().playerIgnoreLayer;
        playerCamera = currPlayer.GetComponentInChildren<Camera>().transform;
        cameraParent = playerCamera.transform.parent;
        originalCameraLocalPosition = playerCamera.localPosition;
    }

    // attempt to activate ragdoll
    public override void Interact(GameObject interactPlayer)
    {
        // stop interacting with whatever they were interacting with before
        currPlayerInteract.LeaveCurrInteractable();

        // enable ragdoll with direction from player to me
        Vector3 direction = transform.position - interactPlayer.transform.position;
        currPlayer.EnableRagdoll(direction, strength);

        // attach camera to ragdoll
        playerCamera.transform.parent = headBone;

        // TODO: insert SFX

        // disable collider for a bit
        ragdollCollider.enabled = false;

        PlayerInteract playerInteract = interactPlayer.GetComponent<PlayerInteract>();
        playerInteract.LeaveCurrInteractable();

        // disables ragdoll and reenable collider after delay
        // TODO: allow player to get up faster
        StartCoroutine(DisableRagdollAfterDelay(5f));
    }

    IEnumerator DisableRagdollAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        currPlayer.DisableRagdoll();
        ragdollCollider.enabled = true;

        // return camera
        playerCamera.transform.parent = cameraParent;
        playerCamera.localPosition = originalCameraLocalPosition;

        // move player object to ragdoll position
        // move up to avoid floor
        characterController.enabled = false;
        currPlayer.transform.position = headBone.position + new Vector3(0, 1, 0);
        characterController.enabled = true;
    }
}
