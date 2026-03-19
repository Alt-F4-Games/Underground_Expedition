using UnityEngine;

public class TorchController : MonoBehaviour
{
    [Header("Torch Settings")]
    [Tooltip("Transform where the particle effects will be instantiated.")]
    [SerializeField] private Transform particleSpawnPoint;

    [Tooltip("Particle prefab used when the torch is lit.")]
    [SerializeField] private ParticleSystem fireParticlesPrefab;

    [Tooltip("Whether the torch starts turned on.")]
    [SerializeField] private bool startOn = false;

    private ParticleSystem currentFireParticles;

    private void Start()
    {
        if (startOn)
            TurnOn();
        else
            TurnOff();
    }

    public void TurnOn()
    {
        if (fireParticlesPrefab == null || particleSpawnPoint == null)
        {
            Debug.LogWarning($"{name}: FireParticlesPrefab or ParticleSpawnPoint is not assigned.");
            return;
        }

        if (currentFireParticles == null)
        {
            currentFireParticles = Instantiate(
                fireParticlesPrefab,
                particleSpawnPoint.position,
                particleSpawnPoint.rotation,
                particleSpawnPoint
            );
        }

        if (!currentFireParticles.isPlaying)
            currentFireParticles.Play();
    }

    public void TurnOff()
    {
        if (currentFireParticles != null && currentFireParticles.isPlaying)
            currentFireParticles.Stop();
    }

    public void SetTorchState(bool state)
    {
        if (state)
            TurnOn();
        else
            TurnOff();
    }
}