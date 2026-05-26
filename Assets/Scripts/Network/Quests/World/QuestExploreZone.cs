using Events;
using Fusion;
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

            var networkObject =
                other.GetComponent<NetworkObject>();

            if (networkObject == null)
                return;

            ZoneDiscoveredEvent zoneDiscoveredEvent = new ZoneDiscoveredEvent
            {
                player = networkObject.InputAuthority,
                zoneId = zoneId
            };
            
            EventController.Instance.TriggerEvent(zoneDiscoveredEvent);

            Debug.Log(
                $"[QuestExploreZone] Player discovered zone: {zoneId}");
        }
    }
}