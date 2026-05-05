using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Health;
using Network.Enemies.Core;

namespace Network.Enemies.States
{
    public class NetworkExplodeState : INetworkState
    {
        private NetworkEnemyController _enemy;

        private float _radius;
        private int _damage;
        private float _stunTime;

        private bool _hasExploded;
        
        private HashSet<NetworkObject> _hitTargets;

        public NetworkExplodeState(float radius, int damage, float stunTime)
        {
            _radius = radius;
            _damage = damage;
            _stunTime = stunTime;
        }

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            _hasExploded = false;
            _hitTargets = new HashSet<NetworkObject>();
            
            if (_enemy.Agent != null && _enemy.Agent.isOnNavMesh)
            {
                _enemy.Agent.isStopped = true;
            }

            var listeners = _enemy.GetComponents<IOnExplodeListener>();

            foreach (var listener in listeners)
            {
                listener.OnExplode();
            }
        }

        public void Update()
        {
            if (!_hasExploded)
            {
                Explode();
                _hasExploded = true;
            }
        }

        private void Explode()
        {
            Collider[] hits = Physics.OverlapSphere(
                _enemy.transform.position,
                _radius,
                _enemy.PlayerLayer
            );

            foreach (var hit in hits)
            {
                var netObj = hit.GetComponent<NetworkObject>();
                if (netObj == null) continue;
                
                if (_hitTargets.Contains(netObj)) continue;

                var health = netObj.GetComponent<NetworkHealthSystem>();
                var stunnable = netObj.GetComponent<IStunnable>();
                
                health?.TakeDamage(_damage);
                stunnable?.ApplyStun(_stunTime);

                _hitTargets.Add(netObj);
            }
            
            if (_enemy.HasStateAuthority)
            {
                _enemy.Runner.Despawn(_enemy.Object);
            }
        }

        public void Exit()
        {
            // Nothing for now.
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Exploding;
    }
}