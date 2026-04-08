using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Network.Enemies;

namespace Network.Spawn
{
    // Generic structure to group what to summon, amount, and patrol path
    [System.Serializable]
    public class SummonGroup
    {
        public NetworkPrefabRef Prefab;
        public int Amount = 1;
        public NetworkPatrolPath PatrolPath;
    }

    // Now inherits from SpawnPoints to reuse core spawning logic
    public class SummonPoint : SpawnPoints
    {
        [Header("Activation Settings")]
        [Tooltip("Detection radius to activate this summon point.")]
        public float ActivationRadius = 15f;
        public LayerMask PlayerLayer;
        
        [Tooltip("List of extra enemies to summon. Example: 2 Rats to Path A, 1 Skull to Path B.")]
        public List<SummonGroup> SummonGroups = new List<SummonGroup>();

        // Networked property that any Controller can read
        [Networked] public NetworkBool IsActive { get; private set; }

        // Overriding Spawned to prevent automatic spawning at the start of the game
        public override void Spawned()
        {
            if (!HasStateAuthority) return;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            // Detect if any player is within the activation radius
            Collider[] hits = Physics.OverlapSphere(transform.position, ActivationRadius, PlayerLayer);
            IsActive = hits.Length > 0;
        }

        public void TriggerSummon()
        {
            if (!HasStateAuthority) return;

            // trigger the base Spawn logic (the main prefab configured in SpawnPoints)
            base.Spawn(); 

            // trigger the extra SummonGroups specific to this point
            foreach (var group in SummonGroups) 
            {
                if (!group.Prefab.IsValid) continue;

                for (int i = 0; i < group.Amount; i++)
                {
                    // Use the offset inherited from SpawnPoints
                    Vector3 offset = new Vector3(Random.Range(-_offsetSpawn, _offsetSpawn), 0, Random.Range(-_offsetSpawn, _offsetSpawn));
                    Vector3 desiredPosition = transform.position + offset;

                    // NavMesh validation logic
                    if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
                    {
                        desiredPosition = hit.position; 
                    }

                    Runner.Spawn(group.Prefab, desiredPosition, transform.rotation, null, (runner, obj) => 
                    {
                        if (obj.TryGetComponent(out NetworkEnemyController enemy))
                        {
                            if (obj.TryGetComponent(out NavMeshAgent agent))
                            {
                                agent.enabled = true;
                            }
        
                            if (group.PatrolPath != null)
                            {
                                enemy.PatrolPath = group.PatrolPath;
                            }
                        }
                    });
                }
            }
            
            Debug.Log($"[SERVER] SummonPoint '{gameObject.name}' executed complex summon.");
        }
        
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return; 
            
            // Check if the Object is valid before accessing [Networked] properties
            if (Application.isPlaying && Object != null && Object.IsValid && IsActive)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            
            Gizmos.DrawSphere(transform.position, 0.3f);
        }

        private void OnDrawGizmosSelected()
        {
            // Check if the Object is valid before accessing [Networked] properties
            if (Application.isPlaying && Object != null && Object.IsValid && IsActive)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            }
            else
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            }
            
            Gizmos.DrawSphere(transform.position, ActivationRadius);
        }
    }
}