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
        private Button _settingsButton;
        private Button _quitButton;
        
        [SerializeField] private MainMenuSettingsUIHandler mainMenuSettingsUIHandler;

        void Start()
        {
            // JQuery like way of retrieving the specific UI elements we care about 
            // https://docs.unity3d.com/6000.2/Documentation/Manual/UIE-UQuery.html
            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;
            _startButton = root.Query<Button>("StartButton").First();
            _settingsButton = root.Query<Button>("SettingsButton").First();
            _quitButton = root.Query<Button>(name: "QuitButton").First();

            // According to https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/UISupport.html
            // This is how to register the click handler while supporting mouse click and gamepad submit actions
            _startButton.clicked += StartButtonPressed;
            _settingsButton.clicked += SettingsButtonPressed;
            _quitButton.clicked += QuitButtonPressed;

            mainMenuSettingsUIHandler.RegisterReturnToMainMenuCallback(FocusSettingsButton);
            
            // Start by having the start menu button focused
            _startButton.Focus();
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

        private void SettingsButtonPressed()
        {
            // Open settings panel
            mainMenuSettingsUIHandler.ShowSettingsPanel();
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
        
        // Main Menu Settings panel handlers
        
        /// <summary>
        /// Focus back on the main menu panel. Focus is on the settings button since this is what it's used for
        /// </summary>
        private void FocusSettingsButton()
        {
            _settingsButton.Focus();
        }
    }
}
