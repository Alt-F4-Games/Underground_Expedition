using System;
using Fusion;
using UnityEngine;

namespace Skills
{
    public class EmpoweredStrikeSkill : NetworkSkill
    {
        // Quick cast of the ScriptableObject to access specific methods
        private EmpoweredStrikeData StrikeData => _skillData as EmpoweredStrikeData;

        // ============================================================
        // NETWORK VARIABLES
        // ============================================================
        
        [Networked] 
        public int RemainingStrikes { get; private set; }
        
        private ChangeDetector _changeDetector;

        public override void Spawned()
        {
            base.Spawned();
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }

        // ============================================================
        // EXECUTION LOGIC (PREDICTED CLIENT / SERVER)
        // ============================================================

        public override void OnExecute(NetworkRunner runner)
        {
            if (StrikeData == null) return;
            
            // Grant charges defined in the ScriptableObject
            RemainingStrikes = StrikeData.BaseCharges;
            
            // Start the base cooldown (inherited from NetworkSkill)
            StartCooldown(runner);
        }

        // ============================================================
        // CHARGE CONSUMPTION AND DAMAGE CALCULATION
        // ============================================================

        // Polymorphic override to calculate damage
        public override int ModifyAttackDamage(int baseDamage)
        {
            return ConsumeChargeAndGetDamage(baseDamage);
        }

        private int ConsumeChargeAndGetDamage(int baseDamage)
        {
            // If no charges left, return normal damage
            if (RemainingStrikes <= 0 || StrikeData == null)
            {
                return baseDamage;
            }

            // Only decrement the charge in the authoritative simulation
            // (Client prediction will also execute this if they are attacking)
            if (HasStateAuthority || HasInputAuthority)
            {
                RemainingStrikes--;
            }

            // Extra damage calculation
            float multiplier = StrikeData.GetTotalMultiplier(CurrentLevel);
            float rawDamage = baseDamage * multiplier;
            
            // Exact rounding: MidpointRounding.AwayFromZero
            int finalDamage = (int)Math.Round(rawDamage, MidpointRounding.AwayFromZero);
            
            return finalDamage;
        }
        
        // ============================================================
        // UI POLYMORPHISM OVERRIDES
        // ============================================================
        
        public override int GetCurrentCharges()
        {
            return RemainingStrikes;
        }

        public override int GetMaxCharges()
        {
            if (StrikeData == null) return 0;
            return StrikeData.BaseCharges;
        }

        // ============================================================
        // VISUAL HOOKS (VFX / SFX)
        // ============================================================

        public override void Render()
        {
            // Iterate over all variables that changed this frame for visual updates
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(RemainingStrikes):
                        OnRemainingStrikesChanged();
                        break;
                }
            }
        }
        
        private void OnRemainingStrikesChanged()
        {
            // Hook ready for VFX partner
            if (RemainingStrikes > 0)
            {
                // Enable fists VFX
            }
            else
            {
                // Disable fists VFX
            }
        }
    }
}