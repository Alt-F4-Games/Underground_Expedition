using Fusion;
using UnityEngine;
using Health;

namespace Network.Enemies.States
{
    /// <summary>
    /// State for performing a leap attack towards the target.
    /// Handles parabolic movement, rotation alignment, and contact damage during flight.
    /// </summary>
    public class NetworkJumpAttackState : INetworkState
    {
        private NetworkEnemyController _enemy;
        private Vector3 _startPos;
        private Vector3 _targetPos;
        
        private float _progress;
        private float _jumpDuration;
        private float _jumpHeight = 2f; // Peak height of the leap arc
        
        private float _jumpSpeed;
        private float _extraDistance;
        private int _attackDamage;
        private float _attackRadius;

        private Collider _enemyCollider;
        private bool _hasDealtDamage;

        public NetworkJumpAttackState(float jumpSpeed, float extraDistance, int attackDamage, float attackRadius)
        {
            _jumpSpeed = jumpSpeed;
            _extraDistance = extraDistance;
            _attackDamage = attackDamage;
            _attackRadius = attackRadius;
        }

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            _progress = 0f;
            _hasDealtDamage = false;
            
            // Disable NavMeshAgent to allow manual transform manipulation during the jump
            _enemy.Agent.enabled = false;

            // Ensure the main collider acts as a trigger to avoid physics jitter during flight
            _enemyCollider = _enemy.GetComponent<Collider>();
            if (_enemyCollider != null)
            {
                _enemyCollider.isTrigger = true; 
            }

            _startPos = _enemy.transform.position;

            // Calculate jump destination based on target position plus an overshoot distance
            if (_enemy.TargetPlayer != null)
            {
                Vector3 dirToPlayer = (_enemy.TargetPlayer.transform.position - _startPos).normalized;
                dirToPlayer.y = 0; 
                float distToPlayer = Vector3.Distance(_startPos, _enemy.TargetPlayer.transform.position);
                
                _targetPos = _startPos + dirToPlayer * (distToPlayer + _extraDistance);
            }
            else
            {
                // Fallback: Jump forward if target is lost upon entering the state
                _targetPos = _startPos + _enemy.transform.forward * 3f; 
            }

            // Determine timing based on distance and defined jump speed
            float totalDistance = Vector3.Distance(_startPos, _targetPos);
            _jumpDuration = totalDistance / Mathf.Max(0.1f, _jumpSpeed);
            
            Debug.Log($"[SERVER] {_enemy.gameObject.name} launched Jump Attack.");
        }

        public void Update()
        {
            if (_enemy.Object == null || !_enemy.Object.IsValid) return;

            // Advance progress based on Network Delta Time
            _progress += _enemy.Runner.DeltaTime / _jumpDuration;

            if (_progress >= 1f)
            {
                _progress = 1f;
                CompleteJump();
                return;
            }

            // POSITION LOGIC: Calculate parabolic arc using Lerp and Sine wave
            Vector3 currentPos = Vector3.Lerp(_startPos, _targetPos, _progress);
            currentPos.y += Mathf.Sin(_progress * Mathf.PI) * _jumpHeight;

            // ROTATION LOGIC: Look ahead towards the next point in the arc
            float lookAheadProgress = Mathf.Clamp01(_progress + 0.05f);
            Vector3 nextPos = Vector3.Lerp(_startPos, _targetPos, lookAheadProgress);
            nextPos.y += Mathf.Sin(lookAheadProgress * Mathf.PI) * _jumpHeight;

            _enemy.transform.position = currentPos;
            
            Vector3 lookDir = nextPos - currentPos;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, targetRotation, _enemy.Runner.DeltaTime * 15f);
            }
            
            // CONTACT LOGIC: Attempt to deal damage if the enemy hasn't hit a target yet
            if (!_hasDealtDamage && _progress > 0.15f)
            {
                CheckForDamageOnContact();
            }
        }

        // Checks for players within the attack radius during the leap
        private void CheckForDamageOnContact()
        {
            Collider[] hits = Physics.OverlapSphere(_enemy.transform.position, _attackRadius, _enemy.PlayerLayer);
            
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    var health = hit.GetComponentInParent<NetworkHealthSystem>();
                    
                    if (health != null)
                    {
                        health.TakeDamage(_attackDamage);
                        Debug.Log($"[SERVER] Mid-air impact! {_attackDamage} damage processed to {hit.gameObject.name}");
                        
                        _hasDealtDamage = true; 
                        break;
                    }
                }
            }
        }

        // Transition back to chase logic upon landing
        private void CompleteJump()
        {
            _enemy.StateMachine.ChangeState(_enemy.GetChaseState());
        }

        public void Exit()
        {
            // Ensure landing on a valid NavMesh point before reactivating physics
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(_enemy.transform.position, out hit, 4f, UnityEngine.AI.NavMesh.AllAreas))
            {
                _enemy.transform.position = hit.position;
            }

            // Restore physics and agent state
            if (_enemyCollider != null)
            {
                _enemyCollider.isTrigger = false;
            }

            // Clear residual physics forces
            if (_enemy.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            _enemy.Agent.enabled = true;
            
            // Sync NavMeshAgent with the final landing position
            if (_enemy.Agent.isOnNavMesh)
            {
                _enemy.Agent.Warp(_enemy.transform.position);
                _enemy.Agent.isStopped = false;
                _enemy.Agent.updatePosition = true;
                _enemy.Agent.updateRotation = true;
            }
            
            // Flatten rotation to ensure the enemy doesn't stay tilted after landing
            Vector3 euler = _enemy.transform.rotation.eulerAngles;
            _enemy.transform.rotation = Quaternion.Euler(0, euler.y, 0);
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Jumping;
    }
}