using System;
using Fusion;
using Unity.VisualScripting;

namespace Network.Quests
{
    /// <summary>
    /// Estado compartido de Main Quests para toda la sesión.
    /// Solo State Authority puede modificarlo.
    /// </summary>
    public class NetworkQuestSession : NetworkBehaviour
    {
        public static NetworkQuestSession Instance { get; private set; }

        [Networked, Capacity(32)] public NetworkDictionary<NetworkString<_16>, byte> AcceptedMainQuests => default;

        [Networked, Capacity(32)] public NetworkDictionary<NetworkString<_16>, byte> CompletedMainQuests => default;
        
        [Networked, Capacity(32)] public NetworkDictionary<NetworkString<_16>, byte> ClaimedRewards => default;

        [Networked, Capacity(32)] public NetworkDictionary<NetworkString<_16>, int> ObjectiveProgress => default;
        

        public override void Spawned() { Instance = this; }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool IsMainQuestCompleted(string questId) { return CompletedMainQuests.ContainsKey(questId); }

        public void MarkMainQuestAccepted(string questId)
        {
            if (!HasStateAuthority)
                return;

            AcceptedMainQuests.Set(questId, 1);
            
        }

        public void MarkMainQuestCompleted(string questId)
        {
            if (!HasStateAuthority)
                return;

            CompletedMainQuests.Set(questId, 1);
            
        }

        public int GetObjectiveProgress(string questId, int objectiveIndex)
        {
            string key = $"{questId}_{objectiveIndex}";

            if (ObjectiveProgress.TryGet(key, out int value)) { return value; }

            return 0;
        }

        public void SetObjectiveProgress(string questId, int objectiveIndex, int value)
        {
            if (!HasStateAuthority)
                return;

            string key = $"{questId}_{objectiveIndex}";

            ObjectiveProgress.Set(key, value);
            
        }
        
        public bool HasClaimedReward(string playerId, string questId)
        {
            string key = $"{playerId}_{questId}";

            return ClaimedRewards.ContainsKey(key);
        }

        public void MarkRewardClaimed(string playerId, string questId)
        {
            if (!HasStateAuthority)
                return;

            string key = $"{playerId}_{questId}";

            ClaimedRewards.Set(key, 1);
        }
    }
}