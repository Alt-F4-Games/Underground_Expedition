using Fusion;
using UnityEngine;
using Network.Inventory;
using Network.Interaction;
using Network.Items;

namespace Network.Inventory
{
    /// <summary>
    /// Represents a physical item in the world.
    /// Inherits from InteractableBase to be detected by the player's scanner and use the Fresnel effect.
    /// Uses the ItemDatabase to retrieve the 3D model based on its NetworkId.
    /// </summary>
    public class NetworkWorldItem : InteractableBase 
    {
        // -----------------------------
        //  NETWORKED PROPERTIES
        // -----------------------------
        [Networked] public int ItemId { get; set; }
        [Networked] public int Quantity { get; set; }

        // -----------------------------
        //  REFERENCES
        // -----------------------------
        [Tooltip("Child empty object where the item's 3D model will be instantiated.")]
        [SerializeField] private Transform visualContainer;

        [Header("Ground Placement")]
        [Tooltip("Layer(s) representing the ground used to align the object.")]
        [SerializeField] private LayerMask groundLayer;
        
        [Tooltip("Maximum downward distance used to search for the ground.")]
        [SerializeField] private float groundCheckDistance = 10f;
        
        [Tooltip("Height offset used to prevent the model from clipping into the floor.")]
        [SerializeField] private float yOffset = 0.05f;

        // -----------------------------
        //  LOCAL STATE
        // -----------------------------
        private int _currentVisualId = -1;
        private bool _isPickupRequested = false;

        // ============================================================
        //  INITIALIZATION & INTERACTION
        // ============================================================
        
        public void Init(int id, int qty)
        {
            ItemId = id;
            Quantity = qty;
            _isPickupRequested = false;

            AlignWithGround();
        }

        private void AlignWithGround()
        {
            // Cast the ray slightly above the center in case the spawner created it partially underground
            Vector3 rayStart = transform.position + (Vector3.up * 1f);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
            {
                // Position the object at the hit point plus a slight offset
                transform.position = hit.point + (hit.normal * yOffset);

                // Align the object's vertical axis (Y) with the surface normal.
                // Also apply a random rotation on its own axis so dropped items look more natural.
                float randomYaw = Random.Range(0f, 360f);
                Quaternion randomRotation = Quaternion.Euler(0, randomYaw, 0);
                
                transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * randomRotation;
            }
            else
            {
                Debug.LogWarning($"[NetworkWorldItem] {gameObject.name} could not find the ground. Make sure the correct layer is configured.");
            }
        }

        public override void Spawned()
        {
            base.Spawned();
            _isPickupRequested = false;
            UpdateVisuals();
        }

        // --- IINTERACTABLE IMPLEMENTATION ---
        public override string GetInteractPrompt()
        {
            var itemSO = ItemDatabase.Instance.GetItemByNetworkId(ItemId);
            string name = itemSO != null ? itemSO.itemName : "Unknown Item";
            return $"Pick up {name} x{Quantity}";
        }

        public override bool CanInteract(PlayerRef player)
        {
            // Prevent multiple players or repeated clicks from attempting to pick it up simultaneously
            return !_isPickupRequested;
        }

        /// <summary>
        /// Executes on the SERVER when the player presses the interaction key (E).
        /// </summary>
        public override void OnInteract(NetworkPlayerController player)
        {
            if (_isPickupRequested) return;

            // Retrieve the inventory manager from the interacting player
            if (player.TryGetComponent(out NetworkInventoryManager inventoryManager))
            {
                _isPickupRequested = true;
                inventoryManager.RequestPickupItem(this);
            }
        }

        // ============================================================
        //  VISUALS (Using ItemDatabase)
        // ============================================================

        public override void Render()
        {
            // If the ID changes through the network (or initializes late for a joining client), update the model.
            if (ItemId != _currentVisualId)
            {
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (visualContainer == null) return;

            // Clear existing visuals (in case the ID changed)
            foreach (Transform child in visualContainer)
            {
                Destroy(child.gameObject);
            }

            // Request the visual prefab (equipPrefab) from the Database using the NetworkId
            GameObject modelPrefab = ItemDatabase.Instance.GetEquipPrefabByNetworkId(ItemId);
            
            if (modelPrefab != null)
            {
                Instantiate(modelPrefab, visualContainer);
                _currentVisualId = ItemId;
                
                base.Spawned(); 
            }
            else
            {
                Debug.LogWarning($"[NetworkWorldItem] No visual prefab found for ID {ItemId} in ItemDatabase.");
            }
        }

        /// <summary>
        /// Safety method used if the server denies the pickup request (e.g. inventory full).
        /// The manager calls this to re-enable interaction.
        /// </summary>
        public void ResetPickupRequest()
        {
            _isPickupRequested = false;
        }
    }
}