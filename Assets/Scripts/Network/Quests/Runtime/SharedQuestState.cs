// =====================================================
// SharedQuestState.cs
// =====================================================

using System.Collections.Generic;

namespace Network.Quests.Runtime
{
    /// <summary>
    /// Estado compartido por sala para Main Quests.
    /// </summary>
    public class SharedQuestState
    {
        private readonly HashSet<string>
            _completedMainQuests = new();

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
    }
}