using System.Collections;
using UnityEngine;

public class BlinkConsole : Interactable
{
    public float totalTimeBlink = 50f;
    public float totalTimePress = 10f;

    public float timeToNextBlink;
    public float pressCountdown;
    public HeadConsole headConsole;
    public GameObject blinkOverlay; // a black overlap for camera
    public AudioSource audioSource;

    private bool timerIsRunning;
    public bool warning = false;
    private bool danger = false;

    private bool fireOver = false;

    private void Start()
    {
        DisableOutline();
        timeToNextBlink = totalTimeBlink;
        pressCountdown = totalTimePress;
        timerIsRunning = true;

        blinkOverlay.SetActive(false);
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
                if (!warning)// so it doesnt trigger every frame
                {
                    warning = true;
                    timeToNextBlink = 0;
                    timerIsRunning = false;
                    timeToNextBlink = totalTimeBlink;
                    outlineColour = Color.red;

                    // start camera fade
                    GlobalPlayerUIManager.Instance.PixelateView(pressCountdown);

                    // player notification
                    PopUpUIHandler.Instance.ShowBlinkPopUp();
                }

                EnableOutline();
            }
        }
        else
        {
            if (pressCountdown > 0)
            {
                pressCountdown -= Time.deltaTime;
            }
            else
            {
                if (!danger) // so it doesnt trigger every frame
                {
                    danger = true;

                    // disable head console
                    headConsole.DisableInteract();

                    // enable fire
                    FireManager.Instance.StartFireArea("eye");

                    hoverMessage = "[Extinguish fire!]";
                    msgColour = new Color(1, 0, 0, 1);
                    outlineColour = new Color(1, 0, 0, 1);
                }
            }
        }

    }

    public override void Interact(GameObject player)
    {
        PlayerInteract playerInteract = player.GetComponent<PlayerInteract>();
        if (!danger)
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
        timeToNextBlink = totalTimeBlink;
        pressCountdown = totalTimePress;
        outlineColour = Color.white;
        //EnableOutline();
        Debug.Log("timers reset");

        headConsole.EnableInteract(); // reenable head
        danger = false; // remove flags
        warning = false; // remove flags
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
