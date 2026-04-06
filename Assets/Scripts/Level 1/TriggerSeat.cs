using UnityEngine;

public class TriggerSeat : MonoBehaviour
{
    public NovaLevel1Manager novaLevel1Manager;
    public CharacterController robotCharController;
    public RobotMovement robotMovement;
    public Transform robot;

    private bool triggered = false;
    private bool playerSitting = false;

    // Where the seat is to place the robot in
    private Vector3 seatVector = new Vector3(253.3f, 18.1f, 60.1f);
    // Where the robot's original position is
    private Vector3 robotPositionBeforeSit;

    private bool _isSitting;

    private float _sitDownDelayCounter = 0f;
    [SerializeField] private float SitBackDownDelay = 2f;

    private void Awake()
    {
        _sitDownDelayCounter = 0f;
    }

    private void Update()
    {
        if (_sitDownDelayCounter > 0f) _sitDownDelayCounter -= Time.deltaTime;
    }
    
    // trigger level start at trigger enter
    private void OnTriggerEnter(Collider other)
    {
        if (playerSitting) return;
        if (_sitDownDelayCounter <= 0f && other != null && other.CompareTag("Robot"))
        {
            if (!triggered) novaLevel1Manager.PlayLevelRoutine();
            robotMovement.disable = true;
            SeatRobot();
            triggered = true;
            playerSitting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.CompareTag("Robot"))
        {
            // Add a small delay to sitting back down to prevent sitting back down too easily
            _sitDownDelayCounter = SitBackDownDelay;
            playerSitting = false;
            BoxCollider collider = GetComponent<BoxCollider>();
            collider.enabled = true;
        }
    }

    public void SeatRobot()
    {
        robotCharController.enabled = false;
        robotPositionBeforeSit = robot.position;
        robot.position = seatVector;
        robot.rotation = new Quaternion(0, 180, 0, 0);
        GlobalPlayerUIManager.Instance.StopWalkingShake();
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.enabled = false;
    }

    public void StandRobot()
    {
        robot.position = robotPositionBeforeSit;
        robotMovement.disable = false;
        robotCharController.enabled = true;
    }

    public bool PlayerInsideSeat()
    {
        return playerSitting;
    }
}
