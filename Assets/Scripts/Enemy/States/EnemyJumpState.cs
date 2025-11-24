using UnityEngine;

namespace Enemy.States.Base
{
    /*
        EnemyJumpState.cs
        Base jump state used by enemies that perform a charged jump attack.
        Handles:
        - Charging phase before the jump
        - Procedural jump velocity calculation
        - Gravity-based jumping using Rigidbody
        - Landing detection
        - Disabling NavMeshAgent during jump

        RatJumpState and other jump-based enemies extend this class.
    */

    [RequireComponent(typeof(Collider))]
    public class EnemyJumpState : EnemyState
    {
        protected Rigidbody rb;
        protected bool isCharging;       // Charging before the jump
        protected bool isJumping;        // True once jump velocity is applied
        protected float chargeTimer;     // Timer used to count the charge duration
        protected Vector3 targetPosition; // Where the jump is aimed at

        // Jump configuration (overridden by child classes)
        public float extraDistance = 2f;
        public float jumpHeight = 3f;
        public float jumpSpeed = 8f;
        public float chargeTime = 1f;

        // Constants
        protected const float MIN_HORIZONTAL_DIST = 0.5f;
        protected const float MAX_VERTICAL_VELOCITY = 20f;

        public EnemyJumpState(EnemyAI enemy) : base(enemy) { }

        public override void Enter()
        {
            // Get or add a Rigidbody for physics-based jumping
            if (!enemy.TryGetComponent(out rb))
                rb = enemy.gameObject.AddComponent<Rigidbody>();
            
            rb.useGravity = true;
            rb.isKinematic = false;

            // Reset movement to avoid leftover forces
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Disable navmesh agent so it doesn’t override physics
            enemy.agent.enabled = false;

            // Start charging phase
            isCharging = true;
            isJumping = false;
            chargeTimer = 0f;

            // Store target position (usually the player)
            targetPosition = enemy.player != null
                ? enemy.player.position
                : enemy.transform.position + enemy.transform.forward * 2f;
        }

        public override void UpdateLogic()
        {
            // Charging phase until timer reaches chargeTime
            if (isCharging)
            {
                chargeTimer += Time.deltaTime;
                if (chargeTimer >= chargeTime)
                    StartJump();

                return;
            }

            // While jumping and falling, check for landing
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
            // End charge phase, begin jump
            isCharging = false;
            isJumping = true;

            // Direction toward the predicted jump endpoint
            Vector3 dir = (targetPosition - enemy.transform.position).normalized;

            // Slight overshoot to simulate aggressive leap
            Vector3 jumpTarget = targetPosition + dir * extraDistance;

            // Horizontal direction only
            Vector3 horizontal = jumpTarget - enemy.transform.position;
            horizontal.y = 0;

            float horizontalDistance = Mathf.Max(MIN_HORIZONTAL_DIST, horizontal.magnitude);

            // Time to cover that distance based on movement speed
            float time = horizontalDistance / jumpSpeed;

            // Compute required upward velocity to reach jumpHeight
            float verticalVelocity = Mathf.Clamp(
                (2 * jumpHeight) / time,
                0f,
                MAX_VERTICAL_VELOCITY
            );

            // Final jump velocity
            Vector3 velocity = horizontal.normalized * jumpSpeed + Vector3.up * verticalVelocity;

            rb.linearVelocity = velocity;
        }

        protected virtual void Land()
        {
            // Stop jump motion
            isJumping = false;
            rb.linearVelocity = Vector3.zero;

            // Re-enable NavMeshAgent and sync position
            enemy.agent.enabled = true;
            enemy.agent.Warp(enemy.transform.position);
        }

        public override void Exit()
        {
            // Reset any momentum when leaving the state
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
