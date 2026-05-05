using UnityEngine;
using Health;

namespace Network.Enemies
{
    // State for dealing damage when close to the target
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
            // TRANSITION: Player disappears or dies -> return to patrol
            if (_enemy.TargetPlayer == null)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetPatrolState());
                return;
            }

            float distanceToTarget = Vector3.Distance(_enemy.transform.position, _enemy.TargetPlayer.transform.position);

            // TRANSITION: Player moves away from attack range -> resume chasing
            if (distanceToTarget > _enemy.AttackRange)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetChaseState());
                return;
            }

            // ATTACK LOGIC: Apply damage considering the cooldown timer
            if (Time.time >= _lastAttackTime + _enemy.AttackCooldown)
            {
                PerformAttack();
                _lastAttackTime = Time.time;
            }
        }

        private void PerformAttack()
        {
            // Look for the health system on the target and apply damage
            var playerHealth = _enemy.TargetPlayer.GetComponent<NetworkHealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_enemy.AttackDamage);
                Debug.Log($"[SERVER] {_enemy.gameObject.name} hit the player for {_enemy.AttackDamage} damage.");
            }
        }

        public void Exit()
        {
            // Logic when exiting attack state (empty for now)
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Attacking;
    }
}