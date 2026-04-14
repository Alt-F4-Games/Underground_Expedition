using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fusion; // CORRECCIÓN 1: Agregamos Fusion para que reconozca el Runner

namespace Network
{
    public class LobbyUIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _createRoomPanel;
        [SerializeField] private GameObject _joinRoomPanel;

        [Header("Create Room Inputs")]
        [SerializeField] private TMP_InputField _roomNameInputField;
        [SerializeField] private TMP_InputField _maxPlayersInputField;
        [SerializeField] private Button _confirmCreateButton;
        
        [Header("Navigation Buttons")]
        [SerializeField] private Button _toCreatePanelButton;
        [SerializeField] private Button _toJoinPanelButton;
        [SerializeField] private Button _backToMainFromCreate;
        [SerializeField] private Button _backToMainFromJoin;

        [Header("Scene Config")]
        [SerializeField] private string _gameSceneName = "GameScene";

        // CORRECCIÓN 2: Declaramos la variable del runner
        private NetworkRunner _runnerInstance; 

        private void Start()
        {
            ShowMainPanel();

            _toCreatePanelButton.onClick.AddListener(ShowCreatePanel);
            _toJoinPanelButton.onClick.AddListener(ShowJoinPanel);
            _backToMainFromCreate.onClick.AddListener(ShowMainPanel);
            _backToMainFromJoin.onClick.AddListener(ShowMainPanel);

            _confirmCreateButton.onClick.AddListener(OnConfirmCreateRoom);
        }

        public void ShowMainPanel()
        {
            _mainPanel.SetActive(true);
            _createRoomPanel.SetActive(false);
            _joinRoomPanel.SetActive(false);
        }

        public void ShowCreatePanel()
        {
            _mainPanel.SetActive(false);
            _createRoomPanel.SetActive(true);
            _joinRoomPanel.SetActive(false);
        }

        public void ShowJoinPanel()
        {
            _mainPanel.SetActive(false);
            _createRoomPanel.SetActive(false);
            _joinRoomPanel.SetActive(true);
        }

        private async void OnConfirmCreateRoom()
        {
            string roomName = _roomNameInputField.text;

            if (string.IsNullOrWhiteSpace(roomName))
            {
                Debug.LogWarning("Room name cannot be empty.");
                return;
            }

            int capacity = 4;
            if (_maxPlayersInputField != null && int.TryParse(_maxPlayersInputField.text, out int parsedCapacity))
            {
                capacity = Mathf.Clamp(parsedCapacity, 2, 10);
            }

            RoomConfig.RoomName = roomName;
            RoomConfig.MaxPlayers = capacity; 
            RoomConfig.IsHost = true;

            if (_runnerInstance != null)
            {
                await _runnerInstance.Shutdown();
            }

            SceneManager.LoadScene(_gameSceneName);
        }
    }
}