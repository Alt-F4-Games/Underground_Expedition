using Fusion;
using UnityEngine;

namespace Network.Interaction.Test
{
    /// <summary>
    /// A simple test object to verify the interaction system.
    /// It toggles its color across all clients when interacted with.
    /// </summary>
    public class TestInteractable : InteractableBase
    {
        [Header("Test Visuals")]
        [Tooltip("The renderer to change color.")]
        [SerializeField] private MeshRenderer _renderer;
        
        [SerializeField] private Color _activeColor = Color.green;
        [SerializeField] private Color _inactiveColor = Color.red;

        // Synchronized state. When changed by the server, it triggers OnStateChanged on all clients.
        [Networked, OnChangedRender(nameof(OnStateChanged))]
        public NetworkBool IsToggled { get; set; }

        public override void Spawned()
        {
            // Set initial color when spawned
            UpdateColor();
        }

        // ============================================================
        // EXECUTION (Server / State Authority Only)
        // ============================================================

        public override void OnInteract(NetworkPlayerController player)
        {
            Debug.Log($"[SERVER] Player {player.Object.InputAuthority} interacted with {gameObject.name}!");
            
            // Toggle the networked state
            IsToggled = !IsToggled;
        }

        // ============================================================
        // VISUALS (Client & Server)
        // ============================================================

        private void OnStateChanged()
        {
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (_renderer != null)
            {
                _renderer.material.color = IsToggled ? _activeColor : _inactiveColor;
            }
        }
    }
}