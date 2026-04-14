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
    // Agregamos la interfaz INetworkRunnerCallbacks
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
        [SerializeField] private NetworkRunner _networkRunnerPrefab; // NUEVO: Prefab del motor de red temporal

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

            // NUEVO: Instanciar el Runner temporal y conectarlo al Lobby
            if (_runnerInstance == null)
            {
                if (_networkRunnerPrefab != null)
                {
                    // Es vital instanciar un prefab que ya tenga el componente NetworkRunner
                    _runnerInstance = Instantiate(_networkRunnerPrefab);
                    _runnerInstance.AddCallbacks(this); 
                }
                else
                {
                    Debug.LogError("[NETWORK] Falta asignar el NetworkRunnerPrefab en el inspector.");
                    return;
                }
            }

            Debug.Log("[NETWORK] Conectando al Session Lobby...");
            
            // Esto le dice a Fusion: "No entres a jugar, solo mostrame qué salas existen"
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
            // NUEVO: Por ahora solo verificamos por consola que la conexión funciona. 
            Debug.Log($"[NETWORK] Actualización del Lobby: {sessionList.Count} salas encontradas.");
        }

        // ============================================================
        // EMPTY CALLBACKS (Requeridos por la interfaz)
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