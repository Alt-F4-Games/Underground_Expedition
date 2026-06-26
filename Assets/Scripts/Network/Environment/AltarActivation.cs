using System;
using Network.Interaction.Altar;
using Network.Spawn;
using UnityEngine;
using UnityEngine.AI;

namespace Network.Environment
{
    public class AltarActivation : MonoBehaviour
    {
      [SerializeField] private NavMeshObstacle _obstacle;
      [SerializeField] private MeshRenderer _renderer;
      [SerializeField] private Collider _collider;
      [SerializeField] private ResurrectionAltarRespawn _resurrectionAltarRespawn;
      [SerializeField] private BossProximitySpawner _bossProximitySpawner;

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
          _collider.enabled = false;
          _renderer.enabled = false;
          _obstacle.enabled = false;
      }

      private void CloseDoor()
      {
          _collider.enabled = true;
          _renderer.enabled = true;
          _obstacle.enabled = true;
      }
    }
}