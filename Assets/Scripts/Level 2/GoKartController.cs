using UnityEngine;

// public methods that let you control the gokart
public class GoKartController : MonoBehaviour
{
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float maxSpeed = 25f;

    [SerializeField] private float steeringPower = 8f;
    [SerializeField] private float extraGravity = 30f;
    [SerializeField] private float stabilizationForce = 15f;
    [SerializeField] private float groundCheckDistance = 2f;
    private float throttleInput;
    private float steeringInput;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lower center of mass
        rb.centerOfMass = new Vector3(0f, -1f, 0f);
    }
    void FixedUpdate()
    {
        // it flies otherwise
        bool grounded = IsGrounded();

        // normal gravity always
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        // extra gravity ONLY when airborne
        if (!grounded)
        {
            rb.AddForce(Vector3.down * extraGravity * 5f, ForceMode.Acceleration);
        }

        HandleMovement();
        HandleSteering();
        HandleStabilization();
    }
    public void ChangeSpeed(float input)
    {
        // input should usually be between -1 and 1
        throttleInput = input;
    }

    public void ChangeDirection(float input)
    {
        // input should usually be between -1 and 1
        steeringInput = input;
    }

    private void HandleMovement()
    {
        // Forward force
        rb.AddForce(transform.forward * throttleInput * acceleration,
            ForceMode.Acceleration);

        // Clamp max speed
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVelocity.magnitude > maxSpeed)
        {
            flatVelocity = flatVelocity.normalized * maxSpeed;

            rb.linearVelocity = new Vector3(
                flatVelocity.x,
                rb.linearVelocity.y,
                flatVelocity.z
            );
        }
    }

    private void HandleSteering()
    {
        float speedFactor = Mathf.Max(rb.linearVelocity.magnitude / maxSpeed, 0.3f);

        // Apply steering torque
        rb.AddTorque(
            transform.up * steeringInput * steeringPower * speedFactor,
            ForceMode.Acceleration
        );

        // Stop spinning when not steering
        if (Mathf.Abs(steeringInput) < 0.01f)
        {
            Vector3 angularVel = rb.angularVelocity;

            angularVel.y = Mathf.Lerp(
                angularVel.y,
                0f,
                8f * Time.fixedDeltaTime
            );

            rb.angularVelocity = angularVel;
        }
    }

    private void HandleStabilization()
    {
        Vector3 targetUp = Vector3.up;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance))
            targetUp = hit.normal;

        Vector3 torque = Vector3.Cross(transform.up, targetUp);
        rb.AddTorque(torque * stabilizationForce, ForceMode.Acceleration);
    }

    private bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }
}
