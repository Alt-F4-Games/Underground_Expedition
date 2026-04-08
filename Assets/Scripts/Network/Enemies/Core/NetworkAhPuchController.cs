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
                
                // Initialize the path index at 0
                CurrentPathIndex = 0; 
                
                CurrentAuraRadius = AuraComponent != null ? AuraComponent.BaseRadius : 3f;
                
                // Initialize timer for the first phase transition
                PhaseTimer = TickTimer.CreateFromSeconds(Runner, _timeToPhase2);
                
                // FIND ALL SUMMON POINTS IN THE SCENE AT START
                _allSummonPoints.AddRange(FindObjectsByType<Network.Spawn.SummonPoint>(FindObjectsSortMode.None));

                // Change initial state so it starts advancing through nodes
                StateMachine.ChangeState(new AhPuchAdvanceState());
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
            // Agent Stats
            if (node.NewSpeed != 0f) BaseSpeed = node.NewSpeed; // Update base so phases scale correctly
            if (node.NewAngularSpeed != 0f) Agent.angularSpeed = node.NewAngularSpeed;
            if (node.NewAcceleration != 0f) Agent.acceleration = node.NewAcceleration;

            // Detection
            if (node.NewVisionRange != 0f) VisionRange = node.NewVisionRange;
            if (node.NewAttackRange != 0f) AttackRange = node.NewAttackRange;

            // Combat
            if (node.NewAttackCooldown != 0f) AttackCooldown = node.NewAttackCooldown;
            if (node.NewAuraRadius != 0f && AuraComponent != null) AuraComponent.BaseRadius = node.NewAuraRadius;

            // Dash
            if (node.NewDashSpeedBoost != 0f) DashSpeedBoost = node.NewDashSpeedBoost;
            if (node.NewDashDurationSuccess != 0f) DashDurationSuccess = node.NewDashDurationSuccess;
            if (node.NewDashDurationFail != 0f) DashDurationFail = node.NewDashDurationFail;

            // Recalculate phase bonuses in case BaseSpeed or BaseRadius changed
            RecalculatePhaseStats();
        }

        // Keeps math clean by adding phase bonuses to the current base
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
                CurrentAuraRadius = AuraComponent.BaseRadius + auraBonus;
            }
        }
        
        public void EvaluateAndDecide()
        {
            Debug.Log("[SERVER] Ah Puch is evaluating invoke zones...");
            
            List<Network.Spawn.SummonPoint> activePoints = _allSummonPoints.FindAll(p => p.IsActive);

            if (activePoints.Count > 0)
            {
                Debug.Log($"[SERVER] Found {activePoints.Count} active zones. Invoking!");
                StateMachine.ChangeState(new AhPuchInvokeState(activePoints));
            }
            else
            {
                Debug.Log("[SERVER] No players near summon zones. Triggering FAIL DASH.");
                StateMachine.ChangeState(new AhPuchDashState(DashDurationFail));
            }
        }
    }
}