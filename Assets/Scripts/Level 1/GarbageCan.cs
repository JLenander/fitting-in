using UnityEngine;

public class GarbageCan : MonoBehaviour
{
    public Outline outline;
    private void Start()
    {
        DisableOutline();
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }

    public void EnableOutline()
    {
        outline.enabled = true;
    }
}
