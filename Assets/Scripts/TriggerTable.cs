using UnityEngine;

public class TriggerTable : MonoBehaviour
{
    bool triggered = false;
    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other != null && other.CompareTag("Hand"))
        {
            // Level0TaskManager.CompleteTaskGoToPhone();
            // Level0TaskManager.StartTaskPickupPhone();
            triggered = true;
        }
    }
}
