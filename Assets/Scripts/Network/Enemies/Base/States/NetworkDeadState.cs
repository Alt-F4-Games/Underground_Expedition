using Fusion;
using UnityEngine;
using Network.Spawn;

namespace Network.Enemies
{
    public class NetworkDeadState : INetworkState
    {
        private NetworkEnemyController _enemy;
        private float _deathDuration;
        private float _timer;
        private bool _isProcessed;

        public NetworkDeadState(float deathDuration)
        {
            _deathDuration = deathDuration;
        }

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            _timer = 0f;
            _isProcessed = false;
            
            if (_enemy.Agent != null && _enemy.Agent.isOnNavMesh)
            {
                _enemy.Agent.isStopped = true;
            }
            
            if (_enemy.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            
            var colliders = _enemy.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            Debug.Log($"[SERVER] {_enemy.gameObject.name} entered DEAD state. Waiting {_deathDuration}s...");
        }

        public void Update()
        {
            if (_isProcessed) return;
            
            _timer += _enemy.Runner.DeltaTime;

            if (_timer >= _deathDuration)
            {
                _isProcessed = true;
                if (_enemy.TryGetComponent(out NetworkLootSpawner spawner))
                {
                    spawner.SpawnLootHere();
                }
                
                if (_enemy.HasStateAuthority)
                {
                    _enemy.Runner.Despawn(_enemy.Object);
                }
            }
        }

        public void Exit() { }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Dead;
    }
}