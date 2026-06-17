using Fusion;
using UnityEngine;
using Network.Interaction;
using Network.Spawn;

namespace Network.Objects
{
    [RequireComponent(typeof(NetworkLootSpawner))]
    public class ChestLootInteractable : InteractableBase
    {
        [Header("Chest Settings")]
        [Tooltip("Text displayed when the player looks at a closed chest.")]
        [SerializeField] private string closedPrompt = "Open Chest";
        
        [Tooltip("Text displayed (or not) when the chest is already empty.")]
        [SerializeField] private string openedPrompt = "Empty";

        [Tooltip("Optional animator used to play the chest opening animation.")]
        [SerializeField] private Animator chestAnimator;

        // ============================================================
        // NETWORK STATE
        // ============================================================
        // OnChangedRender ensures that when the value changes to true,
        // all clients (including those joining later) execute OnChestOpened.
        [Networked, OnChangedRender(nameof(OnChestOpened))] 
        public NetworkBool IsOpened { get; set; }

        private NetworkLootSpawner _lootSpawner;

        public override void Spawned()
        {
            base.Spawned(); // Call the base implementation to initialize the visual Fresnel effect
            _lootSpawner = GetComponent<NetworkLootSpawner>();
        }

        // ============================================================
        // IINTERACTABLE IMPLEMENTATION
        // ============================================================

        public override string GetInteractPrompt()
        {
            return IsOpened ? openedPrompt : closedPrompt;
        }

        public override bool CanInteract(PlayerRef player)
        {
            // If the chest is already open, disable further interactions
            return !IsOpened;
        }

        /// <summary>
        /// Executes ONLY on the Server (State Authority) when a player completes the interaction.
        /// </summary>
        public override void OnInteract(NetworkPlayerController player)
        {
            // Double safety check on the server
            if (IsOpened) return;

            // Lock the chest across the network for all clients
            IsOpened = true;

            // Trigger the spawner to eject the loot in front of the chest
            if (_lootSpawner != null)
            {
                _lootSpawner.SpawnLootHere();
            }
        }

        // ============================================================
        // VISUALS (CLIENT-SIDE)
        // ============================================================

        /// <summary>
        /// Automatically triggered on all clients when IsOpened changes to true.
        /// </summary>
        private void OnChestOpened()
        {
            if (IsOpened && chestAnimator != null)
            {
                // Play the opening animation
                chestAnimator.SetTrigger("Open");
                
                // Note: no se como vas a hacer la animacion asi q si lo haces con un booleano usa esto:
                // chestAnimator.SetBool("IsOpen", true);
            }
        }
    }
}