using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LeftArmUIHandler : TerminalUIHandler
{
    public static LeftArmUIHandler Instance;
    public void Awake()
    {
        Instance = this;
    }
}
