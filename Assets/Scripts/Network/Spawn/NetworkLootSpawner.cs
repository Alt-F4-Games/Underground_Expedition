using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Network.Inventory;
using Network.Items;
using Random = UnityEngine.Random;

namespace Network.Spawn
{
    public enum LootSpawnMode
    {
        Radial,      // For enemies: Drops items in a circle around the origin point.
        Directional  // For chests: Drops items forward based on a reference transform.
    }

    [Serializable]
    public struct LootEntry
    {
        [Tooltip("The item's Database ID. Example: 'mat_teeth' or 'cons_potion'")]
        public string itemGameplayId;
        
        [Tooltip("Minimum number of physical objects that will spawn in the world (inclusive).")]
        public int minDrops;
        
        [Tooltip("Maximum number of physical objects that will spawn in the world (inclusive).")]
        public int maxDrops;
        
        [Tooltip("Amount granted to the inventory per physical drop. Example: 1")]
        public int amountPerDrop;

        [Tooltip("Probability of this item dropping (0.0 to 1.0). 1.0 = 100%")]
        [Range(0f, 1f)]
        public float dropChance;
    }

    /// <summary>
    /// Modular spawner for dropping items into the world.
    /// Supports Radial mode (Enemies) and Directional mode (Chests).
    /// </summary>
    public class NetworkLootSpawner : NetworkBehaviour
    {
        [Header("Loot Table Configuration")]
        [Tooltip("List of possible items that this object can drop.")]
        [SerializeField] private List<LootEntry> lootTable = new List<LootEntry>();

        [Header("Spawn Mode")]
        [SerializeField] private LootSpawnMode spawnMode = LootSpawnMode.Radial;
        
        [Header("Radial Settings (Enemies)")]
        [SerializeField] private float dropRadius = 1.5f;
        
        [Header("Directional Settings (Chests)")]
        [Tooltip("Reference transform to determine the 'forward' direction (usually the chest itself).")]
        [SerializeField] private Transform directionalReference;
        [Tooltip("Distance forward where the items will land.")]
        [SerializeField] private float forwardDistance = 1.5f;
        [Tooltip("Lateral spread to prevent items from stacking exactly on top of each other.")]
        [SerializeField] private float sideSpread = 1f;

        [Header("General Settings")]
        [Tooltip("Height from which items fall, allowing the NetworkWorldItem raycast to detect the ground properly.")]
        [SerializeField] private float dropHeightOffset = 1.0f;

        /// <summary>
        /// Called by the server to generate loot at the specified position.
        /// </summary>
        public void SpawnLoot(Vector3 originPoint)
        {
            if (!HasStateAuthority) return;

            NetworkPrefabRef globalWorldItemPrefab = ItemDatabase.Instance.WorldItemPrefab;
            if (!globalWorldItemPrefab.IsValid) return;

            // Iterate through the entire loot table
            foreach (var entry in lootTable)
            {
                // Roll the dice to determine if this specific item drops
                if (Random.value > entry.dropChance) continue;

                int networkId = ItemDatabase.Instance.GetNetworkId(entry.itemGameplayId);
                if (networkId <= 0) continue;

                // Calculate the exact amount of physical objects to instantiate based on Min/Max
                int dropsToSpawn = Random.Range(entry.minDrops, entry.maxDrops + 1);

                for (int i = 0; i < dropsToSpawn; i++)
                {
                    Vector3 dropPos = CalculateDropPosition(originPoint);
                    
                    Runner.Spawn(globalWorldItemPrefab, dropPos, Quaternion.identity, null, (runner, obj) =>
                    {
                        if (obj.TryGetComponent(out NetworkWorldItem item))
                        {
                            item.Init(networkId, entry.amountPerDrop);
                        }
                    });
                }
            }
        }

        // Calculates the final mid-air position (the item's internal raycast will snap it to the ground later)
        private Vector3 CalculateDropPosition(Vector3 origin)
        {
            if (spawnMode == LootSpawnMode.Radial)
            {
                // Random point within a circle around the origin
                Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
                return origin + new Vector3(randomCircle.x, dropHeightOffset, randomCircle.y);
            }
            else 
            {
                // Forward projection based on the reference transform
                Vector3 forwardDir = directionalReference != null ? directionalReference.forward : transform.forward;
                Vector3 rightDir = directionalReference != null ? directionalReference.right : transform.right;

                // Add slight randomization to forward and lateral placement
                float randomSide = Random.Range(-sideSpread, sideSpread);
                float randomForward = Random.Range(forwardDistance * 0.5f, forwardDistance * 1.5f);

                return origin + (forwardDir * randomForward) + (rightDir * randomSide) + new Vector3(0, dropHeightOffset, 0);
            }
        }

        // Convenience method to drop loot exactly at this object's position
        public void SpawnLootHere()
        {
            SpawnLoot(transform.position);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            
            Vector3 basePosition = transform.position + new Vector3(0, dropHeightOffset, 0);

            if (spawnMode == LootSpawnMode.Radial)
            {
                Gizmos.DrawWireSphere(basePosition, dropRadius);
            }
            else if (spawnMode == LootSpawnMode.Directional)
            {
                Transform refTransform = directionalReference != null ? directionalReference : transform;
                Gizmos.matrix = Matrix4x4.TRS(basePosition, refTransform.rotation, Vector3.one);
                Vector3 center = new Vector3(0, 0, forwardDistance);
                Vector3 size = new Vector3(sideSpread * 2f, 0.1f, forwardDistance);
                
                Gizmos.DrawWireCube(center, size);
            }
        }
#endif
    }
}