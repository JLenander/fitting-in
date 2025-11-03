using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

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
    public GameObject blinkOverlay; // a black overlap for camera
    public AudioSource audioSource;

    private bool timerIsRunning;
    [FormerlySerializedAs("warning")] public bool isPixelatingPhase = false;
    private bool isFullyPixelated = false;
    private bool isInFireBuffer = false;
    private bool fireOver = false;

    private void Start()
    {
        DisableOutline();
        timeToNextBlink = timeBetweenBlinks;
        pressAnimationCountdown = eyeAnimationTime;
        fireBufferCountdown = fireBufferTime;
        timerIsRunning = true;

        blinkOverlay.SetActive(false);
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
            if (pressAnimationCountdown > 0)
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

                    hoverMessage = "[Extinguish fire!]";
                    msgColour = new Color(1, 0, 0, 1);
                    outlineColour = new Color(1, 0, 0, 1);
                }

                if (isInFireBuffer)
                {
                    fireBufferCountdown -= Time.deltaTime;

                    if (fireBufferCountdown <= 0)
                    {
                        // Eye is on fire now.
                        isInFireBuffer = false;
                        FireManager.Instance.StartFireArea("eye");
                    }
                }
            }
        }

    }

    public override void Interact(GameObject player)
    {
        PlayerInteract playerInteract = player.GetComponent<PlayerInteract>();
        if (!isFullyPixelated)
            ResetTimers(); // only allow lever to reset timer if not at critical

        if (fireOver)
        {
            ResetTimers(); // if fire is putout 
        }

        playerInteract.LeaveCurrInteractable();

        if (audioSource != null)
            audioSource.Play();

        if (blinkOverlay != null)
            StartCoroutine(BlinkRoutine());
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

        hoverMessage = "Blink";
        msgColour = new Color(1, 1, 1, 1);
        outlineColour = new Color(1, 1, 1, 1);

        fireOver = false;
    }


    private IEnumerator BlinkRoutine()
    {
        blinkOverlay.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        blinkOverlay.SetActive(false);
    }

    public void FirePutOut()
    {
        fireOver = true;
    }
}
