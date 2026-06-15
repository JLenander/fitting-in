using UnityEngine;
using UnityEngine.InputSystem;

public class GoKartTriggerSeat : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Robot References")]
    [SerializeField] private CharacterController robotCharController;
    [SerializeField] private RobotMovement robotMovement;
    [SerializeField] private Transform robot;

    [Header("GoKart References")]
    [SerializeField] private GoKartController goKartController;
    [SerializeField] private Transform kartSeatPoint;
    [SerializeField] private HandMovement leftArmMovement;
    [SerializeField] private HandMovement rightArmMovement;
    [SerializeField] private Transform steeringWheel;
    [SerializeField] private Transform accelerationLever;

    [Header("Input Tuning")]
    [SerializeField] private float armInputScale = 1f;
    [SerializeField] private float inputDeadzone = 0.1f;

    [Header("Control Visuals")]
    [SerializeField] private float steeringWheelMaxZ = 38.69f;
    [SerializeField] private float accelerationLeverMaxX = 80f;

    [Header("Trigger Settings")]
    [SerializeField] private float sitBackDownDelay = 2f;

    private bool playerDriving;
    private float sitDownDelayCounter;

    private Vector3 robotPositionBeforeDrive;
    private Quaternion robotRotationBeforeDrive;
    private Transform robotParentBeforeDrive;

    private Vector3 robotEntryPosition;
    private Quaternion robotEntryRotation;

    private Vector3 steeringWheelBaseEuler;
    private Vector3 accelerationLeverBaseEuler;

    private void Awake()
    {
        sitDownDelayCounter = 0f;

        if (enableDebugLogs)
        {
            Debug.Log($"[{name}] GoKartTriggerSeat Awake. TriggerCollider={GetComponent<Collider>()?.GetType().Name ?? "NULL"}");
        }
    }

    private void Start()
    {
        if (steeringWheel != null)
        {
            steeringWheelBaseEuler = steeringWheel.localEulerAngles;
        }

        if (accelerationLever != null)
        {
            accelerationLeverBaseEuler = accelerationLever.localEulerAngles;
        }

        if (!enableDebugLogs)
        {
            return;
        }

        Collider triggerCollider = GetComponent<Collider>();
        Debug.Log(
            $"[{name}] GoKartTriggerSeat Start refs: " +
            $"robot={(robot != null ? robot.name : "NULL")}, " +
            $"charController={(robotCharController != null ? robotCharController.name : "NULL")}, " +
            $"robotMovement={(robotMovement != null ? robotMovement.name : "NULL")}, " +
            $"kartSeatPoint={(kartSeatPoint != null ? kartSeatPoint.name : "NULL")}, " +
            $"goKartController={(goKartController != null ? goKartController.name : "NULL")}, " +
            $"leftArm={(leftArmMovement != null ? leftArmMovement.name : "NULL")}, " +
            $"rightArm={(rightArmMovement != null ? rightArmMovement.name : "NULL")}, " +
            $"triggerCollider={(triggerCollider != null ? triggerCollider.name : "NULL")}, " +
            $"isTrigger={(triggerCollider != null && triggerCollider.isTrigger)}"
        );

        if (triggerCollider == null)
        {
            Debug.LogWarning($"[{name}] No collider found on GoKartTriggerSeat object. OnTriggerEnter will never fire.");
        }
        else if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[{name}] Collider exists but isTrigger is FALSE. OnTriggerEnter will not behave as expected.");
        }
    }

    private void Update()
    {
        if (sitDownDelayCounter > 0f)
        {
            sitDownDelayCounter -= Time.deltaTime;
        }

        if (!playerDriving || goKartController == null)
        {
            return;
        }

        float steeringInput = ReadLeftArmSteeringInput();
        float throttleInput = ReadRightArmThrottleInput();

        goKartController.ChangeDirection(steeringInput);
        goKartController.ChangeSpeed(throttleInput);
        ApplyControlVisuals(steeringInput, throttleInput);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs)
        {
            Debug.Log(
                $"[{name}] OnTriggerEnter by {(other != null ? other.name : "NULL")}. " +
                $"tag={(other != null ? other.tag : "NULL")}, " +
                $"playerDriving={playerDriving}, sitDownDelayCounter={sitDownDelayCounter:F2}"
            );
        }

        if (playerDriving || sitDownDelayCounter > 0f)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[{name}] Ignored trigger enter. playerDriving={playerDriving}, sitDownDelayCounter={sitDownDelayCounter:F2}");
            }
            return;
        }

        if (IsRobotCollider(other))
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[{name}] Robot collider confirmed. Calling SeatRobot().");
            }
            SeatRobot();
            playerDriving = true;
            if (enableDebugLogs)
            {
                Debug.Log($"[{name}] Seat complete. playerDriving set TRUE.");
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[{name}] Trigger enter ignored because collider does not belong to assigned robot references.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsRobotCollider(other))
        {
            sitDownDelayCounter = sitBackDownDelay;
        }
    }

    public void SeatRobot()
    {
        if (robot == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[{name}] SeatRobot failed: robot reference is NULL.");
            }
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[{name}] SeatRobot start. robot world pos={robot.position}, rot={robot.rotation.eulerAngles}");
        }

        robotEntryPosition = robot.position;
        robotEntryRotation = robot.rotation;

        robotPositionBeforeDrive = robot.position;
        robotRotationBeforeDrive = robot.rotation;
        robotParentBeforeDrive = robot.parent;

        if (robotMovement != null)
        {
            robotMovement.disable = true;
        }

        if (leftArmMovement != null)
        {
            leftArmMovement.FreezeWristPosition(true);
        }

        if (rightArmMovement != null)
        {
            rightArmMovement.FreezeWristPosition(true);
        }

        if (robotCharController != null)
        {
            robotCharController.enabled = false;
        }

        if (kartSeatPoint != null)
        {
            robot.SetParent(kartSeatPoint);
            robot.localPosition = Vector3.zero;
            robot.localRotation = Quaternion.identity;
            if (enableDebugLogs)
            {
                Debug.Log($"[{name}] Robot parented to kartSeatPoint '{kartSeatPoint.name}'. New world pos={robot.position}");
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogError($"[{name}] SeatRobot failed: kartSeatPoint is NULL, so teleport did not run.");
        }

        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            if (enableDebugLogs)
            {
                Debug.Log($"[{name}] Trigger collider disabled after seating.");
            }
        }

        if (GlobalPlayerUIManager.Instance != null)
        {
            GlobalPlayerUIManager.Instance.StopWalkingShake();
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"[{name}] GlobalPlayerUIManager.Instance is NULL.");
        }
    }

    public void ExitRobotToEntry()
    {
        if (!playerDriving || robot == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[{name}] ExitRobotToEntry ignored. playerDriving={playerDriving}, robotNull={robot == null}");
            }
            return;
        }

        if (goKartController != null)
        {
            goKartController.ChangeDirection(0f);
            goKartController.ChangeSpeed(0f);
        }

        robot.SetParent(robotParentBeforeDrive);
        robot.position = robotEntryPosition;
        robot.rotation = robotEntryRotation;

        if (enableDebugLogs)
        {
            Debug.Log($"[{name}] ExitRobotToEntry complete. Restored to entry pos={robotEntryPosition}");
        }

        if (robotMovement != null)
        {
            robotMovement.disable = false;
        }

        if (leftArmMovement != null)
        {
            leftArmMovement.FreezeWristPosition(false);
        }

        if (rightArmMovement != null)
        {
            rightArmMovement.FreezeWristPosition(false);
        }

        if (robotCharController != null)
        {
            robotCharController.enabled = true;
        }

        playerDriving = false;
        sitDownDelayCounter = sitBackDownDelay;

        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        ApplyControlVisuals(0f, 0f);
    }

    public bool PlayerInsideSeat()
    {
        return playerDriving;
    }

    private float ReadLeftArmSteeringInput()
    {
        if (leftArmMovement == null)
        {
            return 0f;
        }

        // Prefer raw stick input for immediate response (GoKartInputTester feel).
        float value = ReadRawArmAxis(leftArmMovement, useHorizontal: true);
        if (Mathf.Approximately(value, 0f))
        {
            // Fallback when arm/player input context is unavailable.
            value = leftArmMovement.movement.x;
        }

        value = Mathf.Clamp(value * armInputScale, -1f, 1f);
        if (Mathf.Abs(value) < inputDeadzone)
        {
            return 0f;
        }

        return value;
    }

    private float ReadRightArmThrottleInput()
    {
        if (rightArmMovement == null)
        {
            return 0f;
        }

        // Prefer raw stick input for immediate response (GoKartInputTester feel).
        float value = ReadRawArmAxis(rightArmMovement, useHorizontal: false);
        if (Mathf.Approximately(value, 0f))
        {
            // Fallback when arm/player input context is unavailable.
            value = rightArmMovement.movement.y;
        }

        value = Mathf.Clamp(value * armInputScale, -1f, 1f);
        if (Mathf.Abs(value) < inputDeadzone)
        {
            return 0f;
        }

        return value;
    }

    private float ReadRawArmAxis(HandMovement armMovement, bool useHorizontal)
    {
        GameObject currPlayer = armMovement.GetCurrPlayer();
        if (currPlayer == null)
        {
            return 0f;
        }

        PlayerInput playerInput = currPlayer.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            return 0f;
        }

        InputAction moveAction = InputActionMapper.GetPlayerMoveAction(playerInput);
        if (moveAction == null)
        {
            return 0f;
        }

        Vector2 move = moveAction.ReadValue<Vector2>();
        return useHorizontal ? move.x : move.y;
    }

    private void ApplyControlVisuals(float steeringInput, float throttleInput)
    {
        if (steeringWheel != null)
        {
            float steeringZ = steeringInput * steeringWheelMaxZ;
            steeringWheel.localRotation = Quaternion.Euler(
                steeringWheelBaseEuler.x,
                steeringWheelBaseEuler.y,
                steeringWheelBaseEuler.z + steeringZ
            );
        }

        if (accelerationLever != null)
        {
            float leverX = throttleInput * accelerationLeverMaxX;
            accelerationLever.localRotation = Quaternion.Euler(
                accelerationLeverBaseEuler.x + leverX,
                accelerationLeverBaseEuler.y,
                accelerationLeverBaseEuler.z
            );
        }
    }

    private bool IsRobotCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        // Keep tag support for scenes that already use the Robot tag.
        if (other.CompareTag("Robot"))
        {
            return true;
        }

        Transform otherTransform = other.transform;

        // Prefer explicit scene references over tag assumptions.
        if (robot != null && (otherTransform == robot || otherTransform.IsChildOf(robot) || robot.IsChildOf(otherTransform)))
        {
            return true;
        }

        if (robotCharController != null)
        {
            Transform ccTransform = robotCharController.transform;
            if (otherTransform == ccTransform || otherTransform.IsChildOf(ccTransform) || ccTransform.IsChildOf(otherTransform))
            {
                return true;
            }
        }

        if (robotMovement != null)
        {
            Transform movementTransform = robotMovement.transform;
            if (otherTransform == movementTransform || otherTransform.IsChildOf(movementTransform) || movementTransform.IsChildOf(otherTransform))
            {
                return true;
            }
        }

        return false;
    }
}
