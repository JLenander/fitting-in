using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSetup : MonoBehaviour
{
    private PlayerInput playerInput;
    public int playerId;

    public Extinguisher extinguisher;

    public GameObject playerGraphic;

    public float fireCheckRadius = 0.5f;
    public LayerMask fireLayer;
    private SplitscreenUIHandler splitscreenUIHandler;

    void Awake()
    {
        // Force load the player
        DontDestroyOnLoad(gameObject);
        playerInput = GetComponent<PlayerInput>();
        playerId = playerInput.playerIndex + 1;

        int layer = LayerMask.NameToLayer("Player" + playerId);

        // Put renderers on correct layer
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            if (r.CompareTag("Ignore"))
                continue;

            r.gameObject.layer = layer;
        }

        // Setup that player’s camera (if it has one)
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
            cam.cullingMask &= ~(1 << layer);

        splitscreenUIHandler = FindAnyObjectByType<SplitscreenUIHandler>();
    }

    void Update()
    {
        bool inFire = false;

        // check for colliders on fire layer overlapping player
        // need to specify trigger colliders
        Collider[] hits = Physics.OverlapSphere(transform.position, fireCheckRadius, fireLayer, QueryTriggerInteraction.Collide);

        if (hits.Length > 0)
        {
            inFire = true;
        }


        if (inFire)
            splitscreenUIHandler.EnablePlayerBurnOverlay(playerId - 1);
        else
            splitscreenUIHandler.DisablePlayerBurnOverlay(playerId - 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        // Draw main spawn area
        Gizmos.DrawWireSphere(transform.position, fireCheckRadius);
    }
}
