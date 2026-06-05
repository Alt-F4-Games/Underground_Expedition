using System.Collections.Generic;
using Network.Enemies;
using UnityEngine;

namespace Enemies
{
    [CreateAssetMenu(menuName = "Enemies/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        public static EnemyDatabase Instance { get; private set; }

        [SerializeField]
        private List<EnemySO> enemies = new();

        private Dictionary<string, EnemySO> _lookup = new();

        public void Initialize()
        {
            Instance = this;

            _lookup.Clear();

            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    continue;

                if (string.IsNullOrWhiteSpace(enemy.enemyId))
                {
                    Debug.LogWarning(
                        $"[EnemyDatabase] Enemy {enemy.name} has empty ID");

                    continue;
                }

                if (_lookup.ContainsKey(enemy.enemyId))
                {
                    Debug.LogWarning(
                        $"[EnemyDatabase] Duplicate enemy ID: {enemy.enemyId}");

                    continue;
                }

                _lookup.Add(enemy.enemyId, enemy);
            }

            Debug.Log(
                $"[EnemyDatabase] Initialized with {_lookup.Count} enemies.");
        }

        public EnemySO GetEnemyById(string enemyId)
        {
            return _lookup.GetValueOrDefault(enemyId);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadDatabase()
        {
            var db = Resources.Load<EnemyDatabase>("EnemyDatabase");

            if (db != null)
            {
                db.Initialize();
            }
            else
            {
                Debug.LogWarning(
                    "[EnemyDatabase] No database found in Resources.");
            }
        }
    }
}