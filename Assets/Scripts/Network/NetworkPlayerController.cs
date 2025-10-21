using Fusion;
using Network;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private Renderer _renderer;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            _renderer.material.color = Color.yellow;
        }
    }
    
}
