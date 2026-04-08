using Fusion;
using UnityEngine;
using System.Collections.Generic;
using Network.Enemies.Components;
using Network.Enemies;
using Network.Enemies.States;

namespace Network.Enemies
{
    public class NetworkAhPuchController : NetworkEnemyController
    {
        [Header("Ah Puch - Phase Settings")]
        [SerializeField] private float _timeToPhase2 = 30f;
        [SerializeField] private float _timeToPhase3 = 30f;
        [SerializeField] private float _phase2SpeedAdd = 0.2f;
        [SerializeField] private float _phase3SpeedAdd = 0.4f;
        [SerializeField] private float _phase3AuraAdd = 2f;

        [Header("Ah Puch - Abilities Settings")]
        public float DashSpeedBoost = 2f;
        public float DashDurationSuccess = 3f;
        public float DashDurationFail = 5f;

        [Header("Pathing & Node Detection")]
        [Tooltip("Radius to detect and read Stat Nodes while chasing the player.")]
        public float ChaseNodeDetectionRadius = 3f;
        
        [Tooltip("Maximum straight-line distance allowed to start calculating a route to a nearby waypoint.")]
        public float MaxWaypointSearchDistance = 30f;

        [Header("References")]
        public DamageAura AuraComponent;

        [Networked] public TickTimer PhaseTimer { get; set; }
        [Networked] public int CurrentPhaseIndex { get; set; }
        [Networked] public float CurrentAuraRadius { get; set; }
        
        public float BaseSpeed { get; private set; }
        
        [HideInInspector] public bool IsDashing = false;
        
        [HideInInspector] public int CurrentPathIndex = 0;

        // List to store all summon zones in the level
        private List<Network.Spawn.SummonPoint> _allSummonPoints = new List<Network.Spawn.SummonPoint>();

