using UnityEngine;
using Health;

namespace Network.Enemies
{
    public class NetworkAttackState : INetworkState
    {
        private NetworkEnemyController _enemy;
        private float _lastAttackTime;

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            _enemy.Agent.isStopped = true; // Stop moving to perform the attack
            
            // Allow the first attack to be almost immediate upon entering the state
            _lastAttackTime = Time.time - _enemy.AttackCooldown; 
            
            Debug.Log($"[SERVER] {_enemy.gameObject.name} started ATTACKING.");
        }

        public void Update()
        {
            // If the player disappears or is lost
            if (_enemy.TargetPlayer == null)
            {
                _enemy.StateMachine.ChangeState(new NetworkPatrolState());
                return;
            }

            float distanceToTarget = Vector3.Distance(_enemy.transform.position, _enemy.TargetPlayer.transform.position);

            // If the player moves away and out of attack range
            if (distanceToTarget > _enemy.AttackRange)
            {
                _enemy.StateMachine.ChangeState(new NetworkChaseState());
                return;
            }

            // ATTACK LOGIC (Cooldown management)
            if (Time.time >= _lastAttackTime + _enemy.AttackCooldown)
            {
                PerformAttack();
                _lastAttackTime = Time.time;
            }
        }

        private void PerformAttack()
        {
            // Look for the health system on the player NetworkObject
            var playerHealth = _enemy.TargetPlayer.GetComponent<NetworkHealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_enemy.AttackDamage);
                Debug.Log($"[SERVER] {_enemy.gameObject.name} hit the player for {_enemy.AttackDamage} damage.");
            }
        }

        public void Exit()
        {
            
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Attacking;
    }
}