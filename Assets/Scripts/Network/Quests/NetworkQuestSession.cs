using Fusion;

namespace Network.Quests
{
    /// <summary>
    /// Estado compartido de Main Quests para toda la sesión.
    /// Solo puede ser modificado por State Authority.
    /// </summary>
    public class NetworkQuestSession : NetworkBehaviour
    {
        public static NetworkQuestSession Instance
        {
            get;
            private set;
        }

        [Networked, Capacity(64)]
        public NetworkDictionary<NetworkString<_32>, byte>
            AcceptedMainQuests => default;

        [Networked, Capacity(64)]
        public NetworkDictionary<NetworkString<_32>, byte>
            CompletedMainQuests => default;

        public override void Spawned()
        {
            Instance = this;
        }

        public override void Despawned(
            NetworkRunner runner,
            bool hasState)
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public bool IsMainQuestAccepted(
            string questId)
        {
            return AcceptedMainQuests.ContainsKey(
                questId);
        }

        public bool IsMainQuestCompleted(
            string questId)
        {
            return CompletedMainQuests.ContainsKey(
                questId);
        }

        public void MarkMainQuestAccepted(
            string questId)
        {
            if (!HasStateAuthority)
                return;

            AcceptedMainQuests.Set(
                questId,
                1);
        }

        public void MarkMainQuestCompleted(
            string questId)
        {
            if (!HasStateAuthority)
                return;

            CompletedMainQuests.Set(
                questId,
                1);
        }
    }
}