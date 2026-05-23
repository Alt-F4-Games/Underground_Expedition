using System;
using UnityEngine;
using Health;

namespace Network.Items
{
    [Serializable]
    public class HealEffect : ItemEffect
    {
        [Tooltip("Amount of health points restored instantly (ointment).")]
        public int healAmount = 25;

        public override bool Apply(NetworkPlayerController player)
        {
            if (player.TryGetComponent(out NetworkPlayerHealth health))
            {
                if (health.CurrentHealth >= health.MaxHealth) return false;
                
                // TODO
                
                Debug.Log($"[SERVER] HealEffect: {player.gameObject.name} restored {healAmount} health.");
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class StaminaEffect : ItemEffect
    {
        [Tooltip("Amount of stamina restored instantly (Canteen).")]
        public float restoreAmount = 40f;

        public override bool Apply(NetworkPlayerController player)
        {
            if (player.CurrentStamina >= player.MaxStamina) return false;
            
            // TODO
            
            Debug.Log($"[SERVER] StaminaEffect: {player.gameObject.name} restored {restoreAmount} stamina.");
            return true;
        }
    }

    [Serializable]
    public class DamageBuffEffect : ItemEffect
    {
        [Tooltip("Extra damage percentage. Example: 0.15f represents +15% damage (Whetstone).")]
        public float damagePercentage = 0.15f;
        
        [Tooltip("Total duration of the temporary buff in seconds.")]
        public float duration = 10f;

        public override bool Apply(NetworkPlayerController player)
        {
            if (player.TryGetComponent(out NetworkPlayerAttack attackSystem))
            {
                // TODO
                
                Debug.Log($"[SERVER] DamageBuffEffect: +{damagePercentage * 100}% multiplier applied for {duration} seconds.");
                return true;
            }
            return false;
        }
    }
}