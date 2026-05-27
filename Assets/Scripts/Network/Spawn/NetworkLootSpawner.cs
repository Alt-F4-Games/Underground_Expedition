using Fusion;
using UnityEngine;
using Network.Inventory;
using Network.Items;

namespace Network.Spawn
{
    /// <summary>
    /// Modular component used to drop items into the world. 
    /// Can be used by Enemies upon death or by Interactable Objects (Chests) when opened.
    /// </summary>
    public class NetworkLootSpawner : NetworkBehaviour
    {
        [Header("Loot Configuration")]
        [Tooltip("The item's ID (GameplayId). Example: 'obj_canteen'")]
        [SerializeField] private string itemGameplayId;
        
        [Tooltip("Amount granted by EACH physical dropped object. Example: 1")]
        [SerializeField] private int amountPerDrop = 1;
        
        [Tooltip("How many physical objects will be dropped into the world. Example: 3 (3 canteens will drop)")]
        [SerializeField] private int numberOfDrops = 1;

        [Header("Drop Area Configuration")]
        [Tooltip("Scatter radius around the central point")]
        [SerializeField] private float dropRadius = 1.5f;
        
        [Tooltip("Height from which items fall to create an 'explosion' effect")]
        [SerializeField] private float dropHeightOffset = 1.0f;

        /// <summary>
        /// Call this method from the server when the object should drop loot.
        /// </summary>
        public void SpawnLoot(Vector3 originPoint)
        {
            if (!HasStateAuthority) return;

            NetworkPrefabRef globalWorldItemPrefab = ItemDatabase.Instance.WorldItemPrefab;
            if (!globalWorldItemPrefab.IsValid)
            {
                Debug.LogWarning($"[LootSpawner] WorldItem prefab is not assigned in the ItemDatabase.");
                return;
            }

            int networkId = ItemDatabase.Instance.GetNetworkId(itemGameplayId);
            if (networkId <= 0)
            {
                Debug.LogWarning($"[LootSpawner] GameplayID '{itemGameplayId}' is invalid or was not found in the ItemDatabase.");
                return;
            }

            // Spawn the requested number of objects
            for (int i = 0; i < numberOfDrops; i++)
            {
                // Calculate a random point inside a circle so items don't stack on top of each other
                Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
                Vector3 dropPos = originPoint + new Vector3(randomCircle.x, dropHeightOffset, randomCircle.y);

                Runner.Spawn(globalWorldItemPrefab, dropPos, Quaternion.identity, null, (runner, obj) =>
                {
                    if (obj.TryGetComponent(out NetworkWorldItem item))
                    {
                        item.Init(networkId, amountPerDrop);
                    }
                });
            }
        }

        // Convenience method if you want the loot to spawn exactly where this object is located
        public void SpawnLootHere()
        {
            SpawnLoot(transform.position);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, dropRadius);
        }
#endif
    }
}