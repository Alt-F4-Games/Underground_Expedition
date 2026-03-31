using Fusion;
using UnityEngine;
using Health; 

namespace Network.Player.DebugUI
{
    /// <summary>
    /// Debug utility to visualize player health in real-time using Unity's Legacy GUI.
    /// Internal tool used for testing synchronization and health logic during development.
    /// </summary>
    public class PlayerHealthDebug : NetworkBehaviour
    {
        [Header("References")]
        private NetworkPlayerHealth _healthComponent; 

        public override void Spawned()
        {
            // Cache the health component attached to this networked prefab
            _healthComponent = GetComponent<NetworkPlayerHealth>();
        }

        /// <summary>
        /// DEBUG ONLY: Renders a health bar overlay on the local client's screen.
        /// This method is primarily used for rapid prototyping and state verification.
        /// </summary>
        private void OnGUI()
        {
            // Safety checks for Fusion's Object validity and component reference
            if (_healthComponent == null || Object == null || !Object.IsValid) return;

            // Display the UI only for the local player who has Input Authority
            if (HasInputAuthority)
            {
                // UI Layout settings (Position and Dimensions)
                Rect rect = new Rect(20, 20, 250, 45);
                
                // Visual style configuration for the debug box
                GUIStyle style = new GUIStyle(GUI.skin.box);
                style.fontSize = 24;
                style.fontStyle = FontStyle.Bold;
                
                // DYNAMICS: Change text color to red if health drops below 40% for visual feedback
                float healthPercent = (float)_healthComponent.CurrentHealth / _healthComponent.MaxHealth;
                style.normal.textColor = healthPercent > 0.4f ? Color.green : Color.red;

                // Draw the debug label with current health vs max health
                GUI.Box(rect, $"HP: {_healthComponent.CurrentHealth} / {_healthComponent.MaxHealth}", style);
            }
        }
    }
}