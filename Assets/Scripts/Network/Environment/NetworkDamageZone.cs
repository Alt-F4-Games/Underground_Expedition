using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Health;

[RequireComponent(typeof(Collider))]
public class NetworkDamageZone : NetworkBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _tickRate = 1f;

    [Header("Debug")]
    [SerializeField] private bool _debug;

    private readonly List<NetworkHealthSystem> _targets = new();

    private float _tickTimer;
    private Collider _zoneCollider;

    private void Awake()
    {
        _zoneCollider = GetComponent<Collider>();
        _zoneCollider.isTrigger = true;
    }

    // ============================================================
    // TRIGGERS
    // ============================================================

    private void OnTriggerEnter(Collider other)
    {
        NetworkHealthSystem health = null;

        health = other.GetComponent<NetworkHealthSystem>();

        if (health == null)
            health = other.GetComponentInParent<NetworkHealthSystem>();

        if (health == null)
        {
            var no = other.GetComponentInParent<NetworkObject>();
            if (no != null)
                health = no.GetComponent<NetworkHealthSystem>();
        }

        if (health == null) return;

        if (health == null) return;

        if (!_targets.Contains(health))
        {
            _targets.Add(health);

            if (_debug)
                Debug.Log($"[ZONE] Enter: {health.name}");
        }
        Debug.Log($"[ZONE] Collider entered: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        var health = other.GetComponentInParent<NetworkHealthSystem>();

        if (health == null) return;

        if (_targets.Remove(health))
        {
            if (_debug)
                Debug.Log($"[ZONE] Exit: {health.name}");
        }
    }

    // ============================================================
    // NETWORK LOOP
    // ============================================================

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        _tickTimer -= Runner.DeltaTime;

        if (_tickTimer > 0f) return;

        _tickTimer = _tickRate;

        ApplyDamage();
    }

    // ============================================================
    // DAMAGE LOGIC
    // ============================================================

    private void ApplyDamage()
    {
        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            var target = _targets[i];

            if (target == null)
            {
                _targets.RemoveAt(i);
                continue;
            }

            if (!IsInsideZone(target))
            {
                _targets.RemoveAt(i);

                if (_debug)
                    Debug.Log($"[ZONE] Removed (out of bounds): {target.name}");

                continue;
            }

            if (!target.IsAlive)
                continue;

            if (target is NetworkPlayerHealth player)
            {
                HandlePlayerDamage(player);
            }
            else
            {
                HandleGenericDamage(target);
            }
        }
    }

    private void HandlePlayerDamage(NetworkPlayerHealth player)
    {
        if (_debug)
            Debug.Log($"[ZONE] Damage PLAYER: {player.name}");

        player.TakeDamage(_damage);
    }

    private void HandleGenericDamage(NetworkHealthSystem target)
    {
        if (_debug)
            Debug.Log($"[ZONE] Damage MOB: {target.name}");

        target.TakeDamage(_damage);
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    private bool IsInsideZone(NetworkHealthSystem target)
    {
        if (_zoneCollider == null) return false;

        var targetCollider = target.GetComponentInChildren<Collider>();

        if (targetCollider == null) return false;

        return _zoneCollider.bounds.Intersects(targetCollider.bounds);
    }
}