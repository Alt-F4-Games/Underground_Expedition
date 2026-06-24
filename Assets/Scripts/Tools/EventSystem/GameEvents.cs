using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Network.Quests.Runtime;
using UnityEngine;

namespace Tools.EventSystem
{
    public class GameEvent { }

    // =====================================================
    // PLAYER & ENEMIES
    // =====================================================

    public class EnemyDiedEvent : GameEvent
    {
        public PlayerRef killer;
        public int exp;
        public string enemyId;
        public NetworkObject enemyObject;

    }

    public class EnemyTakeDamageEvent : GameEvent
    {
        public NetworkObject enemyObject;
    }

    public class PlayerDiedEvent : GameEvent
    {
        public bool IsAlive;
    }

    public class PlayerStatsEvent : GameEvent
    {
        public NetworkObject Player;
        public int MaxHealth;
        public float MaxStamina;
        public int PlayerDamage;
    }
    
    // =====================================================
    // ITEMS & NPCs
    // =====================================================

    public class ItemCollectedEvent : GameEvent
    {
        public PlayerRef player;
        public string itemId;
        public int quantity;
    }
    
    public class ItemCraftedEvent : GameEvent
    {
        public PlayerRef player;
        public string recipeId;
        public string resultItemId;
        public int quantity;
    }

    public class NpcInteractionEvent : GameEvent
    {
        public PlayerRef player;
        public string npcId;
    }

    // =====================================================
    // QUESTS
    // =====================================================

    public class QuestUIRefreshEvent : GameEvent { }
    
    public class QuestAcceptedEvent : GameEvent
    {
        public string questId;
    }
    
    public class QuestCompletedEvent : GameEvent
    {
        public string questId;
    }
    
    public class ObjectiveCompletedEvent : GameEvent
    {
        public string questId;
        public int stepIndex;
    }
    
    public class RewardClaimedEvent : GameEvent
    {
        public string questId;
    }
    
    public class QuestCancelledEvent : GameEvent
    {
        public string questId;
    }

    // =====================================================
    // SKILLS
    // =====================================================

    public class SkillUpgradeRequestedEvent : GameEvent
    {
        public NetworkObject Player;
        public int SlotIndex;
        
        public SkillUpgradeRequestedEvent(NetworkObject player, int slotIndex)
        {
            Player = player;
            SlotIndex = slotIndex;
        }
    }

    public class SkillPointConsumedEvent : GameEvent
    {
        public NetworkObject Player;
        public int SlotIndex;

        public SkillPointConsumedEvent(NetworkObject player, int slotIndex)
        {
            Player = player;
            SlotIndex = slotIndex;
        }
    }
}