using Fusion;
using Network.Quests.Runtime;

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
    // ITEMS
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

    // =====================================================
    // WORLD
    // =====================================================

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

    // =====================================================
    // QUESTS
    // =====================================================

    public class QuestAcceptedEvent : GameEvent
    {
        public QuestRuntime quest;
    }

    public class QuestCompletedEvent : GameEvent
    {
        public QuestRuntime quest;
    }

    public class QuestCancelledEvent : GameEvent
    {
        public QuestRuntime quest;
    }

    public class RewardClaimedEvent : GameEvent
    {
        public QuestRuntime quest;
    }

    public class ObjectiveCompletedEvent : GameEvent
    {
        public QuestRuntime quest;
        public int StepIndex;
    }

    public class QuestObjectiveProgressEvent : GameEvent
    {
        public QuestRuntime quest;

        public int StepIndex;

        public int ObjectiveIndex;

        public int CurrentAmount;

        public int RequiredAmount;
    }
    
    public class QuestUIRefreshEvent
        : GameEvent
    {
    }
}