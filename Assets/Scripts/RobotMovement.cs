using System;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;


public class RobotMovement : MonoBehaviour
{
    private InputAction _moveAction;
    private InputAction _lookAction;

    private CharacterController _robotCharacterController;
    private Vector3 _robotVelocity;
    private bool _robotIsGrounded;
    public float robotMoveSpeed = 50f;
    public float robotLookSensitivity = 50f;
    public bool disable = false;

    public StudioEventEmitter stepSfx;
    [SerializeField] private float stepInterval = 0.02f;
    private float _stepTimer;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    // The transform for the robot's camera used to inform walk forward direction.
    [SerializeField] private Transform lookCameraTransform;
    
    void Start()
    {
        var input = GetComponent<PlayerInput>();
        _robotCharacterController = GetComponentInChildren<CharacterController>();

        if (_robotCharacterController == null)
            Debug.LogError("No CharacterController found");
    }

    private void OnEnable()
    {
        stepSfx.Stop();
    }

    public void ControlRobotMovement()
    {
        if (disable) return;
        _robotIsGrounded = Physics.CheckSphere(transform.position, groundCheckDistance, groundMask);
        // Movement

        if (_robotIsGrounded && _robotVelocity.y < 0)
        {
            _robotVelocity.y = -2f; // small downward force to keep grounded
        }
        _robotVelocity.y += gravity;

        float leftInput = _moveAction.ReadValue<Vector2>().y;
        float rightInput = _lookAction.ReadValue<Vector2>().y;

        if (Mathf.Abs(leftInput) < 0.1f) leftInput = 0;
        if (Mathf.Abs(rightInput) < 0.1f) rightInput = 0;

        // Move based on the camera forward look.
        float moveInput = (leftInput + rightInput) / 2f;
        Vector3 moveDir = getCameraForward() * moveInput + _robotVelocity;
        _robotCharacterController.Move(moveDir * robotMoveSpeed * Time.deltaTime);

        float rotateInput = (leftInput - rightInput);
        transform.Rotate(Vector3.up, rotateInput * robotLookSensitivity * Time.deltaTime);

        if (Mathf.Abs(moveInput) > 0 || Mathf.Abs(rotateInput) > 0)
        {
            GlobalPlayerUIManager.Instance.StartWalkingShake();
            _stepTimer -= Time.fixedDeltaTime;
            if (_stepTimer <= 0f)
            {
                PlayFootstep();
                _stepTimer = stepInterval;
            }
        }
        else
        {
            GlobalPlayerUIManager.Instance.StopWalkingShake();
            // Reset timer when stopping so next step plays immediately when moving starts
            _stepTimer = stepInterval;
        }
    }

    public void Update()
    {
        var camForwardHorizontal = getCameraForward();
        Debug.DrawRay(transform.position, lookCameraTransform.forward * 10, Color.red);
        Debug.DrawRay(transform.position, transform.forward * 10, Color.blue);
        Debug.DrawRay(transform.position, camForwardHorizontal * 10, Color.green);
    }

    public void PlayFootstep()
    {
        stepSfx.Play();
    }

    public void SetMoveAction(InputAction moveAction)
    { _moveAction = moveAction; }

    public void SetLookAction(InputAction lookAction)
    { _lookAction = lookAction; }
    
    /// <returns>The horizontal component of the camera's forward look</returns>
    private Vector3 getCameraForward()
    {
        Vector3 camForwardHorizontal = new Vector3(lookCameraTransform.forward.x, 0, lookCameraTransform.forward.z);
        camForwardHorizontal.Normalize();
        return camForwardHorizontal;
    }
}
