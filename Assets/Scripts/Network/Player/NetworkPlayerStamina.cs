using Fusion;
using UnityEngine;

namespace Player
{
    public class NetworkPlayerStamina : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private NetworkPlayerController _controller;

        [Header("Debug")]
        [SerializeField] private bool _debug;

        [Networked] [HideInInspector] public float StaminaNormalized { get; private set; }

        // Cache local para UI suave
        public float LocalStaminaNormalized { get; private set; }

        private float _lastValue;

        // ============================================================
        // NETWORK
        // ============================================================

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            UpdateStamina();
        }

        private void UpdateStamina()
        {
            if (_controller == null) return;

            float max = _controller.SprintDuration;
            float current = _controller.SprintTimer;

            float normalized = 0f;

            if (max > 0f)
                normalized = Mathf.Clamp01(current / max);

            StaminaNormalized = normalized;

            if (_debug && Mathf.Abs(normalized - _lastValue) > 0.01f)
            {
                Debug.Log($"[STAMINA] {normalized:F2}");
                _lastValue = normalized;
            }
        }

        // ============================================================
        // RENDER (CLIENT)
        // ============================================================

        public override void Render()
        {
            // UI local sin delay de red
            LocalStaminaNormalized = StaminaNormalized;
        }

        // Helper opcional para UI
        public float GetStaminaPercent()
        {
            return LocalStaminaNormalized * 100f;
        }
    }
}