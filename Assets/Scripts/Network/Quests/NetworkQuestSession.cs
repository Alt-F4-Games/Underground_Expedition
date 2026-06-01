using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Network.Quests
{
    /// <summary>
    /// Estado compartido de quests para toda la sesión.
    /// Solamente almacena Main Quests.
    /// El Host tiene State Authority.
    /// </summary>
    public class NetworkQuestSession : NetworkBehaviour
    {
        public static NetworkQuestSession Instance
        {
            get;
            private set;
        }

        private readonly HashSet<string>
            _acceptedMainQuests = new();

        private readonly HashSet<string>
            _completedMainQuests = new();

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
            return _acceptedMainQuests.Contains(
                questId);
        }

        public bool IsMainQuestCompleted(
            string questId)
        {
            return _completedMainQuests.Contains(
                questId);
        }

        public void MarkMainQuestAccepted(
            string questId)
        {
            _acceptedMainQuests.Add(
                questId);
        }

        public void MarkMainQuestCompleted(
            string questId)
        {
            _completedMainQuests.Add(
                questId);
        }

        public IEnumerable<string>
            AcceptedMainQuests =>
            _acceptedMainQuests;
    }
}