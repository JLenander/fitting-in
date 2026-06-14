using System;
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
    public int playerIgnoreLayer;
    private SplitscreenUIHandler splitscreenUIHandler;

    void Awake()
    {
        // Force load the player
        DontDestroyOnLoad(gameObject);
        playerInput = GetComponent<PlayerInput>();
        playerId = playerInput.playerIndex + 1;

        // Layer for things this player's camera does not render (ex. player geometry)
        playerIgnoreLayer = LayerMask.NameToLayer("Player" + playerId + "IgnoreRender");
        if (playerIgnoreLayer == -1) throw new Exception("Player ignore render layer not found for player " + playerId);
        // Layer for things this player's camera renders (but the specified player's camera does not. Ex. other player only water stream that follows mouth)
        int playerExclusiveLayer = LayerMask.NameToLayer("Player" + playerId + "OnlyRender");
        if (playerExclusiveLayer == -1) throw new Exception("Player exclusive render layer not found for player " + playerId);

        // Put renderers on correct layer
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            // "Ignore" tag designates things that are part of this object that **should** be rendered by this own player's
            // camera (ie. Should be on a default layer or whatever layer it is set to. ie. ignore the rest of this code)
            // Tags can't be renamed
            if (r.CompareTag("Ignore"))
                continue;

            if (r.CompareTag("PlayerExclusiveRender"))
                r.gameObject.layer = playerExclusiveLayer;
            else
                r.gameObject.layer = playerIgnoreLayer;
        }

        // Setup that player’s camera (if it has one)
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null)
            throw  new Exception("Camera not found in player " + playerId);
        // Remove layer for this player's ignored rendering
        cam.cullingMask &= ~(1 << playerIgnoreLayer);
        // Add layer for this player's exclusive rendering
        cam.cullingMask |= (1 << playerExclusiveLayer);

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
