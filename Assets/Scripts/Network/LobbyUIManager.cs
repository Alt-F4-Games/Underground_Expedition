using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        [SerializeField] private Button _confirmCreateButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _toCreatePanelButton;
        [SerializeField] private Button _toJoinPanelButton;
        [SerializeField] private Button _backToMainFromCreate;
        [SerializeField] private Button _backToMainFromJoin;

        [Header("Scene Config")]
        [SerializeField] private string _gameSceneName = "GameScene";

        private void Start()
        {
            // Initialize view
            ShowMainPanel();

            // Configure navigation buttons
            _toCreatePanelButton.onClick.AddListener(ShowCreatePanel);
            _toJoinPanelButton.onClick.AddListener(ShowJoinPanel);
            _backToMainFromCreate.onClick.AddListener(ShowMainPanel);
            _backToMainFromJoin.onClick.AddListener(ShowMainPanel);

            // Create room action
            _confirmCreateButton.onClick.AddListener(OnConfirmCreateRoom);
        }

        // Navigation Logic

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
            
            // In the future, we will call the room list system here
        }

        // Creation Logic

        private void OnConfirmCreateRoom()
        {
            string roomName = _roomNameInputField.text;

            if (string.IsNullOrWhiteSpace(roomName))
            {
                Debug.LogWarning("Room name cannot be empty.");
                return;
            }

            // Save the name in the data bridge
            RoomConfig.RoomName = roomName;

            // Load the game scene (where the NetworkRunner will be located)
            SceneManager.LoadScene(_gameSceneName);
        }
    }
}