using System;
using Events;
using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

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
        
        public event Action BossSpawnedEvent;

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || HasSpawned) return;

            if (!IsSequenceActive)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, ActivationRadius, PlayerLayer);
                
                HashSet<NetworkObject> playersInArea = new HashSet<NetworkObject>();
                
                foreach (var hit in hits)
                {
                    var netObj = hit.GetComponentInParent<NetworkObject>();
                    if (netObj != null)
                    {
                        playersInArea.Add(netObj);
                    }
                }
                
                int totalPlayers = NetworkController.Instance != null ? NetworkController.Instance.ActivePlayerCount : 1;
                
                if (playersInArea.Count > 0 && playersInArea.Count >= totalPlayers)
                {
                    IsSequenceActive = true;
                    
                    ParticleTimer = TickTimer.CreateFromSeconds(Runner, ParticleDelay);
                    BossTimer = TickTimer.CreateFromSeconds(Runner, BossDelay);
                    
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
            }
        }

        protected override void TriggerSpawn()
        {
            base.TriggerSpawn(); 

            if (Object.HasStateAuthority)
            {
                bossSpawnedEvent.RaiseEvent(true);
                BossSpawnedEvent?.Invoke();
            }
        }
    }
}