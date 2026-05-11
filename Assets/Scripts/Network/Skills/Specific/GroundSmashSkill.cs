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

            // Start the ActiveEnd timer to act as a "Cast Time"
            ActiveEnd = TickTimer.CreateFromSeconds(runner, SmashData.SelfStunDuration);

            // Immobilize the player during the smash
            if (_playerController != null)
            {
                _playerController.ApplyStun(SmashData.SelfStunDuration);
            }
        }

        // ============================================================
        // 2. EXECUTION PHASE
        // ============================================================

        public override void FixedUpdateNetwork()
        {
            // Check if the charge timer just finished on this exact tick
            if (ActiveEnd.Expired(Runner))
            {
                // Clear the timer to "None" to ensure the smash happens ONLY ONCE
                ActiveEnd = TickTimer.None;
                
                PerformSmash();
            }
        }

        private void PerformSmash()
        {
            // Start the Cooldown ONLY NOW, as the skill finished channeling
            StartCooldown(Runner);

            // Trigger visual feedback (VFX) for all clients
            SmashCount++;
#if UNITY_EDITOR
            _lastGizmoTime = Time.time;
#endif

            int damage = SmashData.GetTotalDamage(CurrentLevel);
            _hitTargets.Clear();

            // ============================================================
            // DETECTION
            // ============================================================
            
            Vector3 floorPos = transform.position + Vector3.up * SmashData.VerticalOffset;
            Vector3 boxCenter = floorPos + Vector3.up * (SmashData.Height / 2f);
            Vector3 boxHalfExtents = new Vector3(SmashData.Radius, SmashData.Height, SmashData.Radius);

            Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, Quaternion.identity, SmashData.EnemyLayer);
            
            int validHits = 0;

            foreach (var col in hits)
            {
                try
                {
                    if (col == null || !col || !col.gameObject.activeInHierarchy) continue;
                    if (col.transform.root == transform.root) continue; 

                    Vector3 targetPoint = col.ClosestPoint(floorPos);
                    
                    float distanceXZ = Vector2.Distance(
                        new Vector2(floorPos.x, floorPos.z), 
                        new Vector2(targetPoint.x, targetPoint.z)
                    );

                    if (distanceXZ <= SmashData.Radius)
                    {
                        var health = col.GetComponentInParent<NetworkHealthSystem>();
                        
                        if (health != null && !_hitTargets.Contains(health))
                        {
                            validHits++;
                            _hitTargets.Add(health);
                            
                            if (HasStateAuthority) 
                            {
                                health.TakeDamage(damage);
                            }
                        }
                    }
                }
                catch (MissingReferenceException) { }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[GroundSmash] Minor collision error ignored: {e.Message}");
                }
            }

            Debug.Log($"[GroundSmash] Broad-phase detected {hits.Length} colliders. Unique enemies damaged: {validHits}");
        }

        // ============================================================
        // UI CONNECTION (Polymorphism)
        // ============================================================
        
        /// <summary>
        /// Informs the SkillSlotUI of the channeling progress (Active Overlay) from 0 to 1.
        /// </summary>
        public override float GetActiveProgress(NetworkRunner runner)
        {
            // If we are not charging/casting, the white bar is empty (0)
            if (ActiveEnd.ExpiredOrNotRunning(runner)) return 0f;
            
            float remaining = ActiveEnd.RemainingTime(runner) ?? 0f;
            
            // Make the value scale from 0% to 100% as the time runs out
            return 1f - (remaining / SmashData.SelfStunDuration);
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
        // EDITOR GIZMOS
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

            bool isAttacking = (Time.time - _lastGizmoTime < 0.2f);
            Color mainColor = isAttacking ? Color.red : Color.cyan;
            
            Handles.color = new Color(mainColor.r, mainColor.g, mainColor.b, 0.15f);
            Handles.DrawSolidDisc(floorPos, Vector3.up, r);
            Handles.DrawSolidDisc(floorPos + Vector3.up * h, Vector3.up, r);

            Handles.color = mainColor;
            Handles.DrawWireDisc(floorPos, Vector3.up, r);
            Handles.DrawWireDisc(floorPos + Vector3.up * h, Vector3.up, r);

            Gizmos.color = mainColor;
            Gizmos.DrawLine(floorPos + Vector3.forward * r, floorPos + Vector3.up * h + Vector3.forward * r);
            Gizmos.DrawLine(floorPos + Vector3.back * r, floorPos + Vector3.up * h + Vector3.back * r);
            Gizmos.DrawLine(floorPos + Vector3.left * r, floorPos + Vector3.up * h + Vector3.left * r);
            Gizmos.DrawLine(floorPos + Vector3.right * r, floorPos + Vector3.up * h + Vector3.right * r);
        }
#endif
    }
}