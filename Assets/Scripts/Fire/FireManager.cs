using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{
    public static FireManager Instance;
    public FireArea eyeFireArea;
    public FireArea leftArmFireArea;
    public FireArea rightArmFireArea;
    public FireArea legFireArea;
    public BlinkConsole blinkConsole;
    public HandConsole leftArmConsole;
    public HandConsole rightArmConsole;
    public HipConsole hipConsole;

    private Dictionary<string, FireArea> fireAreas;

    private void Awake()
    {
        // Create the mapping once
        fireAreas = new Dictionary<string, FireArea>
        {
            { "eye", eyeFireArea },
            { "leftArm", leftArmFireArea },
            { "rightArm", rightArmFireArea },
            { "leg", legFireArea }
        };

        // init singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartFireArea(string name)
    {
        if (fireAreas.TryGetValue(name, out FireArea area))
        {
            area.EnableFires();
        }
        else
        {
            Debug.Log($"No fire area found for name: {name}");
        }
    }

    public void StopFireArea(string name)
    {
        // based on fire name, fix something
        if (name == "eye")
        {
            blinkConsole.FirePutOut();
        }
        else if (name == "leftArm")
        {
            leftArmConsole.EnableInteract();
        }
        else if (name == "rightArm")
        {
            rightArmConsole.EnableInteract();
        }
        else if (name == "leg")
        {
            // TODO: reenable legs

        }
        else
        {
            Debug.Log($"Can't disable fire area {name} that doesn't exist");
        }
    }
}
