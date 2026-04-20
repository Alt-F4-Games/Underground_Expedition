using System;
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
        [Header("Ah Puch - Abilities Settings")]
        public float DashSpeedBoost = 2f;
        public float DashDurationSuccess = 3f;
        public float DashDurationFail = 5f;
        
        [Tooltip("Time the boss remains still casting before summoning.")]
        public float InvokeWaitTime = 1.5f;

        [Tooltip("Time the boss remains still preparing before performing a fail Dash.")]
        public float DashWaitTime = 1.5f;

        [Header("Targeting & Vision")]
        [Tooltip("Layer of obstacles (walls) that block the boss's line of sight.")]
        public LayerMask ObstacleLayer;
        
        [Tooltip("Height offset for the boss's 'eyes' when casting the vision ray.")]
        public float EyeHeightOffset = 1.5f;

        [Header("Pathing & Node Detection")]
        [Tooltip("Maximum straight-line distance allowed to start calculating a route to a nearby waypoint.")]
        public float MaxWaypointSearchDistance = 30f;

        [Tooltip("Penalty (in virtual meters) added to waypoints that are behind the boss to prevent backtracking.")]
        public float BackwardsNodePenalty = 50f;

        [Header("References")]
        public DamageAura AuraComponent;

        [Networked] public float CurrentAuraRadius { get; set; }
        
        public float BaseSpeed { get; private set; }
        
        [HideInInspector] public bool IsDashing = false;
        
        [HideInInspector] public int CurrentPathIndex = 0;
        
        [SerializeField] private Animator animator;

        // List to store all summon zones in the level
        private List<Network.Spawn.SummonPoint> _allSummonPoints = new List<Network.Spawn.SummonPoint>();

        public override void Spawned()
        {
            base.Spawned();
            
            if (HasStateAuthority)
            {
                BaseSpeed = Agent.speed;
                CurrentPathIndex = 0; 
                CurrentAuraRadius = AttackRange; 
                
                _allSummonPoints.AddRange(FindObjectsByType<Network.Spawn.SummonPoint>(FindObjectsSortMode.None));

                // Start the initial state via the factory method to avoid hardcoding
                StateMachine.ChangeState(GetPatrolState());
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            
            if (AuraComponent != null)
            {
                AuraComponent.UpdateRadius(CurrentAuraRadius);
            }
            
            if (animator == null) return;

            animator.SetBool("IsDashing", IsDashing);
        }
        
        // TARGETING & LINE OF SIGHT

        public bool LookForTarget()
        {
            if (!HasStateAuthority) return false;
            
            Collider[] hits = Physics.OverlapSphere(transform.position, VisionRange, PlayerLayer);
            
            if (hits.Length > 0)
            {
                Vector3 rayOrigin = transform.position + (Vector3.up * EyeHeightOffset);

                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent(out NetworkObject netObj))
                    {
                        Vector3 playerTargetPos = hit.transform.position + (Vector3.up * 1f);
                        Vector3 directionToPlayer = playerTargetPos - rayOrigin;
                        float distanceToPlayer = directionToPlayer.magnitude;
                        
                        if (!Physics.Raycast(rayOrigin, directionToPlayer.normalized, distanceToPlayer, ObstacleLayer))
                        {
                            TargetPlayer = netObj;
                            return true;
                        }
                    }
                }
            }
            
            TargetPlayer = null;
            return false;
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

            RecalculateStats();
        }

        public void RecalculateStats()
        {
            float dashBonus = IsDashing ? DashSpeedBoost : 0f;
            Agent.speed = BaseSpeed + dashBonus;
            
            if (AuraComponent != null) 
            {
                CurrentAuraRadius = AttackRange;
            }
        }
        
        public void EvaluateAndDecide()
        {
            Debug.Log("[SERVER] Ah Puch is evaluating invoke zones...");
            
            List<Network.Spawn.SummonPoint> activePoints = _allSummonPoints.FindAll(p => p.IsActive);

            if (activePoints.Count > 0)
            {
                Debug.Log($"[SERVER] Found {activePoints.Count} active zones. Invoking!");
                StateMachine.ChangeState(GetInvokeState(activePoints, InvokeWaitTime));
            }
            else
            {
                Debug.Log("[SERVER] No valid invoke zones found. Triggering FAIL DASH.");
                StateMachine.ChangeState(GetDashState(DashDurationFail, DashWaitTime));
            }
        }
        
        // PATHING & NAVIGATION

        public void SetNearestPathIndex()
        {
            if (PatrolPath == null || PatrolPath.Waypoints.Count == 0) return;

            float bestScore = float.MaxValue;
            int bestIndex = CurrentPathIndex;
            UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

            for (int i = 0; i < PatrolPath.Waypoints.Count; i++)
            {
                Transform wp = PatrolPath.GetWaypoint(i);
                if (wp == null) continue;
                
                float rawDistance = Vector3.Distance(transform.position, wp.position);
                
                // Fast discard by straight-line distance (Optimization)
                if (rawDistance > MaxWaypointSearchDistance) continue;

                // Calculate the actual route via NavMesh (Avoids walls)
                if (UnityEngine.AI.NavMesh.CalculatePath(transform.position, wp.position, UnityEngine.AI.NavMesh.AllAreas, path))
                {
                    // Ensure the path is valid and reachable
                    if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        float realWalkingDistance = GetPathLength(path);
                        float nodeScore = realWalkingDistance;

                        // Calculate if the node is in front of or behind the boss using Dot Product
                        Vector3 directionToNode = (wp.position - transform.position).normalized;
                        float dotProduct = Vector3.Dot(transform.forward, directionToNode);

                        // If the node is behind the boss (DotProduct < 0), apply penalty to avoid backtracking
                        if (dotProduct < 0)
                        {
                            nodeScore += BackwardsNodePenalty; 
                        }

                        // Choose the node with the best score (closest walking distance, preferably in front)
                        if (nodeScore < bestScore)
                        {
                            bestScore = nodeScore;
                            bestIndex = i;
                        }
                    }
                }
            }

            CurrentPathIndex = bestIndex;
            Debug.Log($"[SERVER] Ah Puch lost target. Chosen best NavMesh waypoint: Index {CurrentPathIndex} (Score: {bestScore})");
        }
        
        // Helper method to calculate the exact length of a NavMeshPath
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
      
        // ANIMATIONS
        protected override void HandleStateChanged()
        {
            if (animator == null) return;

            switch (CurrentState)
            {
                case NetworkEnemyState.Patrolling:
                case NetworkEnemyState.Chasing:
                    animator.SetBool("IsMoving", true);
                    animator.SetBool("IsCasting", false);
                    break;

                case NetworkEnemyState.Attacking: // Invoke
                    animator.SetBool("IsMoving", false);
                    animator.SetBool("IsCasting", true);
                    break;
            }
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
        
        public virtual INetworkState GetDashState(float duration, float waitTime = 0f)
        {
            return new NetworkDashState(duration, waitTime);
        }
        
        public virtual INetworkState GetInvokeState(List<Network.Spawn.SummonPoint> zones, float waitTime)
        {
            return new NetworkInvokeState(zones, waitTime);
        }
    }
}