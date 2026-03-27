using Fusion;
using UnityEngine;

namespace Health
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkCharacterController))]
    public class NetworkPlayerHealth : NetworkHealthSystem
    {
        [Header("Death Settings")]
        [SerializeField] private float _respawnDelay = 3f;

        [Header("References")]
        [SerializeField] private Renderer _renderer;

        private NetworkCharacterController _controller;

        public override void Spawned()
        {
            base.Spawned();

            _controller = GetComponent<NetworkCharacterController>();
        }

        
    }
}