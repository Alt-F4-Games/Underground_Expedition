using Fusion;
using Network;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private Transform _playercameraPivot;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Renderer _renderer; 
    private Camera _playerCamera;
    
    private NetworkCharacterController _characterController;
    
    
    
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            _renderer.material.color = Color.yellow;
        }

        if (HasInputAuthority)
        {
            _playerCamera = Camera.main;
            if (_playerCamera != null)
                _playerCamera.GetComponent<NetworkPlayerCamera>().CameraPivot = _playercameraPivot;
        }
    }

    private void Awake()
    {
        _characterController = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputPlayer inputPlayer)) return;
        
        Vector3 moveVector = inputPlayer.MoveDirection.normalized;
        _characterController.Move(moveVector * _moveSpeed * Runner.DeltaTime);
        
        if (inputPlayer.Buttons.IsSet(NetworkInputPlayer.JUMP_BUTTON) && HasStateAuthority)
        {
            _characterController.Jump();
        }
    }
}
