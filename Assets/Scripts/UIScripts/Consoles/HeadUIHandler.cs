using UnityEngine;

public class HeadUIHandler : TerminalUIHandler
{
    public static HeadUIHandler Instance;
    public void Awake()
    {
        Instance = this;
    }
}
