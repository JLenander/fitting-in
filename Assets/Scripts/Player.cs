using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    // lookSensitivity controls the camera sensitivity for the player as a plorp only (not in terminal)
    [SerializeField] private float lookSensitivity;

    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.5f;
    private CharacterController _characterController;
    private Camera _playerCamera;
    private Camera _outsideCamera;
    private InputAction _moveAction;
    private InputAction _lookAction;
    
    private float xRotationPlayerCam = 0f;
    private float yRotationPlayerCam = 0f; // left/right (yaw)
    private float xRotationExternalCam = 0f;
    private float yRotationExternalCam = 0f;
    
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    private Vector3 velocity;
    private bool isGrounded;

    private bool disableMovement = false;
    private bool disableRotate = false;
    // If true, player is in some UI and should not have normal control.
    private bool inPauseMenu = false;

    private delegate void ControlFunc();
    private ControlFunc _controlFunc;

    private Animator animator;

    private float stepTimer;

    private RobotMovement _robotMovement;
    private int playerID;
    
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        var input = GetComponent<PlayerInput>();
        _playerCamera = input.camera;
        _moveAction = InputActionMapper.GetPlayerMoveAction(input);
        _lookAction = InputActionMapper.GetPlayerLookAction(input);
        animator = GetComponentInChildren<Animator>();
        _characterController.enabled = false;
        this.transform.position = new Vector3(-1.0f, 5.0f, -3.0f);
        _characterController.enabled = true;

        _controlFunc = ControlPlayer;

        footstepSource.volume = 0.1f;
    }

    void FixedUpdate()
    {
        // No control if in some UI.
        if (inPauseMenu) return;
        
        _controlFunc();
    }

    private void ControlPlayer()
    {
        if (!disableMovement)
        {
            isGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);
            // Movement

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // small downward force to keep grounded
            }

            Vector2 moveValue = _moveAction.ReadValue<Vector2>();
            // Camera directions (ignore vertical tilt)
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            // Combine input with camera directions
            Vector3 moveDir = (forward * moveValue.y + right * moveValue.x).normalized;
            _characterController.Move(moveDir * moveSpeed * Time.deltaTime);

            velocity.y += gravity;
            _characterController.Move(velocity * Time.deltaTime);

            float speed = moveDir.magnitude;
            animator.SetFloat("Speed", speed);

            if (speed != 0)
            {
                stepTimer -= Time.fixedDeltaTime;
                if (stepTimer <= 0f)
                {
                    PlayFootstep();
                    stepTimer = stepInterval;
                }
            }
        }

        if (!disableRotate)
        {
            // Look
            Vector2 lookValue = _lookAction.ReadValue<Vector2>();
            xRotationPlayerCam -= lookValue.y * lookSensitivity;
            yRotationPlayerCam -= lookValue.x * lookSensitivity * -1;

            transform.localRotation = Quaternion.Euler(0f, yRotationPlayerCam, 0f);
            xRotationPlayerCam = Math.Clamp(xRotationPlayerCam, -90f, 90f);
            _playerCamera.transform.localRotation = Quaternion.Euler(xRotationPlayerCam, 0f, 0f);
        }
    }

    private void ControlEyeCam()
    {
        // Look
        Vector2 lookValue = _lookAction.ReadValue<Vector2>();
        xRotationExternalCam -= lookValue.y * lookSensitivity;
        yRotationExternalCam -= lookValue.x * lookSensitivity * -1;

        yRotationExternalCam = Math.Clamp(yRotationExternalCam, -70f, 70f);
        xRotationExternalCam = Math.Clamp(xRotationExternalCam, -50f, 70f);
        _outsideCamera.transform.localRotation = Quaternion.Euler(xRotationExternalCam, yRotationExternalCam, 0f);
    }

    private void SwitchOnConsole()
    {
        disableMovement = true;
        disableRotate = false;
    }

    private void SwitchOffConsole()
    {
        disableMovement = false;
        disableRotate = false;
    }


    public void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            int index = UnityEngine.Random.Range(0, footstepClips.Length);
            footstepSource.PlayOneShot(footstepClips[index]);
        }
    }

    /// <summary>
    /// Disable all player control
    /// </summary>
    public void TurnOff()
    {
        disableMovement = true;
        disableRotate = true;
    }

    /// <summary>
    /// Enable all player control
    /// </summary>
    public void TurnOn()
    {
        disableMovement = false;
        disableRotate = false;
    }

    public void switchToHead(Camera outsideCamera)
    {
        SwitchOnConsole();
        
        _outsideCamera = outsideCamera;
        _controlFunc = ControlEyeCam;
    }

    public void switchOffHead()
    {
        SwitchOffConsole();
        _controlFunc = ControlPlayer;
    }

    public void switchToLegs(Transform robotBody)
    {
        SwitchOnConsole();
        _robotMovement = robotBody.GetComponent<RobotMovement>();
        _robotMovement.SetMoveAction(_moveAction);
        _robotMovement.SetLookAction(_lookAction);
        _controlFunc = _robotMovement.ControlRobotMovement;
    }

    public void switchOffLegs()
    {
        SwitchOffConsole();
        _robotMovement = null;

        _controlFunc = ControlPlayer;
    }
    
    /// <summary>
    /// Set the player's state so they are in the pause menu and cannot control anything.
    /// </summary>
    public void SetInPauseMenu()
    {
        inPauseMenu = true;
    }

    /// <summary>
    /// Set the player's state so they are not in the pause menu so they can control something.
    /// </summary>
    public void SetNotInPauseMenu()
    {
        inPauseMenu = false;
    }

    public Color GetPlayerColor()
    {
        return GlobalPlayerManager.Instance.Players[playerID].PlayerColor;
    }

    public int GetPlayerID()
    {
        return playerID;
    }

    public void SetPlayerID(int num)
    {
        playerID = num;
    }

    /// <summary>
    /// Get the look (camera) sensitivity for the player for when they are not in a terminal.
    /// </summary>
    /// <returns></returns>
    public float GetLookSensitivity()
    {
        return lookSensitivity;
    }

    /// <summary>
    /// Set the look (camera) sensitivity for the player for when they are not in a terminal.
    /// </summary>
    /// <param name="sensitivity"></param>
    public void SetLookSensitivity(float sensitivity)
    {
        lookSensitivity = sensitivity;
    }
}
