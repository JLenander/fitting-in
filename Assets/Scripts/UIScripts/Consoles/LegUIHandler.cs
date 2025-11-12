using UnityEngine;

public class LegUIHandler : TerminalUIHandler
{
    public static LegUIHandler Instance;
    public void Awake()
    {
        Instance = this;
    }
}
