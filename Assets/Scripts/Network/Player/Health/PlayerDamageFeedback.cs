using Fusion;
using UnityEngine;
using Health;

public class PlayerDamageFeedback : MonoBehaviour
{
    private DamageOverlayUI _overlay;
    private NetworkPlayerHealth _health;

    private void Awake()
    {
        _health = GetComponent<NetworkPlayerHealth>();

        if (_overlay == null)
        {
            _overlay = FindObjectOfType<DamageOverlayUI>();
        }
    }

    private void OnEnable()
    {
        _health.OnDamageFeedback += HandleDamage;
    }

    private void OnDisable()
    {
        _health.OnDamageFeedback -= HandleDamage;
    }

    private void HandleDamage(int damage)
    {
        if (!_health.Object.HasInputAuthority) return;
        
        float intensity = damage / (float)_health.MaxHealth;

        _overlay.PlayEffect(intensity);
    }
}