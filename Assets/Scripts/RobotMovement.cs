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

    void Start()
    {
        var input = GetComponent<PlayerInput>();
        _robotCharacterController = GetComponentInChildren<CharacterController>();

        if (_robotCharacterController == null)
            Debug.LogError("No CharacterController found");
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

        float moveInput = (leftInput + rightInput) / 2f;
        Vector3 moveDir = transform.forward * moveInput + _robotVelocity;
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

    public void PlayFootstep()
    {
        stepSfx.Play();
    }

    public void SetMoveAction(InputAction moveAction)
    { _moveAction = moveAction; }

    public void SetLookAction(InputAction lookAction)
    { _lookAction = lookAction; }

}
