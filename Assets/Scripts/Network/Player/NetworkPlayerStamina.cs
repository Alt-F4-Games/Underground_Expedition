using Fusion;
using UnityEngine;

namespace Player
{
    public class NetworkPlayerStamina : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private NetworkPlayerController _controller;
        
        private bool _debug = true;

        [Networked] [HideInInspector] public float StaminaNormalized { get; private set; }

        public float LocalStaminaNormalized { get; private set; }

        private float _lastValue;

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            UpdateStamina();
        }

        private void UpdateStamina()
        {
            if (_controller == null) return;

            float current;
            float max = _controller.SprintDuration;

            if (_controller.CanSprint)
            {
                current = _controller.SprintTimer;
            }
            else
            {
                current = Mathf.Lerp(0, max,
                    1 - (_controller.SprintCooldownTimer / _controller.SprintCooldown));
            }

            float normalized = Mathf.Clamp01(current / max);

            StaminaNormalized = normalized;

            if (_debug && Mathf.Abs(normalized - _lastValue) > 0.01f)
            {
                _lastValue = normalized;
            }
        }

        public override void Render()
        {
            LocalStaminaNormalized = StaminaNormalized;
        }
    }
}