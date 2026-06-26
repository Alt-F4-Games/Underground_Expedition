using Fusion;
using Network.Interaction.Altar;
using Network.Spawn;
using UnityEngine;
using UnityEngine.AI;

namespace Network.Environment
{
    public class AltarActivation : NetworkBehaviour
    {
        [SerializeField] private NavMeshObstacle _obstacle;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Collider _collider;
        [SerializeField] private ResurrectionAltarRespawn _resurrectionAltarRespawn;
        [SerializeField] private BossProximitySpawner _bossProximitySpawner;

        [Networked, OnChangedRender(nameof(OnDoorStateChanged))]
        public NetworkBool IsOpen { get; set; }

        private void OnEnable()
        {
            _resurrectionAltarRespawn.Altar1Activated += OpenDoor;
            _bossProximitySpawner.BossSpawnedEvent += CloseDoor;
        }

        private void OnDisable()
        {
            _resurrectionAltarRespawn.Altar1Activated -= OpenDoor;
            _bossProximitySpawner.BossSpawnedEvent -= CloseDoor;
        }

        private void OpenDoor()
        {
            if (!HasStateAuthority)
                return;

            IsOpen = true;
        }

        private void CloseDoor()
        {
            if (!HasStateAuthority)
                return;

            IsOpen = false;
        }

        private void OnDoorStateChanged()
        {
            bool open = IsOpen;

            _collider.enabled = !open;
            _renderer.enabled = !open;
            _obstacle.enabled = !open;
        }
    }
}