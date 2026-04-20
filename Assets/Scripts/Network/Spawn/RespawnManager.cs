using Fusion;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Network.Spawn
{
    public class RespawnManager : NetworkBehaviour
    {
        public static RespawnManager Instance;

        [Header("Respawn Points")]
        [SerializeField] private List<PlayerRespawnPoint> _points = new List<PlayerRespawnPoint>();

        [Networked] private int CurrentIndex { get; set; }

        public override void Spawned()
        {
            Instance = this;
        
            _points = _points.OrderBy(p => p.order).ToList();
        
            if (HasStateAuthority)
            {
                ActivatePoint(0);
            }
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Instance == this)
                Instance = null;
        }

        // ============================================================
        // ACTIVACIÓN PROGRESIVA
        // ============================================================

        public void UnlockNextPoint()
        {
            if (!HasStateAuthority) return;

            if (CurrentIndex < _points.Count - 1)
            {
                CurrentIndex++;
                ActivatePoint(CurrentIndex);

                Debug.Log($"Respawn point {CurrentIndex} unlocked");
            }
        }

        private void ActivatePoint(int index)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                _points[i].IsActive = (i == index);
            }
        }

        // ============================================================
        // GET SPAWN
        // ============================================================

        public Vector3 GetCurrentSpawnPosition()
        {
            if (_points.Count == 0)
                return Vector3.zero;

            return _points[CurrentIndex].GetPosition();
        }
    }
}