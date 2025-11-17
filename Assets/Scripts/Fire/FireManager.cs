using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FireManager : MonoBehaviour
{
    public static FireManager Instance;
    public FireArea eyeFireArea;
    public FireArea leftArmFireArea;
    public FireArea rightArmFireArea;
    public FireArea beginFireArea;
    public FireArea legFireArea;
    public FireArea lowerFireArea;
    public BlinkConsole blinkConsole;
    public HandConsole leftArmConsole;
    public HandConsole rightArmConsole;
    public HipConsole hipConsole;
    public StudioEventEmitter fireSfx;
    public float isFire = 0f;
    public StudioEventEmitter mus;
    private EventInstance instance;

    private Dictionary<string, FireArea> fireAreas;

    private void Awake()
    {
        // Create the mapping once
        fireAreas = new Dictionary<string, FireArea>
        {
            { "eye", eyeFireArea },
            { "leftArm", leftArmFireArea },
            { "rightArm", rightArmFireArea },
            { "leg", legFireArea },
            { "lower", lowerFireArea},
            { "begin", beginFireArea }
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

        fireSfx.EventInstance.setVolume(3f);
        instance = mus.EventInstance;
    }

    public void StartFireArea(string name)
    {
        if (fireAreas.TryGetValue(name, out FireArea area))
        {
            area.EnableFires();
            if (!fireSfx.IsPlaying())
            {
                Debug.Log($"Fire sound start");
                fireSfx.Play();
                isFire = 1f;
                mus.SetParameter("Emergency", isFire);
            }
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
        else if (name == "lower")
        {
            leftArmConsole.EnableInteract();
            rightArmConsole.EnableInteract();
            Level1TaskManager.CompleteTaskPutOutFires();
            Debug.Log("fires out");
        }

        else if (name == "begin")
        {
            TutorialManager.Instance.beginFire = true;
        }
        else
        {
            Debug.Log($"Can't disable fire area {name} that doesn't exist");
        }

        // If any fire area still active, don't stop sfx
        foreach (var fa in fireAreas)
        {
            var area = fa.Value;
            if (area != null && area.IsActive)
            {
                return;
            }
        }

        Debug.Log($"Fire sound stop");
        fireSfx.Stop();
        isFire = 0f;
        mus.SetParameter("Emergency", isFire);
    }
}
