using UnityEngine;
using UnityEngine.UIElements;

public class RightArmUIHandler : TerminalUIHandler
{
    
    public static RightArmUIHandler Instance;
    public void Awake()
    {
        Instance = this;
    }
}
