using Events;
using Fusion;
using UnityEngine;

namespace Network.Spawn
{
    public class BossProximitySpawner : EnemyProximitySpawner
    {
        [Header("Event")]
        [SerializeField] private BoolEventChannel bossSpawnedEvent;
        
        [Header("Sequence Settings")]
        [Tooltip("Partícula (GameObject estándar) que aparecerá antes del jefe.")]
        public GameObject ParticlePrefab;
        
        [Tooltip("Tiempo (en segundos) desde que el jugador entra al área hasta que aparece la partícula.")]
        public float ParticleDelay = 2f;
        
        [Tooltip("Tiempo total (en segundos) desde que se activa la zona hasta que aparece el jefe.")]
        public float BossDelay = 5f;

        [Networked] public NetworkBool IsSequenceActive { get; set; }
        
        [Networked, OnChangedRender(nameof(PlayParticleEffect))] 
        public NetworkBool HasSpawnedParticle { get; set; }

        [Networked] public TickTimer ParticleTimer { get; set; }
        [Networked] public TickTimer BossTimer { get; set; }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || HasSpawned) return;

            if (!IsSequenceActive)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, ActivationRadius, PlayerLayer);
                
                if (hits.Length > 0)
                {
                    IsSequenceActive = true;
                    
                    ParticleTimer = TickTimer.CreateFromSeconds(Runner, ParticleDelay);
                    BossTimer = TickTimer.CreateFromSeconds(Runner, BossDelay);
                    
                    Debug.Log($"[SERVER] Secuencia de Boss iniciada. Partícula en {ParticleDelay}s, Jefe en {BossDelay}s.");
                }
            }
            else
            {
                if (!HasSpawnedParticle && ParticleTimer.Expired(Runner))
                {
                    HasSpawnedParticle = true; 
                }

                if (BossTimer.Expired(Runner))
                {
                    TriggerSpawn();
                }
            }
        }

        // Callback visual ejecutado en todos los clientes cuando HasSpawnedParticle cambia a true
        public void PlayParticleEffect()
        {
            if (HasSpawnedParticle && ParticlePrefab != null)
            {
                Vector3 spawnPos = CustomSpawnPoint != null ? CustomSpawnPoint.position : transform.position;
                Instantiate(ParticlePrefab, spawnPos, Quaternion.identity);
                Debug.Log("[CLIENT/SERVER] Efecto de partícula invocado.");
            }
        }

        protected override void TriggerSpawn()
        {
            base.TriggerSpawn(); 

            if (Object.HasStateAuthority)
            {
                bossSpawnedEvent.RaiseEvent(true);
            }
        }
    }
}