        public override void Spawned()
        {
            base.Spawned();
            
            if (HasStateAuthority)
            {
                BaseSpeed = Agent.speed;
                CurrentPhaseIndex = 1;
                
                CurrentPathIndex = 0; 
                
                CurrentAuraRadius = AttackRange; 
                
                PhaseTimer = TickTimer.CreateFromSeconds(Runner, _timeToPhase2);
                
                _allSummonPoints.AddRange(FindObjectsByType<Network.Spawn.SummonPoint>(FindObjectsSortMode.None));

                // Start the initial state via the factory method to avoid hardcoding
                StateMachine.ChangeState(GetPatrolState());
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (HasStateAuthority)
            {
                UpdatePhases();
            }
            
            if (AuraComponent != null)
            {
                AuraComponent.UpdateRadius(CurrentAuraRadius);
            }
        }
        
        public bool LookForTarget()
        {
            if (!HasStateAuthority) return false;
            
            Collider[] hits = Physics.OverlapSphere(transform.position, VisionRange, PlayerLayer);
            
            if (hits.Length > 0)
            {
                if (hits[0].TryGetComponent(out NetworkObject netObj))
                {
                    TargetPlayer = netObj;
                    return true;
                }
            }

            TargetPlayer = null;
            return false;
        }

        private void UpdatePhases()
        {
            if (PhaseTimer.Expired(Runner))
            {
                if (CurrentPhaseIndex == 1)
                {
                    EnterPhase(2);
                    PhaseTimer = TickTimer.CreateFromSeconds(Runner, _timeToPhase3);
                }
                else if (CurrentPhaseIndex == 2)
                {
                    EnterPhase(3);
                    PhaseTimer = TickTimer.None; 
                }
            }
        }

        private void EnterPhase(int phase)
        {
            CurrentPhaseIndex = phase;
            RecalculatePhaseStats();
            Debug.Log($"[SERVER] Ah Puch entered Phase {CurrentPhaseIndex}. Speed: {Agent.speed}, Aura Radius: {CurrentAuraRadius}");
        }

        public void ApplyStatNode(AhPuchStatNode node)
        {
            if (node.NewSpeed != 0f) BaseSpeed = node.NewSpeed; 
            if (node.NewAngularSpeed != 0f) Agent.angularSpeed = node.NewAngularSpeed;
            if (node.NewAcceleration != 0f) Agent.acceleration = node.NewAcceleration;

            if (node.NewVisionRange != 0f) VisionRange = node.NewVisionRange;
            
            if (node.NewAttackRange != 0f) AttackRange = node.NewAttackRange;

            if (node.NewAttackCooldown != 0f) AttackCooldown = node.NewAttackCooldown;

            if (node.NewDashSpeedBoost != 0f) DashSpeedBoost = node.NewDashSpeedBoost;
            if (node.NewDashDurationSuccess != 0f) DashDurationSuccess = node.NewDashDurationSuccess;
            if (node.NewDashDurationFail != 0f) DashDurationFail = node.NewDashDurationFail;

            RecalculatePhaseStats();
        }

        public void RecalculatePhaseStats()
        {
            float speedBonus = 0f;
            float auraBonus = 0f;

            if (CurrentPhaseIndex >= 2) speedBonus += _phase2SpeedAdd;
            if (CurrentPhaseIndex >= 3) 
            {
                speedBonus += _phase3SpeedAdd;
                auraBonus += _phase3AuraAdd;
            }

            float dashBonus = IsDashing ? DashSpeedBoost : 0f;

            Agent.speed = BaseSpeed + speedBonus + dashBonus;
            
            if (AuraComponent != null) 
            {
                CurrentAuraRadius = AttackRange + auraBonus;
            }
        }
        
        public void EvaluateAndDecide()
        {
            Debug.Log("[SERVER] Ah Puch is evaluating invoke zones...");
            
            List<Network.Spawn.SummonPoint> activePoints = _allSummonPoints.FindAll(p => p.IsActive);

            if (activePoints.Count > 0)
            {
                Debug.Log($"[SERVER] Found {activePoints.Count} active zones. Invoking!");
                StateMachine.ChangeState(GetInvokeState(activePoints));
            }
            else
            {
                Debug.Log("[SERVER] No valid invoke zones found. Triggering FAIL DASH.");
                StateMachine.ChangeState(GetDashState(DashDurationFail));
            }
        }

        // PATHING & NAVIGATION

        public void SetNearestPathIndex()
        {
            if (PatrolPath == null || PatrolPath.Waypoints.Count == 0) return;

            float minDistance = float.MaxValue;
            int nearestIndex = CurrentPathIndex;
            UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

            for (int i = 0; i < PatrolPath.Waypoints.Count; i++)
            {
                Transform wp = PatrolPath.GetWaypoint(i);
                if (wp == null) continue;
                
                // Fast discard by straight-line distance (Optimization)
                if (Vector3.Distance(transform.position, wp.position) > MaxWaypointSearchDistance) continue;

                // Calculate the actual route via NavMesh (Avoids walls)
                if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, wp.position, UnityEngine.AI.NavMesh.AllAreas, path))
                {
                    // Ensure the path is valid and reachable
                    if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        float dist = GetPathLength(path);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            nearestIndex = i;
                        }
                    }
                }
            }

            CurrentPathIndex = nearestIndex;
            Debug.Log($"[SERVER] Ah Puch lost target. Recalculated nearest waypoint (NavMesh): Index {CurrentPathIndex}");
        }
        
        private float GetPathLength(UnityEngine.AI.NavMeshPath path)
        {
            float length = 0.0f;
            if (path.corners.Length > 1)
            {
                for (int i = 1; i < path.corners.Length; i++)
                {
                    length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                }
            }
            return length;
        }
      
        // STATE FACTORY METHODS (Overrides & Customs)
        
        public override INetworkState GetPatrolState()
        {
            return new NetworkAdvanceState();
        }
        
        public override INetworkState GetChaseState()
        {
            return new NetworkAuraChaseState(); 
        }

        public virtual INetworkState GetDashState(float duration)
        {
            return new NetworkDashState(duration);
        }

        public virtual INetworkState GetInvokeState(List<Network.Spawn.SummonPoint> zones)
        {
            return new NetworkInvokeState(zones);
        }
    }
}