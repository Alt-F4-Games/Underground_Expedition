using UnityEngine;
using UnityEngine.UI;

public class StaminaHUDTest : MonoBehaviour
{
    [SerializeField]
    private Slider staminaSlider;

    private NetworkPlayerController _player;

    private void Start()
    {
        Connect();
    }

    private void Connect()
    {
        _player = NetworkPlayerController.Local;

        if (_player == null)
        {
            Invoke(nameof(Connect), 0.25f);
            return;
        }

        _player.OnStaminaChanged += UpdateUI;

        UpdateUI(
            _player.CurrentStamina,
            _player.MaxStamina);
    }

    private void OnDestroy()
    {
        if (_player != null)
        {
            _player.OnStaminaChanged -= UpdateUI;
        }
    }

    private void UpdateUI(float current, float max)
    {
        staminaSlider.maxValue = max;
        staminaSlider.value = current;
    }
}