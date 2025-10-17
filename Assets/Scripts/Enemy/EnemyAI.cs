using UnityEngine;
using UnityEngine.AI;

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

    [Header("Attack")]
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;

    [Header("Patrol")]
    public float waypointTolerance = 0.5f;

    [HideInInspector] public EnemyStateMachine stateMachine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateMachine = new EnemyStateMachine();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        eyePoint ??= transform;

        // Crear los estados y establecer el inicial
        var patrol = new EnemyPatrolState(this);
        var chase = new EnemyChaseState(this);
        var attack = new EnemyAttackState(this);

        stateMachine.Initialize(patrol);

        // Registrar posibles transiciones (opcional)
        // No necesario si lo manejas desde los estados directamente
    }

    private void Update()
    {
        stateMachine.CurrentState?.UpdateLogic();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState?.UpdatePhysics();
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dir = player.position - eyePoint.position;
        float dist = dir.magnitude;
        if (dist > viewDistance)
        {
            return false;
        }

        float angle = Vector3.Angle(eyePoint.forward, dir.normalized);
        if (angle > viewAngle * 0.5f)
        {
            return false;
        }

        if (Physics.Raycast(eyePoint.position, dir.normalized, out RaycastHit hit, viewDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Raycast golpeó: " + hit.collider.name);
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Jugador detectado correctamente");
                return true;
            }
        }

        return false;
    }   

    private void OnDrawGizmosSelected()
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