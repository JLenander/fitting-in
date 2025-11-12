using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UIScripts
{
    /// <summary>
    /// The UI handler for the character selection screen.
    /// </summary>
    public class CharacterSelectHandler : MonoBehaviour, ICharacterSelectScreen
    {
        private VisualElement[] _playerBoxes;
        private VisualElement _readyText;
        private Label[] _playerColorWarnings;
        private VisualElement[] _playerArrows;
        private VisualElement[] _playerLabels;
        
        public GameObject[] plorpSpawns; // assign in inspector
        private PlorpSelect[] _plorps = new PlorpSelect[3];
        public GameObject plorpPrefab;

        private int _readyPlayers;
        private int _playerCount;

        private readonly Color[] _availableColors = {
            HexToColor("#e74848ff"),
            HexToColor("#5656f3ff"),
            HexToColor("#e4e40fff"),
            HexToColor("#38d45aff"),
            HexToColor("#ff83f5ff"),
            HexToColor("#2cebf1ff"),
            HexToColor("#c340ffff"),
            HexToColor("#ffb327ff"),
        };

        private static Color HexToColor(string hex)
        {
            Color color;
            if (ColorUtility.TryParseHtmlString(hex, out color))
                return color;
            return Color.white;
        }

        private int[] _playerColorIndices = { 0, 1, 2 };

        private GlobalPlayerManager _playerManager;

        void Start()
        {
            _playerColorWarnings = new Label[3];
            _playerLabels = new VisualElement[3];
            _playerArrows = new VisualElement[6];

            var root = gameObject.GetComponent<UIDocument>().rootVisualElement;

            _readyText = root.Query<VisualElement>("StartInstruction");


            for (int i = 0; i < 3; i++)
            {
                _playerColorWarnings[i] = root.Query<Label>("Player" + (i + 1) + "ColorWarning").First();
                _playerLabels[i] = root.Query<VisualElement>("P" + (i + 1) + "Label").First();
                _playerArrows[i] = root.Query<VisualElement>("P" + (i + 1) + "L1").First();
                _playerArrows[i + 3] = root.Query<VisualElement>("P" + (i + 1) + "R1").First();
            }

            _playerManager = FindAnyObjectByType<GlobalPlayerManager>();
        }

        public void AddPlayer(int playerIndex, PlayerInput playerInput)
        {
            // spawn player prefab at anchor instead
            var plorpGameObj = Instantiate(plorpPrefab, plorpSpawns[playerIndex].transform.position, plorpSpawns[playerIndex].transform.rotation);
            _plorps[playerIndex] = plorpGameObj.GetComponent<PlorpSelect>();
            _plorps[playerIndex].Initialize(playerInput);
            
            _playerArrows[playerIndex].visible = true;
            _playerArrows[playerIndex + 3].visible = true;
            _playerLabels[playerIndex].visible = true;
            
            // Set player select box background color to player color
            _plorps[playerIndex].ChangeColor(_availableColors[playerIndex]);
            
            // Player default select default color
            _playerManager.playerColorSelector[playerIndex] = _availableColors[playerIndex];
            
            _playerCount++;
            _readyText.visible = AllPlayersReady();
        }

        public void RemovePlayer(int playerIndex)
        {
            // destroy player prefab
            Destroy(_plorps[playerIndex].gameObject);
            _plorps[playerIndex] = null;
            
            _playerColorWarnings[playerIndex].visible = false;
            _playerArrows[playerIndex].visible = false;
            _playerArrows[playerIndex + 3].visible = false;
            _playerLabels[playerIndex].visible = false;
            
            // Free up color from selector
            _playerManager.playerColorSelector[playerIndex] = Color.clear;
            
            _playerCount--;
            
            // if now rest of players all ready, show "all players ready" text
            _readyText.visible = AllPlayersReady();
        }

        public void ReadyPlayer(int playerIndex)
        {
            // Play ready animation
            _plorps[playerIndex].Ready();
            
            _readyPlayers++;
            // if that was last player to ready, show "all players ready" text
            _readyText.visible = AllPlayersReady();
            
            // _playerColorWarnings[playerIndex].visible = false;
            _playerArrows[playerIndex].visible = false;
            _playerArrows[playerIndex + 3].visible = false;
        }

        public void UnreadyPlayer(int playerIndex)
        {
            // back to idle animation
            _plorps[playerIndex].Unready();
            
            _playerColorWarnings[playerIndex].visible = false;
            _playerArrows[playerIndex].visible = true;
            _playerArrows[playerIndex + 3].visible = true;
            
            _readyPlayers--;
            
            // if all players were ready now not, hide "all players ready" text
            _readyText.visible = AllPlayersReady();
        }

        // True if all players (at least 1) have readied up
        private bool AllPlayersReady()
        {
            return _readyPlayers == _playerCount && _playerCount > 0;
        }

        // Called in GLobalPlayerManager when a player changes color (left/right bumper action)
        public void ChangeColor(int playerIndex, int direction)
        {
            // Ignore color change if player is ready
            if (_playerManager.Players[playerIndex].Ready)
                return;
            
            // Cycle index
            var max = _availableColors.Length;
            _playerColorIndices[playerIndex] = (_playerColorIndices[playerIndex] + direction + max) % max;
            var newColor = _availableColors[_playerColorIndices[playerIndex]];
            
            // Update plorp outline color
            _plorps[playerIndex].ChangeColor(newColor);
            
            // Update GlobalPlayerManager’s color selector
            _playerManager.playerColorSelector[playerIndex] = newColor;
            HideColorConflictWarning(playerIndex);
        }

        public void ShowColorConflictWarning(int playerIndex, int otherIndex)
        {
            string message = "Color taken by Player " + (otherIndex + 1);
            _playerColorWarnings[playerIndex].text = message;
            _playerColorWarnings[playerIndex].visible = true;
        }

        public void HideColorConflictWarning(int playerIndex)
        {
            _playerColorWarnings[playerIndex].visible = false;
        }

        public void DestroyPlorps()
        {
            for (int i = 0; i < _plorps.Length; i++)
            {
                if (_plorps[i] != null)
                {
                    Destroy(_plorps[i].gameObject);
                    _plorps[i] = null;
                }
            }
        }
    }
}
