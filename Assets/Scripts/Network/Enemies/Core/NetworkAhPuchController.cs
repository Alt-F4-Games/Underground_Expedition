using Fusion;
using UnityEngine;
using Network.Enemies.Components;

namespace Network.Enemies.Variants
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
        
        [HideInInspector] public int CurrentPathIndex = 0;

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

            if (phase == 2)
            {
                Agent.speed = BaseSpeed + _phase2SpeedAdd;
            }
            else if (phase == 3)
            {
                Agent.speed = BaseSpeed + _phase2SpeedAdd + _phase3SpeedAdd;
                
                if (AuraComponent != null)
                {
                    CurrentAuraRadius = AuraComponent.BaseRadius + _phase3AuraAdd;
                }
            }
            
            Debug.Log($"[SERVER] Ah Puch entered Phase {CurrentPhaseIndex}. Speed: {Agent.speed}, Aura Radius: {CurrentAuraRadius}");
        }
    }
}