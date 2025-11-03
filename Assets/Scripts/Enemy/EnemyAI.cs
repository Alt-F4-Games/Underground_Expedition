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
        public NavMeshAgent agent;
        public Transform eyePoint;
        public PatrolPath patrolPath;
        public Transform player;

        [Header("Detection")]
        public float viewDistance = 10f;
        [Range(0f, 360f)] public float viewAngle = 120f;
        public LayerMask playerMask;
        public LayerMask obstacleMask;

        [Header("Attack Settings")]
        public float attackRange = 1.5f;
        public int attackDamage = 10;
        public float attackCooldown = 1.2f;

        [Header("Patrol Settings")]
        public float waypointTolerance = 0.5f;

        [HideInInspector] public EnemyStateMachine stateMachine;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            stateMachine = new EnemyStateMachine();
        }

        protected virtual void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            eyePoint ??= transform;
        }

        protected virtual void Update()
        {
            stateMachine.CurrentState?.UpdateLogic();
        }

        protected virtual void FixedUpdate()
        {
            stateMachine.CurrentState?.UpdatePhysics();
        }
        
        public virtual bool CanSeePlayer()
        {
            if (player == null)
                return false;

            Vector3 dir = player.position - eyePoint.position;
            float dist = dir.magnitude;

            if (dist > viewDistance)
                return false;

            float angle = Vector3.Angle(eyePoint.forward, dir.normalized);
            if (angle > viewAngle * 0.5f)
                return false;

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
            Collider[] hits = Physics.OverlapSphere(
                transform.position + transform.forward * 0.5f,
                attackRange,
                playerMask,
                QueryTriggerInteraction.Collide
            );

            if (hits.Length == 0)
                return;
            
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();

            foreach (var c in hits)
            {
                if (c.TryGetComponent(out IDamageable dmg))
                {
                    damaged.Add(dmg);
                }
                else if (c.GetComponentInParent<IDamageable>() is IDamageable parentDmg)
                {
                    damaged.Add(parentDmg);
                }
            }
            
            foreach (var target in damaged)
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"{name} hit {((MonoBehaviour)target).name} once for {attackDamage} damage!");
            }
        }

        public string CurrentStateName => stateMachine?.CurrentStateName ?? "None";

#if UNITY_EDITOR
        private void OnGUI()
        {
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

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

            Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * eyePoint.forward;
            Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * eyePoint.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + right * viewDistance);
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + left * viewDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}