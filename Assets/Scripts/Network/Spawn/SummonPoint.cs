using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Network.Enemies;

namespace Network.Spawn
{
    // Now inherits from SpawnPoints to reuse core spawning logic
    public class SummonPoint : SpawnPoints
    {
        [Header("Activation Settings")]
        [Tooltip("Detection radius to activate this summon point.")]
        public float ActivationRadius = 15f;
        public LayerMask PlayerLayer;

        // Networked property that any Controller can read
        [Networked] public NetworkBool IsActive { get; private set; }

        // Overriding Spawned to prevent automatic spawning at the start of the game
        public override void Spawned()
        {
            if (!HasStateAuthority) return;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            // Detect if any player is within the activation radius
            Collider[] hits = Physics.OverlapSphere(transform.position, ActivationRadius, PlayerLayer);
            IsActive = hits.Length > 0;
        }

        public void TriggerSummon()
        {
            if (!HasStateAuthority) return;

            // trigger the base Spawn logic (the main prefab configured in SpawnPoints)
            base.Spawn();

            Debug.Log($"[SERVER] SummonPoint '{gameObject.name}' executed summon.");
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            // Check if the Object is valid before accessing [Networked] properties
            if (Application.isPlaying && Object != null && Object.IsValid && IsActive)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawSphere(transform.position, 0.3f);
        }

        private void OnDrawGizmosSelected()
        {
            // Check if the Object is valid before accessing [Networked] properties
            if (Application.isPlaying && Object != null && Object.IsValid && IsActive)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            }
            else
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            }
            Gizmos.DrawSphere(transform.position, ActivationRadius);
        }
    }
}