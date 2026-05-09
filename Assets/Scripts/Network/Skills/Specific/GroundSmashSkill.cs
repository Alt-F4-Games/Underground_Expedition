using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Health;

#if UNITY_EDITOR
using UnityEditor; 
#endif

namespace Skills
{
    public class GroundSmashSkill : NetworkSkill
    {
        private GroundSmashData SmashData => _skillData as GroundSmashData;

        // ============================================================
        // NETWORK VARIABLES
        // ============================================================

        [Networked] 
        private int SmashCount { get; set; }

        private ChangeDetector _changeDetector;
        private NetworkPlayerController _playerController;
        
        // Cache: HashSet prevents applying damage multiple times to the same enemy 
        // if they have multiple colliders overlapping the area.
        private readonly HashSet<NetworkHealthSystem> _hitTargets = new HashSet<NetworkHealthSystem>();

#if UNITY_EDITOR
        private float _lastGizmoTime;
#endif

        public override void Spawned()
        {
            base.Spawned();
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            _playerController = GetComponent<NetworkPlayerController>();
        }

        // ============================================================
        // EXECUTION LOGIC (PREDICTED CLIENT / SERVER)
        // ============================================================

        public override void OnExecute(NetworkRunner runner)
        {
            if (SmashData == null || Object == null) return;

            // 1. Pay the cost upfront: Start Cooldown immediately to prevent spam exploits
            StartCooldown(runner);

            // 2. Immobilize the player during the smash
            if (_playerController != null)
            {
                _playerController.ApplyStun(SmashData.SelfStunDuration);
            }

            // 3. Trigger visual feedback hook for all clients
            SmashCount++;
#if UNITY_EDITOR
            _lastGizmoTime = Time.time;
#endif

            // Setup Combat Math
            int damage = SmashData.GetTotalDamage(CurrentLevel);
            _hitTargets.Clear();

            // ============================================================
            // DETECTION
            // ============================================================
            
            // Calculate the actual floor position using the inspector's Vertical Offset
            Vector3 floorPos = transform.position + Vector3.up * SmashData.VerticalOffset;
            
            // Broad-phase: Box Center is half the height up from the floor
            Vector3 boxCenter = floorPos + Vector3.up * (SmashData.Height / 2f);
            
            // Broad-phase: We double the height of the box to be generous. 
            // This prevents missing enemies that are slightly hovering or on slopes.
            Vector3 boxHalfExtents = new Vector3(SmashData.Radius, SmashData.Height, SmashData.Radius);

            // Fetch ALL physical colliders in the broad area
            Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, Quaternion.identity, SmashData.EnemyLayer);
            
            int validHits = 0;

            foreach (var col in hits)
            {
                // Catching exceptions ensures that if one enemy gets destroyed 
                // mid-loop, it doesn't crash the execution for the remaining enemies.
                try
                {
                    // Ultra-strict lifecycle validation (Did the object die a millisecond ago?)
                    if (col == null || !col || !col.gameObject.activeInHierarchy) continue;
                    if (col.transform.root == transform.root) continue; // Ignore self

                    // Narrow-phase: Find the closest physical point of the enemy relative to our floor center
                    Vector3 targetPoint = col.ClosestPoint(floorPos);
                    
                    // 2D Distance Check (Ignore Y axis to create a perfect circle footprint)
                    float distanceXZ = Vector2.Distance(
                        new Vector2(floorPos.x, floorPos.z), 
                        new Vector2(targetPoint.x, targetPoint.z)
                    );

                    // If the enemy's closest point intersects our circular radius...
                    if (distanceXZ <= SmashData.Radius)
                    {
                        var health = col.GetComponentInParent<NetworkHealthSystem>();
                        
                        // Single-hit validation
                        if (health != null && !_hitTargets.Contains(health))
                        {
                            validHits++;
                            _hitTargets.Add(health);
                            
                            // Only the Server applies authoritative damage
                            if (HasStateAuthority) 
                            {
                                health.TakeDamage(damage);
                            }
                        }
                    }
                }
                catch (MissingReferenceException)
                {
                    // Silently catch. This happens when an enemy is destroyed instantly by TakeDamage(),
                    // and its secondary colliders try to process in the next loop iteration.
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[GroundSmash] Minor collision error ignored: {e.Message}");
                }
            }

            Debug.Log($"[GroundSmash] Broad-phase detected {hits.Length} colliders. Unique enemies damaged: {validHits}");
        }

        // ============================================================
        // VISUAL HOOKS (VFX / SFX)
        // ============================================================

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                if (change == nameof(SmashCount)) 
                {
                    OnSmashExecuted();
                }
            }
        }
        
        private void OnSmashExecuted() 
        { 
            // Hola ale, porque haces publico mis mensajes de amor?
            // por aca creo que tenes que aplicar las particulas y esas cosas 
            // con amor benaj
        }

        // ============================================================
        // EDITOR GIZMOS (Solid Cylinder Representation)
        // ============================================================
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_skillData == null) return;
            var data = _skillData as GroundSmashData;
            if (data == null) return;

            Vector3 floorPos = transform.position + Vector3.up * data.VerticalOffset;
            float r = data.Radius;
            float h = data.Height;

            // Flash red for 0.2 seconds when attacking, otherwise cyan
            bool isAttacking = (Time.time - _lastGizmoTime < 0.2f);
            Color mainColor = isAttacking ? Color.red : Color.cyan;
            
            // 1. Draw Solid Discs for base and top
            Handles.color = new Color(mainColor.r, mainColor.g, mainColor.b, 0.15f);
            Handles.DrawSolidDisc(floorPos, Vector3.up, r);
            Handles.DrawSolidDisc(floorPos + Vector3.up * h, Vector3.up, r);

            // 2. Draw Wire Contours
            Handles.color = mainColor;
            Handles.DrawWireDisc(floorPos, Vector3.up, r);
            Handles.DrawWireDisc(floorPos + Vector3.up * h, Vector3.up, r);

            // 3. Draw connecting lines to form the cylinder
            Gizmos.color = mainColor;
            Gizmos.DrawLine(floorPos + Vector3.forward * r, floorPos + Vector3.up * h + Vector3.forward * r);
            Gizmos.DrawLine(floorPos + Vector3.back * r, floorPos + Vector3.up * h + Vector3.back * r);
            Gizmos.DrawLine(floorPos + Vector3.left * r, floorPos + Vector3.up * h + Vector3.left * r);
            Gizmos.DrawLine(floorPos + Vector3.right * r, floorPos + Vector3.up * h + Vector3.right * r);
        }
#endif
    }
}