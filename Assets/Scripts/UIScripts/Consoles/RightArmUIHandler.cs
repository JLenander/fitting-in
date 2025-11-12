using UnityEngine;

public class RightArmUIHandler : TerminalUIHandler
{
    public static RightArmUIHandler Instance;
    public void Awake()
    {
        Instance = this;
    }
}
