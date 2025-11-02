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

    private Vector3 seatVector = new Vector3(253.3f, 18.1f, 60.1f);

    // only enable collider if we found two hands
    private int handCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // trigger level start at trigger enter
    private void OnTriggerEnter(Collider other)
    {
        if (playerInside) return;
        if (other != null && other.CompareTag("Hand"))
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
        if (other != null && other.CompareTag("Hand"))
        {
            handCount++;
            Debug.Log(handCount);
            if (handCount == 2)
            {
                playerInside = false;
                BoxCollider collider = GetComponent<BoxCollider>();
                collider.enabled = true;
                handCount = 0;
            }
        }
    }

    public void SeatRobot()
    {
        robotCharController.enabled = false;
        robot.position = seatVector;
        robot.rotation = new Quaternion(0, 180, 0, 0);
        GlobalPlayerUIManager.Instance.StopWalkingShake();
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.enabled = false;
        handCount = 0;
    }

    public void StandRobot()
    {
        robotCharController.enabled = true;
        robotMovement.disable = false;
        // enable the exit door collier
        sceneExitDoor.enabled = true;
        handCount = 0;
    }

    public bool PlayerInsideSeat()
    {
        return playerInside;
    }
}
