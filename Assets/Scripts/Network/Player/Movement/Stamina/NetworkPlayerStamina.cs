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
            if (!HasStateAuthority) return;

            if (_controller == null) return;

            float normalized = 0f;

            if (_controller.MaxStamina > 0f)
                normalized = _controller.CurrentStamina / _controller.MaxStamina;

            StaminaNormalized = normalized;
        }

        // ============================================================
        // RENDER (CLIENT)
        // ============================================================

        public override void Render() { LocalStaminaNormalized = StaminaNormalized; }
        public float GetCurrentStamina() { return _controller.CurrentStamina; }
        public float GetMaxStamina() { return _controller.MaxStamina; }
    }
}