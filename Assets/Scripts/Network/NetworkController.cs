using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine.UI;
using System;
using Network;
using UnityEngine.InputSystem;

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
    
    private Vector2 _moveInput; 
    private bool _jumpPressed;
    private Vector2 _lookInput;
    void Start()
    {
        _createRoomButton.onClick.AddListener(CreateRoom);
        _joinRoomButton.onClick.AddListener(JoinRoom);
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpPressed = context.ReadValue<float>() > 0;
    }
    
    private async void CreateRoom()
    {
        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = "TestRoom",
            SceneManager = _networkSceneManagerDefault,
        };

        var result = await _networkRunner.StartGame(gameArg);
     

        if (!result.Ok)
        {
            Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            Debug.LogError($"Error: {result.ErrorMessage}");
        }
    }

    private async void JoinRoom()
    {
        var gameArg = new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = "TestRoom",
            SceneManager = _networkSceneManagerDefault,
        };
        var result = await _networkRunner.StartGame(gameArg);
        

        if (!result.Ok)
        {
            Debug.LogError($"Failed to start game: {result.ShutdownReason}");
            Debug.LogError($"Error: {result.ErrorMessage}");
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
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var InputPlayer = new NetworkInputPlayer();
        
        InputPlayer.MoveDirection = new Vector3(_moveInput.x,0 ,_moveInput.y );
        InputPlayer.Buttons.Set(NetworkInputPlayer.JUMP_BUTTON, _jumpPressed); 
        
        input.Set(InputPlayer);
    }
    
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
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
