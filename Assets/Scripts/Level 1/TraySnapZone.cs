using UnityEngine;

public class TraySnapZone : MonoBehaviour
{
    public Transform snapPoint; // where tray should lock onto (e.g., top center of table)

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entering Table!!!");
        Tray tray = other.GetComponent<Tray>();
        if (tray != null && tray.IsTwoHanded()) // only snap if tray being held
        {
            tray.TrySnapToTable(snapPoint);
        }
    }
}