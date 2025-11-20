using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class LoadingScreenUIHandler : MonoBehaviour
{
    [SerializeField] private UIDocument loadingScreen;
    private VisualElement _loadingScreenRoot;

    private VisualElement _plorpLoadingAnimation;
    // Loading animation FPS default -1 making animation 1 second.
    [SerializeField] private int fps = -1;
    private float _yieldTimeFromFPS;
    [SerializeField] private float fadeAnimationSeconds = 0.5f;
    private const float FadeAnimationStep = 0.05f;

    [SerializeField] private Texture2D[] _loadingAnimationSprites;

    
    private Coroutine _loadingAnimationCoroutine;

    private void Awake()
    {
        _loadingScreenRoot = loadingScreen.rootVisualElement;
        _plorpLoadingAnimation = _loadingScreenRoot.Query<VisualElement>("PlorpLoadingAnimation");
        
        _plorpLoadingAnimation.style.backgroundImage = _loadingAnimationSprites[0];

        if (fps <= 0)
        {
            fps = _loadingAnimationSprites.Length;
        }
        
        // Calculate yield time from FPS
        _yieldTimeFromFPS = 1.0f / fps;

        _loadingScreenRoot.style.opacity = 0.0f;
    }

    /// <summary>
    /// Show the loading screen. Use returned IEnumerator if using the fade in animation.
    /// </summary>
    public IEnumerator ShowLoadingScreen()
    {
        // Start animation
        if (_loadingAnimationCoroutine != null)
            StopCoroutine(_loadingAnimationCoroutine);

        _loadingAnimationCoroutine = StartCoroutine(PlorpLoadingAnimation());

        yield return StartCoroutine(FadeInAnimation());
    }

    /// <summary>
    /// Hide the loading screen. Use returned IEnumerator if using the fade out animation.
    /// </summary>
    public IEnumerator HideLoadingScreen()
    {
        yield return StartCoroutine(FadeOutAnimation());
        
        if (_loadingAnimationCoroutine != null)
            StopCoroutine(_loadingAnimationCoroutine);
    }

    private IEnumerator PlorpLoadingAnimation()
    {
        while (true)
        {
            for (int i = 0; i < _loadingAnimationSprites.Length; i++)
            {
                _plorpLoadingAnimation.style.backgroundImage = _loadingAnimationSprites[i];
                yield return new WaitForSeconds(_yieldTimeFromFPS);
            }
        }
    }

    // Fade into the loading screen based on FadeAnimationSeconds
    private IEnumerator FadeInAnimation()
    {
        float counter = 0.0f;
        while (_loadingScreenRoot.style.opacity.value < fadeAnimationSeconds)
        {
            _loadingScreenRoot.style.opacity = Mathf.InverseLerp(0.0f, fadeAnimationSeconds, counter);
            counter += FadeAnimationStep;
            yield return new WaitForSecondsRealtime(FadeAnimationStep);
        }
        // Correct any off errors
        _loadingScreenRoot.style.opacity = 1.0f;
    }
    
    // Fade out of the loading screen based on FadeAnimationSeconds
    private IEnumerator FadeOutAnimation()
    {
        float counter = fadeAnimationSeconds;
        while (_loadingScreenRoot.style.opacity.value > 0.0f)
        {
            _loadingScreenRoot.style.opacity = Mathf.InverseLerp(0.0f, fadeAnimationSeconds, counter);
            counter -= FadeAnimationStep;
            yield return new WaitForSecondsRealtime(FadeAnimationStep);
        }
        // Correct any off errors
        _loadingScreenRoot.style.opacity = 0.0f;
    }
}
