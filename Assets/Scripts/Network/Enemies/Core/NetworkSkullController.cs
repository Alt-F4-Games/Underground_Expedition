using System.Collections;
using Health;
using Network.Enemies;
using UnityEngine;
using UnityEngine.AI;

public class NetworkSkullController : NetworkEnemyController
{
    [Header("Explosion Settings")]
    [SerializeField] private float stunDuration = 3;
    [SerializeField] private float flashDuration = 0.2f;
    
    [Header("Floating Settings")]
    [SerializeField] private float floatHeight = 2.5f;
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatSpeed = 2f;
    
    // Variables
    private float _baseOffset;
    
    // Getters
    public float StunDuration => stunDuration;
    
    // Components
    private NetworkEnemyHealth _enemyHealth;

    private void Awake()
    {
        _enemyHealth = GetComponent<NetworkEnemyHealth>();
        Agent  = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        Agent.updateUpAxis = false; 
        Agent.updateRotation = true;

        _baseOffset = floatHeight;
        Agent.baseOffset = floatHeight;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        
        Agent.baseOffset = _baseOffset + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
    }

    public void PlayExplosionFeedback()
    {
        // VFx
    }
    public void PlayExplodeChargeFeedback()
    {
        // Explosion Anim
    }

    public void Despawn()
    {
        if (!HasStateAuthority) return;
        
        _enemyHealth.TakeDamage(_enemyHealth.MaxHealth);
    }
    
    public override INetworkState GetAttackState() => new NetworkExplodeState();
}
