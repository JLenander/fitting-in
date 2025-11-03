using UnityEngine;

public class Drink : MonoBehaviour
{
    private Tray tray;
    private bool hasFallen;

    public void Init(Tray trayRef)
    {
        tray = trayRef;
        hasFallen = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasFallen) return; // prevent multiple triggers
        
        // didnt want to add an extra tag for floor so just checking layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasFallen = true;
            tray.OnCupFell();
        }
    }
}
