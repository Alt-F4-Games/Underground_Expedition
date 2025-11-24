/*
 * TorchController
 * ---------------
 * Controls a simple torch with particle effects for ON/OFF states.
 * Spawns and manages fire particles at a given spawn point.
 *
 * Dependencies:
 * - ParticleSystem prefab (fireParticlesPrefab)
 * - Transform (particleSpawnPoint)
 */

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
        // Initialize according to startOn flag
        if (startOn)
            TurnOn();
        else
            TurnOff();
    }

    public void TurnOn()
    {
        // Ensure required references are valid
        if (fireParticlesPrefab == null || particleSpawnPoint == null)
        {
            Debug.LogWarning($"{name}: FireParticlesPrefab or ParticleSpawnPoint is not assigned.");
            return;
        }

        // Instantiate the fire particle system if not already spawned
        if (currentFireParticles == null)
        {
            currentFireParticles = Instantiate(
                fireParticlesPrefab,
                particleSpawnPoint.position,
                particleSpawnPoint.rotation,
                particleSpawnPoint
            );
        }

        // Play the particle effect
        if (!currentFireParticles.isPlaying)
            currentFireParticles.Play();
    }

    public void TurnOff()
    {
        // Stop particle effect if active
        if (currentFireParticles != null && currentFireParticles.isPlaying)
            currentFireParticles.Stop();
    }

    public void SetTorchState(bool state)
    {
        // Toggle based on boolean input
        if (state)
            TurnOn();
        else
            TurnOff();
    }
}
