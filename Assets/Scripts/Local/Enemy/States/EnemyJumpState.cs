using UnityEngine;

namespace Enemy.States.Base
{
    [RequireComponent(typeof(Collider))]
    public class EnemyJumpState : EnemyState
    {
        protected Rigidbody rb;
        protected bool isCharging;
        protected bool isJumping;
        protected float chargeTimer;
        protected Vector3 targetPosition;

        public float extraDistance = 2f;
        public float jumpHeight = 3f;
        public float jumpSpeed = 8f;
        public float chargeTime = 1f;
        
        protected const float MIN_HORIZONTAL_DIST = 0.5f;
        protected const float MAX_VERTICAL_VELOCITY = 20f;

        public EnemyJumpState(EnemyAI enemy) : base(enemy) { }

        public override void Enter()
        {
            if (!enemy.TryGetComponent(out rb))
                rb = enemy.gameObject.AddComponent<Rigidbody>();
            
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            enemy.agent.enabled = false;

            isCharging = true;
            isJumping = false;
            chargeTimer = 0f;
            
            targetPosition = enemy.player != null
                ? enemy.player.position
                : enemy.transform.position + enemy.transform.forward * 2f;
        }

        public override void UpdateLogic()
        {
            if (isCharging)
            {
                chargeTimer += Time.deltaTime;
                if (chargeTimer >= chargeTime)
                    StartJump();
                return;
            }

            if (isJumping && rb.linearVelocity.y <= 0)
            {
                if (Physics.Raycast(enemy.transform.position, Vector3.down, out RaycastHit hit, 1.2f))
                {
                    if (hit.collider.CompareTag("Ground"))
                        Land();
                }
            }
        }

        protected virtual void StartJump()
        {
            isCharging = false;
            isJumping = true;

            Vector3 dir = (targetPosition - enemy.transform.position).normalized;
            Vector3 jumpTarget = targetPosition + dir * extraDistance;

            Vector3 horizontal = jumpTarget - enemy.transform.position;
            horizontal.y = 0;

            float horizontalDistance = Mathf.Max(MIN_HORIZONTAL_DIST, horizontal.magnitude);
            float time = horizontalDistance / jumpSpeed;
            float verticalVelocity = Mathf.Clamp((2 * jumpHeight) / time, 0f, MAX_VERTICAL_VELOCITY);

            Vector3 velocity = horizontal.normalized * jumpSpeed + Vector3.up * verticalVelocity;
            rb.linearVelocity = velocity;
        }

        protected virtual void Land()
        {
            isJumping = false;
            rb.linearVelocity = Vector3.zero;
            enemy.agent.enabled = true;
            enemy.agent.Warp(enemy.transform.position);
        }

        public override void Exit()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
