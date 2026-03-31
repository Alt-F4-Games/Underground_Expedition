using System.Collections;
using Health;
using Network.Enemies;
using UnityEngine;
using UnityEngine.AI;

public class NetworkSkullController : NetworkEnemyController
{
    [Header("Explosion Settings")]
    [SerializeField] private float stunDuration = 3;
    [SerializeField] private Color flashFeedbackColor = Color.red;
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
    private Renderer _renderer;
    private Color _originalColor;
    private Coroutine _flashCoroutine;
    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        _enemyHealth = GetComponent<NetworkEnemyHealth>();
        _renderer = GetComponent<Renderer>();
        Agent  = GetComponent<NavMeshAgent>();
        _mpb = new MaterialPropertyBlock();
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
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _originalColor = _renderer.material.color;
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }
    
    private IEnumerator FlashRoutine()
    {
        bool isRed = false;

        while (true)
        {
            isRed = !isRed;

            _renderer.material.color = isRed ? flashFeedbackColor : _originalColor;

            yield return new WaitForSeconds(flashDuration);
        }
    }
    
    public void StopExplodeChargeFeedback()
    {
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _renderer.material.color = _originalColor;
    }

    public void Despawn()
    {
        if (!HasStateAuthority) return;
        
        _enemyHealth.TakeDamage(_enemyHealth.MaxHealth);
    }
    
    public override INetworkState GetAttackState() => new NetworkExplodeState();
}
