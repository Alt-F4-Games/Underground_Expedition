/*
 * EnemyAI
 * Core enemy logic handling detection, movement, attacking, and state machine updates.
 *
 * Responsibilities:
 * - Maintain references (NavMeshAgent, player, patrol path)
 * - Perform player detection via FOV and raycast
 * - Handle attack logic and cooldown (external states manage timing)
 * - Update the current state machine state
 * - Draw debug gizmos and show current state in the editor
 *
 * Dependencies:
 * - Requires NavMeshAgent
 * - Uses EnemyStateMachine and EnemyState-derived classes
 * - Expects an IDamageable interface for player damage
 */

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Enemy.States.Base;

namespace Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("References")]
        public NavMeshAgent agent;          // Navigation component
        public Transform eyePoint;          // Point used for detection raycasts
        public PatrolPath patrolPath;       // Optional patrol path
        public Transform player;            // Player reference (auto-found)

        [Header("Detection")]
        public float viewDistance = 10f;    // Max detection range
        [Range(0f, 360f)] public float viewAngle = 120f;  // Field of view
        public LayerMask playerMask;        // Layer mask for player
        public LayerMask obstacleMask;      // Layer mask for obstacles

        [Header("Attack Settings")]
        public float attackRange = 1.5f;    // Radius for melee attack
        public int attackDamage = 10;       // Damage dealt per hit
        public float attackCooldown = 1.2f; // Time between attacks

        [Header("Patrol Settings")]
        public float waypointTolerance = 0.5f;

        // The state machine controlling enemy behavior
        [HideInInspector] public EnemyStateMachine stateMachine;

        protected virtual void Awake()
        {
            // Initialize dependencies
            agent = GetComponent<NavMeshAgent>();
            stateMachine = new EnemyStateMachine();
        }

        protected virtual void Start()
        {
            // Auto-assign player if tagged
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            // Fallback eye point to enemy transform if not set
            eyePoint ??= transform;
        }

        protected virtual void Update()
        {
            // Logic update for current state
            stateMachine.CurrentState?.UpdateLogic();
        }

        protected virtual void FixedUpdate()
        {
            // Physics update for current state
            stateMachine.CurrentState?.UpdatePhysics();
        }

        public virtual bool CanSeePlayer()
        {
            // If no player exists, detection is impossible
            if (player == null)
                return false;

            // Direction and distance to the player
            Vector3 dir = player.position - eyePoint.position;
            float dist = dir.magnitude;

            // Check max view distance
            if (dist > viewDistance)
                return false;

            // FOV check
            float angle = Vector3.Angle(eyePoint.forward, dir.normalized);
            if (angle > viewAngle * 0.5f)
                return false;

            // Raycast to detect obstruction
            if (Physics.Raycast(eyePoint.position, dir.normalized, out RaycastHit hit, viewDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.collider.CompareTag("Player");
            }

            return false;
        }

        public virtual float DistanceToPlayer()
        {
            if (player == null) return Mathf.Infinity;
            return Vector3.Distance(transform.position, player.position);
        }

        public virtual void TryAttack()
        {
            // Overlap sphere to detect targets in front of the enemy
            Collider[] hits = Physics.OverlapSphere(
                transform.position + transform.forward * 0.5f,
                attackRange,
                playerMask,
                QueryTriggerInteraction.Collide
            );

            if (hits.Length == 0)
                return;

            // Avoid hitting the same target twice
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();

            foreach (var c in hits)
            {
                // Search for IDamageable on collider or parent
                if (c.TryGetComponent(out IDamageable dmg))
                {
                    damaged.Add(dmg);
                }
                else if (c.GetComponentInParent<IDamageable>() is IDamageable parentDmg)
                {
                    damaged.Add(parentDmg);
                }
            }

            // Apply damage once per target
            foreach (var target in damaged)
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"{name} hit {((MonoBehaviour)target).name} once for {attackDamage} damage!");
            }
        }

        // Exposes current state name for debugging
        public string CurrentStateName => stateMachine?.CurrentStateName ?? "None";

#if UNITY_EDITOR
        private void OnGUI()
        {
            // Draws the current state above the enemy in the editor/game view
            if (!Application.isPlaying) return;
            if (Camera.current == null) return;

            var screenPos = Camera.current.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            if (screenPos.z > 0)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    normal = { textColor = Color.white }
                };
                Vector2 size = style.CalcSize(new GUIContent(CurrentStateName));
                GUI.Label(new Rect(screenPos.x - size.x / 2, Screen.height - screenPos.y, size.x, size.y), CurrentStateName, style);
            }
        }
#endif

        protected virtual void OnDrawGizmosSelected()
        {
            if (eyePoint == null) eyePoint = transform;

            // Draw detection radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

            // Draw field of view lines
            Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * eyePoint.forward;
            Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * eyePoint.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + right * viewDistance);
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + left * viewDistance);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
