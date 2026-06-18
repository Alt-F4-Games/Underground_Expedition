using System;
using UnityEngine;
using Network;

namespace Network.Items
{
    // =========================================================
    // HEALTH RESTORATION
    // =========================================================
    [CreateAssetMenu(menuName = "Inventory/Effects/Heal Effect")]
    public class HealEffect : ItemEffect
    {
        [Tooltip("Amount of health points restored instantly.")]
        public int healAmount = 25;

        public override bool Apply(NetworkPlayerController player)
        {
            if (player.TryGetComponent(out PlayerStatsManager stats))
            {
                stats.HealPlayer(healAmount);
                Debug.Log($"[SERVER] HealEffect: {player.gameObject.name} restored {healAmount} HP.");
                return true;
            }
            return false;
        }
    }

    // =========================================================
    // STAMINA RESTORATION
    // =========================================================
    [CreateAssetMenu(menuName = "Inventory/Effects/Stamina Effect")]
    public class StaminaEffect : ItemEffect
    {
        [Tooltip("Amount of stamina restored instantly.")]
        public float restoreAmount = 40f;

        public override bool Apply(NetworkPlayerController player)
        {
            if (player.TryGetComponent(out PlayerStatsManager stats))
            {
                stats.RestoreStamina(restoreAmount);
                Debug.Log($"[SERVER] StaminaEffect: {player.gameObject.name} restored {restoreAmount} SP.");
                return true;
            }
            return false;
        }
    }

    // =========================================================
    // DAMAGE BUFF
    // =========================================================
    [CreateAssetMenu(menuName = "Inventory/Effects/Damage Buff")]
    public class DamageBuffEffect : ItemEffect
    {
        [Tooltip("Extra damage percentage. Example: 0.10f represents +10% damage.")]
        public float damagePercentage = 0.10f;
        
        [Tooltip("Total duration of the temporary buff in seconds.")]
        public float duration = 5f;

        public override bool Apply(NetworkPlayerController player)
        {
            if (player.TryGetComponent(out PlayerStatsManager stats))
            {
                stats.ApplyDamageBuff(damagePercentage, duration);
                Debug.Log($"[SERVER] DamageBuffEffect: +{damagePercentage * 100}% damage for {duration}s.");
                return true;
            }
            return false;
        }
    }

    // =========================================================
    // MAX HEALTH BUFF
    // =========================================================
    [CreateAssetMenu(menuName = "Inventory/Effects/Max Health Buff")]
    public class MaxHealthBuffEffect : ItemEffect
    {
        [Tooltip("Percentage of max health increase. Example: 0.10f for 10%.")]
        public float percentageIncrease = 0.10f;

        public override bool Apply(NetworkPlayerController player)
        {
            if (player.TryGetComponent(out PlayerStatsManager stats))
            {
                stats.ApplyMaxHealthBuff(percentageIncrease);
                Debug.Log($"[SERVER] MaxHealthBuffEffect: Max health increased by {percentageIncrease * 100}%.");
                return true;
            }
            return false;
        }
    }

    // =========================================================
    // SPEED BUFF
    // =========================================================
    [CreateAssetMenu(menuName = "Inventory/Effects/Speed Buff")]
    public class SpeedBuffEffect : ItemEffect
    {
        [Tooltip("Extra walk speed percentage. Example: 0.15f for 15%.")]
        public float walkSpeedPercentage = 0.15f;
        
        [Tooltip("Extra sprint speed percentage. Example: 0.15f for 15%.")]
        public float sprintSpeedPercentage = 0.15f;
        
        [Tooltip("Total duration of the temporary buff in seconds.")]
        public float duration = 10f;

        public override bool Apply(NetworkPlayerController player)
        {
            if (player.TryGetComponent(out PlayerStatsManager stats))
            {
                stats.ApplySpeedBuff(walkSpeedPercentage, sprintSpeedPercentage, duration);
                Debug.Log($"[SERVER] SpeedBuffEffect: Speed increased for {duration}s.");
                return true;
            }
            return false;
        }
    }
}