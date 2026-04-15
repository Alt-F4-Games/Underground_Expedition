using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Player;

public class StaminaHUDTest : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider staminaSlider;

    private NetworkPlayerStamina _stamina;

    void Start()
    {
        FindLocalPlayer();
    }

    void Update()
    {
        if (_stamina == null)
        {
            FindLocalPlayer();
            return;
        }

        // Valor suave para UI
        staminaSlider.value = _stamina.LocalStaminaNormalized;
    }

    private void FindLocalPlayer()
    {
        var players = FindObjectsOfType<NetworkPlayerStamina>();

        foreach (var player in players)
        {
            if (player.HasInputAuthority)
            {
                _stamina = player;
                Debug.Log("[HUD] Local player stamina linked");
                return;
            }
        }
    }
}