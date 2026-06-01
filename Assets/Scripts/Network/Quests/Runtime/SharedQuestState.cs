// =====================================================
// SharedQuestState.cs
// =====================================================

using System.Collections.Generic;

namespace Network.Quests.Runtime
{
    /// <summary>
    /// Estado compartido por sesión para Main Quests.
    /// </summary>
    public class SharedQuestState
    {
        private readonly HashSet<string>
            _acceptedMainQuests = new();

        private readonly HashSet<string>
            _completedMainQuests = new();

        public bool IsAccepted(
            string questId)
        {
            return _acceptedMainQuests.Contains(
                questId);
        }

        public void MarkAccepted(
            string questId)
        {
            _acceptedMainQuests.Add(
                questId);
        }

        public bool IsCompleted(
            string questId)
        {
            return _completedMainQuests.Contains(
                questId);
        }

        public void MarkCompleted(
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