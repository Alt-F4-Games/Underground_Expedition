using Fusion;
using UnityEngine;

namespace Network.Spawn
{
    // Inherit directly from SpawnPoints instead of NetworkBehaviour to reuse spawning logic
    public class EnemyProximitySpawner : SpawnPoints
    {
        [Header("Spawn Configuration")]
        [Tooltip("Where the enemy will appear. If left empty, it will spawn at this object's position.")]
        public Transform CustomSpawnPoint;

        [Header("Activation Settings")]
        [Tooltip("How close a player needs to be to trigger the spawn.")]
        public float ActivationRadius = 5f;
        
        [Tooltip("The layer assigned to your players.")]
        public LayerMask PlayerLayer;

        // Networked boolean to ensure it only spawns once across all clients
        [Networked] public NetworkBool HasSpawned { get; private set; }

        public override void Spawned()
        {
            // OVERRIDE the Spawned method so it DOES NOT automatically spawn when the scene starts.
            if (!HasStateAuthority) return;
        }

        public override void FixedUpdateNetwork()
        {
            // Only the server checks for player distance
            if (!HasStateAuthority || HasSpawned) return;

            // Detect if any player enters the activation radius
            Collider[] hits = Physics.OverlapSphere(transform.position, ActivationRadius, PlayerLayer);
            
            if (hits.Length > 0)
            {
                TriggerSpawn();
            }
        }

        private void TriggerSpawn()
        {
            HasSpawned = true;

            // Store original transform data to restore it after spawning
            Vector3 originalPos = transform.position;
            Quaternion originalRot = transform.rotation;

            // If a custom point is assigned, move the spawner there temporarily
            if (CustomSpawnPoint != null)
            {
                transform.position = CustomSpawnPoint.position;
                transform.rotation = CustomSpawnPoint.rotation;
            }

            // Call the base logic that handles NavMesh validation, Agent setup, and PatrolPath assignment
            base.Spawn();

            // Revert transform to its original state (e.g., back to the statue position)
            if (CustomSpawnPoint != null)
            {
                transform.position = originalPos;
                transform.rotation = originalRot;
            }

            Debug.Log($"[SERVER] ProximitySpawner '{gameObject.name}' triggered by player! Executed base Spawn at {(CustomSpawnPoint != null ? CustomSpawnPoint.name : "default position")}.");
        }

        private void OnDrawGizmosSelected()
        {
            // Draw the activation radius for visual configuration in the Editor
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, ActivationRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ActivationRadius);

            // Draw a visual link and the target spawn point
            if (CustomSpawnPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position, CustomSpawnPoint.position);
                Gizmos.DrawWireCube(CustomSpawnPoint.position, Vector3.one * 0.5f);
            }
        }
    }
}