using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine.UI;
using System;
using Network;
using UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("UI References")]
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;

    [Header("Network References")]
    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] private NetworkSceneManagerDefault _networkSceneManagerDefault;
    [SerializeField] private NetworkObject _playerprefab;

    private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private int sceneIndex ;
    
    [Header("Test Items (Only for development)")]
    [SerializeField] private NetworkObject _testItemPrefab;
    private bool worldItemsSpawned = false;
    [SerializeField] private NetworkObject _testEnemyPrefab;
    
    private Vector2 _moveInput; 
    private bool _jumpPressed;
    private Vector2 _lookInput;
    private float _yawInput;
   
    void Start()
    {
        _createRoomButton.onClick.AddListener(CreateRoom);
        // CORRECCIÓN 1: Usamos la función correcta con una función lambda
        _joinRoomButton.onClick.AddListener(() => JoinSpecificRoom(RoomConfig.RoomName));
        
        if (!string.IsNullOrEmpty(RoomConfig.RoomName) && _networkRunner.Config == null)
        {
            // CORRECCIÓN 2: Evaluamos el rol configurado en el menú
            if (RoomConfig.IsHost)
            {
                CreateRoom();
            }
            else
            {
                JoinSpecificRoom(RoomConfig.RoomName);
            }
        }
    }
    
    public void OnMove(InputAction.CallbackContext context) { _moveInput = context.ReadValue<Vector2>(); } 
    public void OnJump(InputAction.CallbackContext context) { _jumpPressed = context.ReadValue<float>() > 0; }
    public void OnLook(InputAction.CallbackContext context) { _lookInput = context.ReadValue<Vector2>(); }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var InputPlayer = new NetworkInputPlayer();
        
        InputPlayer.Buttons.Set(NetworkInputPlayer.JUMP_BUTTON, _jumpPressed); 
        
        if (InputManager.Mode != InputMode.Game)
        {
            InputPlayer.MoveDirection = Vector3.zero;
            InputPlayer.MouseRotation = Vector2.zero;
        }
        else
        {
            InputPlayer.MoveDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
            InputPlayer.MouseRotation = _lookInput;
        }
        
        input.Set(InputPlayer);
        _lookInput = Vector2.zero;
    }
    
    private async void CreateRoom()
    {
        if (_networkRunner.IsRunning) return; 

        _createRoomButton.interactable = false;
        _joinRoomButton.interactable = false;

        string sessionName = RoomConfig.RoomName; 

        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionName, 
            PlayerCount = RoomConfig.MaxPlayers,
            SceneManager = _networkSceneManagerDefault
        };

        var result = await _networkRunner.StartGame(gameArg);

        if (!result.Ok)
        {
            Debug.LogError($"[NETWORK] Error creating room: {result.ShutdownReason}");
            _createRoomButton.interactable = true;
            _joinRoomButton.interactable = true;
        }
    }
    
    public async void JoinSpecificRoom(string sessionName)
    {
        if (_networkRunner.IsRunning) return; 

        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = _networkSceneManagerDefault,
        };

        var result = await _networkRunner.StartGame(gameArg);

        if (!result.Ok)
        {
            Debug.LogError($"[NETWORK] Error joining room: {result.ShutdownReason}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log($"Player joined");
        _lobbyPanel.SetActive(false);

        if (!_networkRunner.IsServer) return;
        
        var playerSpawned = _networkRunner.Spawn(_playerprefab,new Vector3(UnityEngine.Random.Range(-3,3),0,0),Quaternion.identity,player);
        _players.Add(player,playerSpawned);
    }
    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
       if(!_networkRunner.IsServer) return;
       
       if (_players.Remove(player, out var playerSpawned))
       {
           _networkRunner.Despawn(playerSpawned);
       }
    }
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("[Runner] Shutdown detected - Saving player inventory");

        var inv = NetworkInventoryManager.Local;

        if (inv && inv.HasInputAuthority)
            inv.SaveLocalInventory();
    }
    
    // EMPTY CALLBACKS 
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { } 
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}