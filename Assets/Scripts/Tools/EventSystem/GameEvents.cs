using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Events{
    public class GameEvent  { }

    public class EnemyDiedEvent : GameEvent
    {
        public PlayerRef killer;
        public int exp;
        public string enemyId;
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
    
    public class ZoneDiscoveredEvent : GameEvent
    {
        public PlayerRef player;
        public string zoneId;
    }

    public class InteractObjectiveEvent : GameEvent
    {
        public PlayerRef player;
        public string interactionId;
    }

    public class NpcInteractionEvent : GameEvent
    {
        public PlayerRef player;
        public string npcId;
    }
    
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
}


