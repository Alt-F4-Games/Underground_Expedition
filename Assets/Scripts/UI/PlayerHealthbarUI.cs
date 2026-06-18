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

        private NetworkPlayerHealth _networkPlayerHealth;

        void Start()
        {
            FindLocalPlayer();
        }

        void Update()
        {
            if (_networkPlayerHealth == null)
            {
                FindLocalPlayer();
                return;
            }

            healthSlider.maxValue = _networkPlayerHealth.MaxHealth;
            healthSlider.value = _networkPlayerHealth.CurrentHealth;

            healthText.text =
                $"{_networkPlayerHealth.CurrentHealth} / {_networkPlayerHealth.MaxHealth}";
        }

        private void FindLocalPlayer()
        {
            var players = FindObjectsOfType<NetworkPlayerHealth>();

            foreach (var player in players)
            {
                if (player.HasInputAuthority)
                {
                    _networkPlayerHealth = player;
                    return;
                }
            }
        }
    }
}