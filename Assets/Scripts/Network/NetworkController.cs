using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine.UI;
using System;
using Network;
using UnityEngine.InputSystem;

/// <summary>
/// CONTROLADOR GENERAL DE RED
/// ---------------------------
/// Este script es el “cerebro” inicial del networking:
/// - Crea o se une a salas.
/// - Spawnea jugadores cuando entran.
/// - Gestiona inputs locales.
/// - Spawnea ítems iniciales (solo para testeo).
///
/// Para NO PROGRAMADORES:
/// Piensen en este script como el recepcionista del multijugador.
/// Cada vez que alguien entra, se crea su personaje.
/// Cada vez que se mueve o salta, este script envía esa información a la partida.
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
    
    // ----------------------- Test Items -------------------------
    [Header("Test Items (Only for development)")]
    [SerializeField] private NetworkObject _testItemPrefab;
    private bool worldItemsSpawned = false;
     
    // ------------------------ Player Input ---------------------- 
    private Vector2 _moveInput; 
    private bool _jumpPressed;
    private Vector2 _lookInput;
   
    // ============================================================
    //                      UNITY EVENTS
    // ============================================================
    void Start()
    {
        _createRoomButton.onClick.AddListener(CreateRoom);
        _joinRoomButton.onClick.AddListener(JoinRoom);
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
        
        InputPlayer.MoveDirection = new Vector3(_moveInput.x,0 ,_moveInput.y );
        InputPlayer.Buttons.Set(NetworkInputPlayer.JUMP_BUTTON, _jumpPressed); 
        InputPlayer.MouseRotation = _lookInput;
        
        input.Set(InputPlayer);
        _lookInput = Vector2.zero;
    }
    
    // ============================================================
    //                     ROOM CREATION / JOIN
    // ============================================================
    
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
    
    // ============================================================
    //                   PLAYER JOIN / LEAVE
    // ============================================================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        Debug.Log($"Player joined");
        _lobbyPanel.SetActive(false);

        if (!_networkRunner.IsServer) return;

        //TODO: Move this logic to Spawner.cs, this is only to test 
        if (!worldItemsSpawned)
        {
            SpawnWorldItems();
            worldItemsSpawned = true;
        }
        
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
    //                    WORLD ITEM SPAWNING (TEMP)
    // ============================================================

    private void SpawnWorldItems()
    {
        if (!_testItemPrefab) return;

        Vector3[] positions =
        {
            new(5, 1, 0),
            new(5, 1, 3),
            new(5, 1, 6)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            var obj = _networkRunner.Spawn(_testItemPrefab, positions[i], Quaternion.identity);
            if (obj.TryGetComponent(out NetworkWorldItem item))
                item.Init(i + 1, i == 1 ? 2 : 1); 
        }

        Debug.Log("Server spawned test items");
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
