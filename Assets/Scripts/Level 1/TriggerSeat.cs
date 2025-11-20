using UnityEngine;

public class TriggerSeat : MonoBehaviour
{
    public NovaLevel1Manager novaLevel1Manager;
    public CharacterController robotCharController;
    public RobotMovement robotMovement;
    public Transform robot;
    public SceneExitDoor sceneExitDoor;

    private bool triggered = false;
    private bool playerInside = false;

    // Where the seat is to place the robot in
    private Vector3 seatVector = new Vector3(253.3f, 18.1f, 60.1f);
    // Where the robot's original position is
    private Vector3 robotPositionBeforeSit;

    // trigger level start at trigger enter
    private void OnTriggerEnter(Collider other)
    {
        if (playerInside) return;
        if (other != null && other.CompareTag("Robot"))
        {
            if (!triggered) novaLevel1Manager.PlayLevelRoutine();
            robotMovement.disable = true;
            SeatRobot();
            triggered = true;
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.CompareTag("Robot"))
        {
            playerInside = false;
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
        // enable the exit door collier
        sceneExitDoor.enabled = true;
    }

    public bool PlayerInsideSeat()
    {
        return playerInside;
    }
}
