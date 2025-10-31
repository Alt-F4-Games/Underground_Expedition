using UnityEngine;

namespace Enemy.States
{
    public class EnemyJumpAttackState : EnemyState
    {
        private Rigidbody rb;
        private bool isJumping;
        private bool isCharging;
        private Vector3 jumpTarget;

        private float chargeTimer;
        private float initialY;

        [Header("Jump Settings")]
        public float extraDistance = 2f;
        public float jumpHeight = 3f;
        public float jumpSpeed = 8f;
        public float pushForce = 5f;
        public float chargeTime = 1f;

        public EnemyJumpAttackState(EnemyAI enemy) : base(enemy) { }

        public override void Enter()
        {
            if (!enemy.TryGetComponent(out rb))
            {
                rb = enemy.gameObject.AddComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }

            enemy.agent.enabled = false;
            initialY = enemy.transform.position.y;
            chargeTimer = 0f;
            isCharging = true;
            isJumping = false;

            Debug.Log($"{enemy.name} comenzó a cargar su salto...");
        }

        public override void UpdateLogic()
        {
            if (isCharging)
            {
                chargeTimer += Time.deltaTime;
                if (chargeTimer >= chargeTime)
                {
                    StartJump();
                }
            }

            if (isJumping && rb.velocity.y <= 0)
            {
                if (Physics.Raycast(enemy.transform.position, Vector3.down, out RaycastHit hit, 1.1f))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        Land();
                    }
                }
            }
        }

        private void StartJump()
        {
            if (enemy.player == null)
            {
                stateMachine.ChangeState(new EnemyPatrolState(enemy));
                return;
            }

            isCharging = false;
            isJumping = true;

            Vector3 dir = (enemy.player.position - enemy.transform.position).normalized;
            jumpTarget = enemy.player.position + dir * extraDistance;
            
            Vector3 jumpDir = (jumpTarget - enemy.transform.position);
            jumpDir.y = 0;

            float horizontalDist = jumpDir.magnitude;
            float time = horizontalDist / jumpSpeed;
            float verticalVelocity = (2 * jumpHeight) / time;

            Vector3 velocity = jumpDir.normalized * jumpSpeed + Vector3.up * verticalVelocity;
            rb.velocity = velocity;

            Debug.Log($"{enemy.name} saltó hacia el jugador");
        }

        private void Land()
        {
            isJumping = false;
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;

            Debug.Log($"{enemy.name} aterrizó");
            
            Collider[] hits = Physics.OverlapSphere(enemy.transform.position, enemy.attackRange, enemy.playerMask);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    if (hit.TryGetComponent(out Rigidbody playerRb))
                    {
                        Vector3 pushDir = (hit.transform.position - enemy.transform.position).normalized;
                        playerRb.AddForce(pushDir * pushForce, ForceMode.Impulse);
                    }

                    if (hit.TryGetComponent(out IDamageable target))
                    {
                        target.TakeDamage(enemy.attackDamage);
                    }

                    Debug.Log("Golpeó al jugador al aterrizar");
                }
            }
            
            rb.isKinematic = false;
            enemy.agent.enabled = true;
            stateMachine.ChangeState(new EnemyChaseState(enemy));
        }

        public override void Exit()
        {
            rb.isKinematic = false;
            enemy.agent.enabled = true;
        }
    }
}
