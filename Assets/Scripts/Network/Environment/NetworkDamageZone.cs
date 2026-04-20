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

    private readonly List<NetworkHealthSystem> _targets = new();

    private float _tickTimer;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public override void Spawned()
    {
        _targets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Runner || !Runner.IsServer) return;

        var health = other.GetComponentInParent<NetworkHealthSystem>();
        if (health == null) return;

        if (!_targets.Contains(health))
        {
            _targets.Add(health);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Runner || !Runner.IsServer) return;

        var health = other.GetComponentInParent<NetworkHealthSystem>();
        if (health == null) return;

        _targets.Remove(health);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        _tickTimer -= Runner.DeltaTime;

        if (_tickTimer > 0f) return;

        _tickTimer = _tickRate;

        ApplyDamage();
    }

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

            if (!target.IsAlive)
                continue;

            target.TakeDamage(_damage);
        }
    }
}