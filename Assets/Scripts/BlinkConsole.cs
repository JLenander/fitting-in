using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using FMODUnity;

public class BlinkConsole : Interactable
{
    [FormerlySerializedAs("totalTimeBlink")] public float timeBetweenBlinks = 50f;
    // Time from the start of blinking to fully pixelated eyes
    [FormerlySerializedAs("totalTimePress")] public float eyeAnimationTime = 10f;
    // Buffer time between fully pixelated and when fires start
    public float fireBufferTime = 5f;

    public float timeToNextBlink;
    public float pressAnimationCountdown;
    public float fireBufferCountdown;
    public HeadConsole headConsole;
    public AudioSource audioSource;
    public StudioEventEmitter enterSfx;

    private bool timerIsRunning;
    [FormerlySerializedAs("warning")] public bool isPixelatingPhase = false;
    private bool isFullyPixelated = false;
    private bool isInFireBuffer = false;
    private bool fireOver = false;
    private bool _canInteract = false;

    private void Start()
    {
        DisableOutline();
        timeToNextBlink = timeBetweenBlinks;
        pressAnimationCountdown = eyeAnimationTime;
        fireBufferCountdown = fireBufferTime;
        timerIsRunning = false;
        _canInteract = true;

        PopUpUIHandler.Instance.HideBlinkPopUp();
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeToNextBlink > 0)
            {
                timeToNextBlink -= Time.deltaTime;
            }
            else
            {
                if (!isPixelatingPhase)// so it doesnt trigger every frame
                {
                    isPixelatingPhase = true;
                    timeToNextBlink = 0;
                    timerIsRunning = false;
                    timeToNextBlink = timeBetweenBlinks;
                    outlineColour = Color.red;

                    // start camera pixelate
                    GlobalPlayerUIManager.Instance.PixelateView(pressAnimationCountdown);

                    // player notification
                    PopUpUIHandler.Instance.ShowBlinkPopUp();
                }

                EnableOutline();
            }
        }
        else
        {
            if (pressAnimationCountdown > 0 && timerIsRunning)
            {
                pressAnimationCountdown -= Time.deltaTime;
            }
            else
            {
                if (!isFullyPixelated) // so it doesnt trigger every frame
                {
                    isFullyPixelated = true;

                    // disable head console
                    headConsole.DisableInteract();

                    // start fire buffer countdown before the fire starts to give players time to blink
                    isInFireBuffer = true;
                }

                if (isInFireBuffer && timerIsRunning)
                {
                    fireBufferCountdown -= Time.deltaTime;

                    if (fireBufferCountdown <= 0)
                    {
                        // Eye is on fire now.
                        isInFireBuffer = false;
                        FireManager.Instance.StartFireArea("eye");

                        hoverMessage = "[Extinguish fire!]";
                        msgColour = new Color(1, 0, 0, 1);
                        outlineColour = new Color(1, 0, 0, 1);

                        // if level 1 play sus animation
                        // play blink sus dialogue
                    }
                }
            }
        }

    }

    public override void Interact(GameObject player)
    {
        if (!_canInteract) return;

        PlayerInteract playerInteract = player.GetComponent<PlayerInteract>();
        if (!isFullyPixelated || (isFullyPixelated && isInFireBuffer))
            ResetTimers(); // only allow lever to reset timer if not at critical

        if (fireOver)
        {
            ResetTimers(); // if fire is putout 
        }

        playerInteract.LeaveCurrInteractable();

        if (enterSfx != null)
            enterSfx.Play();

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.blinked = true;
        }
    }

    public void ResetTimers()
    {
        DisableOutline();
        timerIsRunning = true;
        timeToNextBlink = timeBetweenBlinks;
        pressAnimationCountdown = eyeAnimationTime;
        fireBufferCountdown = fireBufferTime;
        outlineColour = Color.white;
        //EnableOutline();
        Debug.Log("timers reset");

        headConsole.EnableInteract(); // reenable head
        isFullyPixelated = false; // remove flags
        isPixelatingPhase = false; // remove flags
        isInFireBuffer = false; // remove flags
        GlobalPlayerUIManager.Instance.DisablePixelate(); // undo pixelate
        PopUpUIHandler.Instance.HideBlinkPopUp();

        fireOver = false;

        hoverMessage = "Blink";
        msgColour = new Color(1, 1, 1, 1);
        outlineColour = new Color(1, 1, 1, 1);
    }

    public void FirePutOut()
    {
        fireOver = true;

        hoverMessage = "Blink";
        msgColour = new Color(1, 1, 1, 1);
    }

    public void DisableInteract()
    {
        _canInteract = false;
        timerIsRunning = false;
        hoverMessage = "[CONTROL DISABLED]";
        msgColour = new Color(1, 0, 0, 1);
        outlineColour = new Color(1, 0, 0, 1);
    }

    public void EnableInteract()
    {
        _canInteract = true;
        ResetTimers();
        timerIsRunning = true;
        hoverMessage = "Blink";
        msgColour = new Color(1, 1, 1, 1);
        outlineColour = new Color(1, 1, 1, 1);
    }

    public void SetTimerIsRunning(bool isRunning)
    {
        timerIsRunning = isRunning;
    }
}
