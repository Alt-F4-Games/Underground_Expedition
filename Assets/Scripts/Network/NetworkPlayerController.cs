using Fusion;
using Network;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkPlayerController : NetworkBehaviour
{
    private NetworkCharacterController _characterController;
    [SerializeField] private Renderer _renderer;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            _renderer.material.color = Color.yellow;
        }
    }

    private void Awake()
    {
        _characterController = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputPlayer inputPlayer)) return;
    }
}
