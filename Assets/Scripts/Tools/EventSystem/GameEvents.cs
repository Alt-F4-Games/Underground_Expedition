using Fusion;
using Network.Quests.Runtime;
using UnityEngine;

namespace Tools.EventSystem
{
    public class GameEvent { }

    // =====================================================
    // PLAYER
    // =====================================================

    public class EnemyDiedEvent : GameEvent
    {
        public PlayerRef killer;
        public int exp;
        public string enemyId;
        public NetworkObject enemyObject;

    }

    public class EnemyAttackEvent : GameEvent
    {
        public NetworkObject enemyObject;
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

    // =====================================================
    // QUESTS
    // =====================================================

    public class QuestUIRefreshEvent : GameEvent { }
    
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