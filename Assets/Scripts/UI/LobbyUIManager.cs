using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

namespace Network
{
    public class LobbyUIManager : MonoBehaviour, INetworkRunnerCallbacks
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

        [Header("Join Room Config")]
        [SerializeField] private NetworkRunner _networkRunnerPrefab; 
        [SerializeField] private Transform _sessionListContainer;
        [SerializeField] private GameObject _sessionEntryPrefab;

        [Header("Scene Config")]
        [SerializeField] private string _gameSceneName = "GameScene";
        
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

        public async void ShowJoinPanel()
        {
            _mainPanel.SetActive(false);
            _createRoomPanel.SetActive(false);
            _joinRoomPanel.SetActive(true);

            // Instantiate a temporary Runner to browse the Session Lobby
            if (_runnerInstance == null)
            {
                if (_networkRunnerPrefab != null)
                {
                    // It's vital to instantiate a prefab that contains the NetworkRunner component
                    _runnerInstance = Instantiate(_networkRunnerPrefab);
                    _runnerInstance.AddCallbacks(this); 
                }
                else
                {
                    Debug.LogError("[NETWORK] NetworkRunnerPrefab is missing in the inspector.");
                    return;
                }
            }

            Debug.Log("[NETWORK] Connecting to Session Lobby...");
            
            // Join the lobby to receive session updates without entering a game session yet
            await _runnerInstance.JoinSessionLobby(SessionLobby.ClientServer);
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

            // Shutdown the lobby runner before loading the game scene
            if (_runnerInstance != null)
            {
                await _runnerInstance.Shutdown();
            }

            SceneManager.LoadScene(_gameSceneName);
        }

        // ============================================================
        // LOBBY CALLBACKS
        // ============================================================

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Debug.Log($"[NETWORK] Lobby Update: {sessionList.Count} rooms found.");

            // Clear the current list by destroying old entry objects
            foreach (Transform child in _sessionListContainer)
            {
                Destroy(child.gameObject);
            }

            // Iterate through the sessions provided by Fusion and create entries
            foreach (var session in sessionList)
            {
                // Only display sessions marked as visible
                if (session.IsVisible)
                {
                    GameObject entryGO = Instantiate(_sessionEntryPrefab, _sessionListContainer);
                    
                    if (entryGO.TryGetComponent(out SessionEntryUI entryUI))
                    {
                        // Configure the entry and assign the join action using the session name
                        entryUI.Setup(session, () => {
                            JoinSession(session.Name);
                        });
                    }
                }
            }
        }

        // Triggered when a player clicks a room entry in the list
        private async void JoinSession(string sessionName)
        {
            // Store configuration for the game scene
            RoomConfig.RoomName = sessionName;
            RoomConfig.IsHost = false; // Player is joining an existing room as a Client
            
            // Shutdown the Lobby Runner to avoid conflicts with the Game Runner
            if (_runnerInstance != null)
            {
                await _runnerInstance.Shutdown();
            }
            
            // Transition to the game scene
            SceneManager.LoadScene(_gameSceneName);
        }

        // ============================================================
        // EMPTY CALLBACKS (Required by INetworkRunnerCallbacks)
        // ============================================================
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}