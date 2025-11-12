using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PopUpUIHandler : MonoBehaviour
{
    public static PopUpUIHandler Instance;
    public float blinkInterval = 0.5f;
    
    private Coroutine blinkRoutine;
    private ISplitscreenUIHandler _splitScreenUIHandler;

    private void Awake()
    {
        // Only allow one level manager
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        
        _splitScreenUIHandler = FindAnyObjectByType<SplitscreenUIHandler>();
    }
    
    void Start()
    {
        HideNewTaskPopUp();
        HideBlinkPopUp();
    }

    // show new task container popup
    public void ShowNewTaskPopUp()
    {
        _splitScreenUIHandler.ShowNewTaskPopUp();
    }

    // hide new task popup
    public void HideNewTaskPopUp()
    {
        _splitScreenUIHandler.HideNewTaskPopUp();
    }

    public void ShowBlinkPopUp()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            _splitScreenUIHandler.ShowBlinkPopUp();
            yield return new WaitForSeconds(blinkInterval);
            _splitScreenUIHandler.HideBlinkPopUp();
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    public void HideBlinkPopUp()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        _splitScreenUIHandler.HideBlinkPopUp();
    }
}
