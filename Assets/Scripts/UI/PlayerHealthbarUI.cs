using Health;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerHealthbarUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Slider healthSlider;

        [SerializeField] private TextMeshProUGUI healthText;

        private NetworkPlayerHealth _playerHealth;

        private void Start()
        {
            Connect();
        }

        private void Connect()
        {
            _playerHealth = NetworkPlayerHealth.LocalPlayerHealth;

            if (_playerHealth == null)
            {
                Invoke(nameof(Connect), 0.25f);
                return;
            }

            _playerHealth.OnHealthChanged += UpdateHealthUI;

            UpdateHealthUI(
                _playerHealth.CurrentHealth,
                _playerHealth.MaxHealth);
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnHealthChanged -= UpdateHealthUI;
        }

        private void UpdateHealthUI(int current, int max)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;

            healthText.SetText("{0}/{1}", current, max);
        }
    }
}