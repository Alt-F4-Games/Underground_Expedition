using UnityEngine;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using System; 
using Network;
using UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network References")]
    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] private NetworkSceneManagerDefault _networkSceneManagerDefault;
    [SerializeField] private NetworkObject _playerprefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnPoint;

    private Dictionary<PlayerRef, NetworkObject> _players = new();

    public static NetworkController Instance;

    [Header("Test Items")]
    [SerializeField] private NetworkObject _testEnemyPrefab;
    private bool worldItemsSpawned = false;

    // ---------------- INPUT ----------------
    [Header("Mouse Settings")]
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _maxLookAngle = 80f;

    private Vector2 _moveInput; 
    private bool _jumpPressed;
    private bool _sprintPressed;

    private float _accumulatedYaw;
    private float _accumulatedPitch;

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_networkRunner != null)
                _networkRunner.AddCallbacks(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
#if UNITY_EDITOR
        if ((string.IsNullOrEmpty(RoomConfig.RoomName) || RoomConfig.RoomName == "DefaultRoom") 
            && (_networkRunner != null && _networkRunner.Config == null))
        {
            RoomConfig.RoomName = "Test_" + Guid.NewGuid().ToString().Substring(0, 6);
            RoomConfig.MaxPlayers = 4;
            RoomConfig.IsHost = true;
        }
#endif
        
        if (!string.IsNullOrEmpty(RoomConfig.RoomName) && _networkRunner.Config == null)
        {
            if (RoomConfig.IsHost)
                CreateRoom();
            else
                JoinSpecificRoom(RoomConfig.RoomName);
        }
    }

    // ============================================================
    // INPUT
    // ============================================================

    public void OnMove(InputAction.CallbackContext context) => _moveInput = context.ReadValue<Vector2>(); 
    public void OnJump(InputAction.CallbackContext context) => _jumpPressed = context.ReadValue<float>() > 0;
    public void OnSprint(InputAction.CallbackContext context) => _sprintPressed = context.ReadValue<float>() > 0;

    public void OnLook(InputAction.CallbackContext context) 
    { 
        Vector2 mouseDelta = context.ReadValue<Vector2>();

        _accumulatedYaw += mouseDelta.x * _mouseSensitivity;
        _accumulatedPitch -= mouseDelta.y * _mouseSensitivity; 
        _accumulatedPitch = Mathf.Clamp(_accumulatedPitch, -_maxLookAngle, _maxLookAngle);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputPlayer();
        
        data.Buttons.Set(NetworkInputPlayer.JUMP_BUTTON, _jumpPressed); 
        data.Buttons.Set(NetworkInputPlayer.SPRINT_BUTTON, _sprintPressed);
        
        if (InputManager.Mode != InputMode.Game)
        {
            data.MoveDirection = Vector3.zero;
            data.MouseRotation = Vector2.zero;
        }
        else
        {
            data.MoveDirection = new Vector3(_moveInput.x, 0, _moveInput.y);
            data.MouseRotation = new Vector2(_accumulatedYaw, _accumulatedPitch); 
        }
        
        input.Set(data);
    }

    // ============================================================
    // SESSION
    // ============================================================

    private async void CreateRoom()
    {
        if (_networkRunner.IsRunning) return; 

        var result = await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = RoomConfig.RoomName,
            PlayerCount = RoomConfig.MaxPlayers,
            SceneManager = _networkSceneManagerDefault,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
        });

        if (!result.Ok)
            Debug.LogError($"[NETWORK] Error creating room: {result.ShutdownReason}");
    }

    public async void JoinSpecificRoom(string sessionName)
    {
        if (_networkRunner.IsRunning) return; 

        var result = await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionName,
            SceneManager = _networkSceneManagerDefault,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex)
        });

        if (!result.Ok)
            Debug.LogError($"[NETWORK] Error joining room: {result.ShutdownReason}");
    }

    // ============================================================
    // PLAYER SPAWN
    // ============================================================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        if (!runner.IsServer) return;

        if (_players.ContainsKey(player))
            return;

        Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;

        var obj = runner.Spawn(_playerprefab, spawnPos, spawnRot, player);
        _players.Add(player, obj);

        if (!worldItemsSpawned && _testEnemyPrefab != null)
        {
            runner.Spawn(_testEnemyPrefab, spawnPos + new Vector3(5,0,5), Quaternion.identity);
            worldItemsSpawned = true;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        if (_players.Remove(player, out var obj))
            runner.Despawn(obj);
    }

    // ============================================================
    // SCENE CHANGE 
    // ============================================================

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        var spawn = FindObjectOfType<PlayerSpawnPoint>();
        if (spawn == null)
        {
            Debug.LogWarning("No PlayerSpawnPoint in scene");
            return;
        }

        foreach (var kvp in _players)
        {
            PlayerRef playerRef = kvp.Key;
            NetworkObject playerObj = kvp.Value;

            if (playerObj.TryGetComponent(out NetworkCharacterController controller))
            {
                Vector3 pos = spawn.GetPosition(playerRef);
                controller.Teleport(pos);
            }
        }

        Debug.Log("[NETWORK] Players teleported with offset");
    }

    // ============================================================
    // SHUTDOWN
    // ============================================================

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        var inv = NetworkInventoryManager.Local;

        if (inv && inv.HasInputAuthority)
            inv.SaveLocalInventory();
    }

    // ============================================================
    // EMPTY CALLBACKS
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
    public void OnSceneLoadStart(NetworkRunner runner) { }
}