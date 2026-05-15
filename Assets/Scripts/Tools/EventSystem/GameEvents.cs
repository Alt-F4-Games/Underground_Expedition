using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Events{
    public class GameEvent  { }

    public class EnemyDiedEvent : GameEvent
    {
        public PlayerRef killer;
        public int exp;
    }

    public class PlayerDiedEvent : GameEvent
    {
        public bool IsAlive;
    }

    public class PlayerStatsEvent : GameEvent
    {
        public int MaxHealth;
        public float MaxStamina;
        public int PlayerDamage;
    }
    
    public class SkillUpgradeRequestedEvent : GameEvent
    {
        public NetworkObject Player;
        public int SlotIndex;
    }

    public class SkillPointConsumedEvent : GameEvent
    {
        public NetworkObject Player;
        public int SlotIndex;
    }
}