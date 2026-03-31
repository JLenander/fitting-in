using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlorpSelect : MonoBehaviour
{
    public Animator plorpAnimator;
    public Outline outline;
    public Transform modelTransform;
    private InputAction _lookAction;
    // private float xRotation;
    private float _yRotation;
    private readonly float _rotationSpeed = 300f;
    private bool _isReady = false;
    // Used in SmoothDampAngle
    private float _smoothRotateVelocity = 0f;
    
    //
    private const float LookAtScreenRotation = 180f;

    public void Initialize(PlayerInput playerInput)
    {
        _lookAction = playerInput.actions.FindAction("Rotate");
    }

    public void Start()
    {
        // essential to start with the correct rotation
        _yRotation = modelTransform.localEulerAngles.y;
    }

    public void ChangeColor(Color color)
    {
        outline.OutlineColor = color;
    }

    public void Ready()
    {
        plorpAnimator.SetBool("isReady", true);
        _isReady = true;
    }

    public void Unready()
    {
        plorpAnimator.SetBool("isReady", false);
        _isReady = false;
    }
  
    private void Update()
    {
        if (_isReady)
        {
            // Smoothly move look back towards the front and keep it locked there if ready.
            _yRotation = Mathf.SmoothDampAngle(_yRotation, LookAtScreenRotation, ref _smoothRotateVelocity, 0.2f);
        }
        else
        {
            // Allow player to rotate the character model when not ready
            Vector2 lookValue = _lookAction.ReadValue<Vector2>();
            _yRotation -= lookValue.x * _rotationSpeed * Time.deltaTime; // horizontal (yaw)
            // x rotation is at feet, disable unless can make pivot point at center
            // xRotation -= lookValue.y * rotationSpeed * Time.deltaTime;
            // xRotation = Mathf.Clamp(xRotation, -10f, 10f);
        }

        modelTransform.localRotation = Quaternion.Euler(0f, _yRotation, 0f);
    }
}