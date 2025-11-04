using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// used to control player UI in the splitscreen view, used to shake and dim their cameras
public class GlobalPlayerUIManager : MonoBehaviour
{
    public static GlobalPlayerUIManager Instance { get; private set; }
    private ISplitscreenUIHandler _splitscreenUIHandler;
    [SerializeField] private TMP_Text textPrefab; // prefab of prompt text
    [SerializeField] private List<TMP_Text> interactionText = new List<TMP_Text>(); // player texts
    [SerializeField] private DialogueSystem dialogueDisplay; // dialogues
    [SerializeField] private RectTransform canvas; // main canvas
    [SerializeField] private Image cameraDim; // image used to dim camera
    [SerializeField] private float dim = 0.796f; // dim amount, range 0 to 1
    [SerializeField] private RenderTexture outsideRenderTextureView;
    [SerializeField] private List<RenderTexture> playerRenderTextureView;
    [SerializeField] private float downScaleAmount = 0.8f; // range 0 to 1, 0.999 for highest
    [SerializeField] private int originalWidth;
    [SerializeField] private int originalHeight;

    private Coroutine _walkingShakeCoroutine;
    [SerializeField] private float walkShakeIntensity = 0.01f; // small offset
    [SerializeField] private float walkShakeDuration = 0.05f;   // how long each shake lasts
    [SerializeField] private float walkShakeInterval = 0.1f;     // time between shakes

    private List<PlayerData> playerData = new List<PlayerData>();
    private List<Vector3> playerCameraLocalPos = new List<Vector3>();

    private bool start = false;
    private Coroutine pixelateCoroutine;

    public void Awake()
    {
        // Setup singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        _splitscreenUIHandler = FindAnyObjectByType<SplitscreenUIHandler>();
        
        // Register scene change handler after loading the UI to start with 
        SceneManager.activeSceneChanged += UpdateUIForSceneChange;
        InitializeUI();
    }

    /// <summary>
    /// As a global player UI manager, update all player UI that needs to update on a scene change.
    /// For example, reset UI to initial states.
    /// </summary>
    private void UpdateUIForSceneChange(Scene oldScene, Scene newScene)
    {
        InitializeUI();
    }

    /// <summary>
    /// Initialize UI to defaults.
    /// </summary>
    private void InitializeUI()
    {
        _splitscreenUIHandler.HideOutsideCamera(0f);
        DisableDim();
        DisablePixelate();
        dialogueDisplay.gameObject.SetActive(false);
        PopUpUIHandler.Instance.HideBlinkPopUp();
        PopUpUIHandler.Instance.HideNewTaskPopUp();
    }

