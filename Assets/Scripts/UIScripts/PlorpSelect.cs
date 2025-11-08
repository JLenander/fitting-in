using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlorpSelect : MonoBehaviour
{
    public Animator plorpAnimator;
    public Outline outline;
    public Transform modelTransform;
    private InputAction _lookAction;
    // private float xRotation = 0f;
    private float yRotation = 0f;
    private float rotationSpeed = 100f;

    public void Initialize(PlayerInput playerInput)
    {
        _lookAction = playerInput.actions.FindAction("Rotate");
    }

    public void Start()
    {
        // essential to start with the correct rotation
        yRotation = modelTransform.localEulerAngles.y;
    }

    public void changeColor(Color color)
    {
        outline.OutlineColor = color;
    }

    public void ready()
    {
        plorpAnimator.SetBool("isReady", true);
    }

    public void unready()
    {
        plorpAnimator.SetBool("isReady", false);
    }
  
    private void Update()
    {
        Vector2 lookValue = _lookAction.ReadValue<Vector2>();
        yRotation -= lookValue.x * rotationSpeed * Time.deltaTime; // horizontal (yaw)
        // x rotation is at feet, disable unless can make pivot point at center
        // xRotation -= lookValue.y * rotationSpeed * Time.deltaTime;
        // xRotation = Mathf.Clamp(xRotation, -10f, 10f);
        modelTransform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }
}