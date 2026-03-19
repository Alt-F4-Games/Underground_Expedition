using UnityEngine;
using Enemy;

[CreateAssetMenu(menuName = "Spawn System/Spawnable Enemy", fileName = "NewSpawnableEnemy")]
public class SpawnableObjectEnemy : SpawnableObject
{
    [Header("Enemy Overrides (0 or null = use prefab defaults)")]

    [Header("Detection")]
    [Tooltip("0 = use prefab value")]
    public float viewDistance = 0f;
    [Range(0f, 360f)] public float viewAngle = 0f;

    [Header("Attack Settings")]
    [Tooltip("0 = use prefab value")]
    public float attackRange = 0f;
    [Tooltip("0 = use prefab value")]
    public int attackDamage = 0;
    [Tooltip("0 = use prefab value")]
    public float attackCooldown = 0f;

    [Header("Patrol Settings")]
    [Tooltip("0 = use prefab value")]
    public float waypointTolerance = 0f;

    [Header("Optional Overrides")]
    [Tooltip("If assigned, will override the SpawnPoint's patrol path")]
    public PatrolPath defaultPatrolPath = null;
}
