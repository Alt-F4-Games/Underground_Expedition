using Events;
using Fusion;
using Network.Quests.Enums;
using Tools.EventSystem;
using UnityEngine;

namespace Network.Quests.World
{
    public class QuestExploreZone : MonoBehaviour
    {
        [Header("Zone")]
        [SerializeField]
        private string zoneId;

        [Header("Detection")]
        [SerializeField]
        private string playerTag = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;

            var netObj = other.GetComponent<NetworkObject>();
            if (netObj == null) return;

            if (!netObj.HasInputAuthority)
                return;

            NetworkQuestManager.Local.RPC_ReportQuestEvent(
                (int)QuestObjectiveType.ExploreArea,
                zoneId,
                1);
        }
    }
}