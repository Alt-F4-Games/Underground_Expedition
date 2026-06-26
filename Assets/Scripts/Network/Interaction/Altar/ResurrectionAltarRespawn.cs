using System;
using Fusion;
using UnityEngine;
using Network.Spawn;
using Network.Inventory; 
using Local.Inventory;
using Network.Items;

namespace Network.Interaction.Altar
{
    public class ResurrectionAltarRespawn : InteractableBase
    {
        [Header("Altar Requirements")]
        [Tooltip("The text ID (gameplayId) of the item required to activate the altar.")]
        [SerializeField] private string _requiredItemGameplayId;

        private PlayerRespawnPoint _respawnPoint;
        private int _cachedNetworkId = -1;
        
        public event Action Altar1Activated;
        
        private int RequiredNetworkId 
        {
            get 
            {
                if (_cachedNetworkId == -1 && ItemDatabase.Instance != null)
                {
                    _cachedNetworkId = ItemDatabase.Instance.GetNetworkId(_requiredItemGameplayId);
                }
                return _cachedNetworkId;
            }
        }

        private void Awake()
        {
            _respawnPoint = GetComponent<PlayerRespawnPoint>();

            if (_respawnPoint == null)
            {
                Debug.LogError($"No PlayerRespawnPoint found on {gameObject.name}");
            }
        }
        
        public override bool CanInteract(PlayerRef player)
        {
            if (_respawnPoint == null || _respawnPoint.WasActivated)
                return false;

            // If the database didn't load or the item doesn't exist, we block interaction
            if (RequiredNetworkId == -1) return false; 

            NetworkInventoryManager inventory = GetPlayerInventory(player);
            if (inventory == null || inventory.inventorySystem == null)
                return false;

            var slot = inventory.inventorySystem.GetSlotData(SlotType.Hotbar, inventory.SelectedHotbarIndex);
            
            // We compare using the numeric ID obtained from the ItemDatabase
            return slot.ItemId == RequiredNetworkId && slot.Quantity > 0;
        }
        
        public override string GetInteractPrompt()
        {
            if (NetworkInventoryManager.Local != null && RequiredNetworkId != -1)
            {
                var slot = NetworkInventoryManager.Local.inventorySystem.GetSlotData(SlotType.Hotbar, NetworkInventoryManager.Local.SelectedHotbarIndex);
                if (slot.ItemId != RequiredNetworkId || slot.Quantity <= 0)
                {
                    // We search for the real name of the item to display it in the UI (optional, looks more polished)
                    var itemData = ItemDatabase.Instance.GetItemByNetworkId(RequiredNetworkId);
                    string itemName = itemData != null ? itemData.itemName : _requiredItemGameplayId;
                    
                    return $"Requires {itemName} in hand";
                }
            }
            
            return _promptMessage;
        }
        
        public override void OnInteract(NetworkPlayerController player)
        {
            if (_respawnPoint == null || _respawnPoint.WasActivated || RequiredNetworkId == -1)
                return;

            var inventoryManager = player.GetComponent<NetworkInventoryManager>();
            if (inventoryManager == null || inventoryManager.inventorySystem == null)
                return;

            var currentSlot = inventoryManager.inventorySystem.GetSlotData(SlotType.Hotbar, inventoryManager.SelectedHotbarIndex);

            if (currentSlot.ItemId == RequiredNetworkId && currentSlot.Quantity > 0)
            {
                inventoryManager.inventorySystem.Server_TryRemoveItem(RequiredNetworkId, 1, SlotType.Hotbar);

                Debug.Log($"[Server] Altar {name} activated using {_requiredItemGameplayId} by {player.Object.InputAuthority}");
                RespawnManager.Instance.ActivatePoint(_respawnPoint);
                Altar1Activated?.Invoke();
            }
        }
        
        private NetworkInventoryManager GetPlayerInventory(PlayerRef player)
        {
            if (NetworkInventoryManager.Local != null && NetworkInventoryManager.Local.Object.InputAuthority == player)
                return NetworkInventoryManager.Local;

            if (Runner.TryGetPlayerObject(player, out NetworkObject playerObj))
                return playerObj.GetComponent<NetworkInventoryManager>();

            foreach (var controller in FindObjectsByType<NetworkPlayerController>(FindObjectsSortMode.None))
            {
                if (controller.Object.InputAuthority == player)
                    return controller.GetComponent<NetworkInventoryManager>();
            }

            return null;
        }
    }
}