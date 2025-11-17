using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


namespace UIScripts
{
    public class MainMenuHandler : MonoBehaviour
    {
        private Button _startButton;
        private Button _quitButton;

        void Start()
        {
            // JQuery like way of retrieving the specific UI elements we care about 
            // https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-UQuery.html
            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            _startButton = root.Query<Button>("StartButton").First();
            _quitButton = root.Query<Button>(name: "QuitButton").First();

            // According to https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/UISupport.html
            // This is how to register the click handler while supporting mouse click and gamepad submit actions
            _startButton.clicked += StartButtonPressed;
            _quitButton.clicked += QuitButtonPressed;
        }

        private void StartButtonPressed()
        {
            RuntimeManager.PlayOneShot("event:/SFX/UI/choose");
            // doing delay so that the sound effect can play before switching scenes
            StartCoroutine(LoadSceneAfterDelay(SceneConstants.CharacterSelectScene, 0.2f));
        }

        private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneName);
        }

        private static void QuitButtonPressed()
        {
            Debug.Log("QuitButtonPressed");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(0);
#endif
        }
    }
}