    // log players' cameras
    public void PassPlayers(PlayerData[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].Valid)
            {
                playerData.Add(players[i]);
                playerCameraLocalPos.Add(players[i].Input.camera.transform.localPosition);
                _splitscreenUIHandler.EnablePlayerOverlay(i);

                // assign render texture to camera
                players[i].Input.camera.targetTexture = playerRenderTextureView[i];
            }
        }

        start = true;
    }

    public void EnableInteractionText(int player, string content, Color msgColour, string buttonPath)
    {
        _splitscreenUIHandler.EnablePlayerInteractionText(player, content, msgColour, buttonPath);
    }

    public void DisableInteractionText(int player)
    {
        if (!start) return;
        _splitscreenUIHandler.DisablePlayerInteractionText(player);

    }

    public void EnableScreenGreyscale(int player)
    {
        _splitscreenUIHandler.EnablePlayerScreenGreyscale(player);
    }

    public void DisableScreenGreyscale(int player)
    {
        if (!start) return;
        _splitscreenUIHandler.DisablePlayerScreenGreyscale(player);
    }

    // fades image into view based on *time* seconds, used for blink terminal
    public void PixelateView(float time)
    {
        // StartCoroutine(FadeRoutine(time));
        if (pixelateCoroutine != null)
        {
            StopCoroutine(pixelateCoroutine);
            pixelateCoroutine = null;
        }

        pixelateCoroutine = StartCoroutine(PixelateRoutine(time));
        Debug.Log("Start telling the player");
    }

    // pixelates the render texture showing what the outside camera sees
    IEnumerator PixelateRoutine(float time)
    {
        if (outsideRenderTextureView == null)
            yield break; // missing image

        originalWidth = outsideRenderTextureView.width;
        originalHeight = outsideRenderTextureView.height;

        float elapsed = 0f;

        // scale down aspect ratio until its crunchy
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            float perceptualT = Mathf.Pow(t, 0.1f); // because it doesnt look "pixealted" until like 900

            float scale = 1f - perceptualT * (1f - downScaleAmount);

            // res = original *  (1 - lerpscale)
            outsideRenderTextureView.Release();
            outsideRenderTextureView.width = (int)(originalWidth * scale);
            outsideRenderTextureView.height = (int)(originalHeight * scale);
            outsideRenderTextureView.Create();

            yield return null;
        }
    }

    public void DisablePixelate()
    {
        if (pixelateCoroutine != null)
        {
            StopCoroutine(pixelateCoroutine);
            pixelateCoroutine = null;
        }

        outsideRenderTextureView.Release();
        outsideRenderTextureView.width = originalWidth;
        outsideRenderTextureView.height = originalHeight;
        outsideRenderTextureView.Create();
    }

    public void LoadText(DialogueScriptableObj content)
    {
        dialogueDisplay.gameObject.SetActive(true);
        dialogueDisplay.StartDialogue(content);
    }

    IEnumerator FadeRoutine(float time)
    {
        if (cameraDim == null)
            yield break; // missing image

        Color colour = cameraDim.color;
        float startAlpha = 0f;
        float targetAlpha = dim;

        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            colour.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            Debug.Log(colour.a);
            cameraDim.color = colour;

            yield return null;
        }
    }

    public void DisableDim()
    {
        // alpha to 0
        Color colour = cameraDim.color;
        colour.a = 0;
        cameraDim.color = colour;
    }
    public void StartWalkingShake()
    {
        // Store player camera transforms
        foreach (var player in playerData)
        {
            if (player.Valid)
            {
                playerCameraLocalPos[player.Index] = player.Input.camera.transform.localPosition;
            }
        }
        
        if (_walkingShakeCoroutine == null)
            _walkingShakeCoroutine = StartCoroutine(WalkingShakeRoutine());
    }

    public void StopWalkingShake()
    {
        if (_walkingShakeCoroutine != null)
        {
            StopCoroutine(_walkingShakeCoroutine);
            _walkingShakeCoroutine = null;

            // reset all cameras to original position
            foreach (var player in playerData)
            {
                if (player.Valid && player.Input?.camera != null)
                    player.Input.camera.transform.localPosition = playerCameraLocalPos[player.Index];
            }
        }
    }

    private IEnumerator WalkingShakeRoutine()
    {
        while (true)
        {
            foreach (var playerData in playerData)
            {
                if (playerData.Valid && playerData.Input?.camera != null)
                {
                    StartCoroutine(WalkShakeOnce(playerData.Input.camera));
                }
            }
            yield return new WaitForSeconds(walkShakeInterval);
        }
    }

    private IEnumerator WalkShakeOnce(Camera cam)
    {
        Transform camTransform = cam.transform;
        Vector3 originalPos = camTransform.localPosition;

        float elapsed = 0f;
        while (elapsed < walkShakeDuration)
        {
            elapsed += Time.deltaTime;
            float offsetX = UnityEngine.Random.Range(-1f, 1f) * walkShakeIntensity;
            float offsetY = UnityEngine.Random.Range(-1f, 1f) * walkShakeIntensity;
            camTransform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            yield return null;
        }

        camTransform.localPosition = originalPos; // reset
    }

}
