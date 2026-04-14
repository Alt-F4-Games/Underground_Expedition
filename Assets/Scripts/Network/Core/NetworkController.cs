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

/// <summary>
/// GENERAL NETWORK CONTROLLER
/// --------------------------
/// This script acts as the initial “brain” of the networking system:
/// - Creates or joins multiplayer sessions.
/// - Spawns players when they connect.
/// - Handles local input.
/// - Spawns initial test items (for debugging only).
///
/// For NON-PROGRAMMERS:
/// Think of this script as the multiplayer receptionist.
/// Every time someone joins, it creates their character.
/// Every time they move or jump, this script sends that information to the match.
/// </summary>


public class NetworkController : MonoBehaviour, INetworkRunnerCallbacks
{
    // ---------------------------- UI ----------------------------
    [Header("UI References")]
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;

    // ----------------------- Network Config ---------------------
    [Header("Network References")]
    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] private NetworkSceneManagerDefault _networkSceneManagerDefault;
    [SerializeField] private NetworkObject _playerprefab;

    private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private int sceneIndex ;
    
    // ----------------------- Test Items -------------------------
    [Header("Test Items (Only for development)")]
    [SerializeField] private NetworkObject _testItemPrefab;
    private bool worldItemsSpawned = false;
    [SerializeField] private NetworkObject _testEnemyPrefab;
    
     
    // ------------------------ Player Input ---------------------- 
    private Vector2 _moveInput; 
    private bool _jumpPressed;
    private Vector2 _lookInput;
    private float _yawInput;
   
    // ============================================================
    //                      UNITY EVENTS
    // ============================================================
    void Start()
    {
        _createRoomButton.onClick.AddListener(CreateRoom);
        _joinRoomButton.onClick.AddListener(JoinRoom);
        
        if (!string.IsNullOrEmpty(RoomConfig.RoomName) && _networkRunner.Config == null)
        {
            CreateRoom();
        }
    }
    
    // ============================================================
    //                       INPUT SYSTEM
    // ============================================================
    
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
    
    // ============================================================
    //                     ROOM CREATION / JOIN
    // ============================================================
    
    private async void CreateRoom()
    {
        // Usamos el nombre que viene del Menú a través del puente
        string sessionName = RoomConfig.RoomName; 

        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionName, 
            SceneManager = _networkSceneManagerDefault,
            Scene = SceneRef.FromIndex(sceneIndex)
        };

        var result = await _networkRunner.StartGame(gameArg);

        if (!result.Ok)
        {
            Debug.LogError($"[NETWORK] Error al crear sala: {result.ShutdownReason}");
        }
    }
    public async void JoinSpecificRoom(string sessionName)
    {
        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = _networkSceneManagerDefault,
        };

        var result = await _networkRunner.StartGame(gameArg);

        if (!result.Ok)
        {
            Debug.LogError($"[NETWORK] Error al unirse: {result.ShutdownReason}");
        }
    }
    
    // ============================================================
    //                   PLAYER JOIN / LEAVE
    // ============================================================

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
    
    // ============================================================
    //                  SHUTDOWN (GUARDADO LOCAL)
    // ============================================================
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("[Runner] Shutdown detected - Saving player inventory");

        var inv = NetworkInventoryManager.Local;

        if (inv && inv.HasInputAuthority)
            inv.SaveLocalInventory();
    }
    
    // ============================================================
    //                     EMPTY CALLBACKS 
    // ============================================================
    
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
