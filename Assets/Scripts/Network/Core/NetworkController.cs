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
    [Header("UI References")]
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinRoomButton;

    [Header("Network References")]
    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] private NetworkSceneManagerDefault _networkSceneManagerDefault;
    [SerializeField] private NetworkObject _playerprefab;

    private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private int sceneIndex;
    
    [Header("Test Items (Only for development)")]
    [SerializeField] private NetworkObject _testItemPrefab;
    private bool worldItemsSpawned = false;
    [SerializeField] private NetworkObject _testEnemyPrefab;
    
    // ------------------------ Player Input ---------------------- 
    [Header("Mouse Settings")]
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _maxLookAngle = 80f;

    private Vector2 _moveInput; 
    private bool _jumpPressed;
    private bool _sprintPressed;
    
    // Store the final calculated angles to avoid desync
    private float _accumulatedYaw;
    private float _accumulatedPitch;
   
    void Start()
    {
        _createRoomButton.onClick.AddListener(CreateRoom);
        _joinRoomButton.onClick.AddListener(() => JoinSpecificRoom(RoomConfig.RoomName));

#if UNITY_EDITOR
        if (string.IsNullOrEmpty(RoomConfig.RoomName) && (_networkRunner != null && _networkRunner.Config == null))
        {
            Debug.Log("<color=cyan>[NETWORK]</color> Editor execution detected. Creating automatic test room...");
    
            RoomConfig.RoomName = "Test_" + System.Guid.NewGuid().ToString().Substring(0, 6);
            RoomConfig.MaxPlayers = 4;
            RoomConfig.IsHost = true;
        }
#endif
        
        if (!string.IsNullOrEmpty(RoomConfig.RoomName) && _networkRunner.Config == null)
        {
            // Evaluate the role configured in the menu
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
    public void OnSprint(InputAction.CallbackContext context) { _sprintPressed = context.ReadValue<float>() > 0; }
    
    public void OnLook(InputAction.CallbackContext context) 
    { 
        Vector2 mouseDelta = context.ReadValue<Vector2>();

        // Apply sensitivity and accumulate YAW (Horizontal rotation)
        _accumulatedYaw += mouseDelta.x * _mouseSensitivity;
        
        // Apply sensitivity and accumulate PITCH (Vertical rotation)
        // Subtract because moving the mouse up (positive Y) rotates the camera up (negative Pitch in Unity)
        _accumulatedPitch -= mouseDelta.y * _mouseSensitivity; 
        
        // CLAMP: Limit the raw accumulator so it NEVER exceeds 80. This fixes the over-rotation bug!
        _accumulatedPitch = Mathf.Clamp(_accumulatedPitch, -_maxLookAngle, _maxLookAngle);
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var InputPlayer = new NetworkInputPlayer();
        
        InputPlayer.Buttons.Set(NetworkInputPlayer.JUMP_BUTTON, _jumpPressed); 
        InputPlayer.Buttons.Set(NetworkInputPlayer.SPRINT_BUTTON, _sprintPressed);
        
        if (InputManager.Mode != InputMode.Game)
        {
            InputPlayer.MoveDirection = Vector3.zero;
            InputPlayer.MouseRotation = Vector2.zero;
        }
        else
        {
            InputPlayer.MoveDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
            // Send the perfectly clamped angles across the network
            InputPlayer.MouseRotation = new Vector2(_accumulatedYaw, _accumulatedPitch); 
        }
        
        input.Set(InputPlayer);
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
        Debug.Log($"[NETWORK] Player joined");
        if (_lobbyPanel != null) _lobbyPanel.SetActive(false);

        if (!_networkRunner.IsServer) return;
        
        // Spawn player with a small random offset on the X axis
        var playerSpawned = _networkRunner.Spawn(_playerprefab, new Vector3(UnityEngine.Random.Range(-3,3), 0, 0), Quaternion.identity, player);
        _players.Add(player, playerSpawned);
    }
    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
       if(!_networkRunner.IsServer) return;
       
       if (_players.Remove(player, out var playerSpawned))
       {
           _networkRunner.Despawn(playerSpawned);
           Debug.Log($"[NETWORK] Player left and despawned");
       }
    }
    
    // ============================================================
    //                  SHUTDOWN (LOCAL SAVE)
    // ============================================================
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("[Runner] Shutdown detected - Saving player inventory locally");

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