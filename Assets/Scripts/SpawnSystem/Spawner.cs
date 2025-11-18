using UnityEngine;
using System.Collections.Generic;
using Enemy;

public class Spawner : MonoBehaviour
{
    [SerializeField] private SpawnDatabase database;
    private Dictionary<string, SpawnPoint> spawnPoints = new();

    private void Awake()
    {
        var points = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var p in points)
        {
            if (!spawnPoints.ContainsKey(p.spawnID))
                spawnPoints.Add(p.spawnID, p);
        }
    }

    public GameObject Spawn(string objectID, string spawnID)
    {
        var spawnable = database.GetSpawnableByID(objectID);
        if (spawnable == null)
        {
            Debug.LogWarning($"[Spawner] Object not found with ID: {objectID}");
            return null;
        }

        if (!spawnPoints.TryGetValue(spawnID, out var spawnPoint))
        {
            Debug.LogWarning($"[Spawner] Spawn point not found with ID: {spawnID}");
            return null;
        }

        GameObject obj = Instantiate(spawnable.prefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

        // 🧠 Configurar enemigos automáticamente
        if (obj.TryGetComponent(out EnemyAI enemy))
        {
            if (spawnable is SpawnableObjectEnemy enemyData)
            {
                // --- Detection ---
                if (enemyData.viewDistance > 0)
                    enemy.viewDistance = enemyData.viewDistance;
                if (enemyData.viewAngle > 0)
                    enemy.viewAngle = enemyData.viewAngle;

                // --- Attack ---
                if (enemyData.attackRange > 0)
                    enemy.attackRange = enemyData.attackRange;
                if (enemyData.attackDamage > 0)
                    enemy.attackDamage = enemyData.attackDamage;
                if (enemyData.attackCooldown > 0)
                    enemy.attackCooldown = enemyData.attackCooldown;

                // --- Patrol ---
                if (enemyData.waypointTolerance > 0)
                    enemy.waypointTolerance = enemyData.waypointTolerance;
            }

            // 🧩 Generar un PatrolPath dinámico si hay waypoints definidos
            if (spawnPoint.additionalWaypoints.Count > 0)
            {
                GameObject pathGO = new GameObject($"{spawnID}_DynamicPath");
                pathGO.transform.SetParent(transform);
                var path = pathGO.AddComponent<PatrolPath>();

                // El primer punto siempre es el spawn point
                path.Waypoints.Add(spawnPoint.transform);

                // Luego los adicionales
                foreach (var wp in spawnPoint.additionalWaypoints)
                {
                    if (wp != null)
                        path.Waypoints.Add(wp);
                }

                enemy.patrolPath = path;
            }
        }

        if (spawnable.lifeTime > 0)
            Destroy(obj, spawnable.lifeTime);

        return obj;
    }
}
