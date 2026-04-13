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

        // Networked (opcional pero recomendado si otros players lo necesitan)
        [Networked] public float StaminaNormalized { get; private set; }

        // Cache local (más eficiente para UI local)
        public float LocalStaminaNormalized { get; private set; }

        private float _lastValue;

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            UpdateStamina();
        }

        private void UpdateStamina()
        {
            // Protección
            if (_controller == null) return;

            float current;
            float max = _controller.SprintDuration;

            if (_controller.CanSprint)
            {
                // Usamos el timer de sprint
                current = _controller.SprintTimer;
            }
            else
            {
                // Durante cooldown → invertimos el progreso
                current = Mathf.Lerp(0, max,
                    1 - (_controller.SprintCooldownTimer / _controller.SprintCooldown));
            }

            float normalized = Mathf.Clamp01(current / max);

            StaminaNormalized = normalized;

            if (_debug && Mathf.Abs(normalized - _lastValue) > 0.01f)
            {
                Debug.Log($"[STAMINA] Normalized: {normalized:F2}");
                _lastValue = normalized;
            }
        }

        public override void Render()
        {
            // Para UI local (suave y sin delay)
            LocalStaminaNormalized = StaminaNormalized;
        }

        // Helper para UI 0–100
        public float GetStaminaPercent()
        {
            return LocalStaminaNormalized * 100f;
        }
    }
}