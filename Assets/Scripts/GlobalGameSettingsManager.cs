using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Global settings manager to handle persistent game settings like audio settings.
/// </summary>
public class GlobalGameSettingsManager : MonoBehaviour
{
    public static GlobalGameSettingsManager Instance;
    
    private VCA _masterAudio;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(this);
        
        // Set framerate cap
        Application.targetFrameRate = 120;
        
        _masterAudio = RuntimeManager.GetVCA("vca:/Master");
    }

    public float GetMasterVolume()
    {
        _masterAudio.getVolume(out float volume);
        return volume;
    }
    
    public void SetMasterVolume(float value)
    {
        _masterAudio.setVolume(value);
    }

    public bool GetFullscreen()
    {
        return Screen.fullScreen;
    }
    
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
