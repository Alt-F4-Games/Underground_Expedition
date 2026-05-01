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
        [SerializeField] private List<PlayerRespawnPoint> _points = new();
        
        private List<PlayerRespawnPoint> _activationStack = new();
        
        [Networked] private int CurrentIndex { get; set; }

        public override void Spawned()
        {
            Instance = this;
            
            if (_points == null || _points.Count == 0)
            {
                _points = GetComponentsInChildren<PlayerRespawnPoint>()
                    .OrderBy(p => p.order)
                    .ToList();
            }
            else
            {
                _points = _points.OrderBy(p => p.order).ToList();
            }

            if (HasStateAuthority && _points.Count > 0)
            {
                _activationStack.Clear();
                _activationStack.Add(_points[0]);
                CurrentIndex = 0;

                UpdateActivePoints();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Instance == this)
                Instance = null;
        }

        // ============================================================
        // ACTIVATION
        // ============================================================

        public void ActivatePoint(PlayerRespawnPoint point)
        {
            if (!HasStateAuthority) return;
            if (point == null) return;

            if (_activationStack.Contains(point))
                return;

            _activationStack.Add(point);

            point.WasActivated = true;

            CurrentIndex = _points.IndexOf(point);

            UpdateActivePoints();
        }

        // ============================================================
        // DEACTIVATION
        // ============================================================

        public void DeactivateLastPoint()
        {
            if (!HasStateAuthority) return;

            if (_activationStack.Count <= 1)
                return;

            _activationStack.RemoveAt(_activationStack.Count - 1);

            var newCurrent = _activationStack[^1];
            CurrentIndex = _points.IndexOf(newCurrent);

            Debug.Log($"Respawn rolled back to {newCurrent.name}");

            UpdateActivePoints();
        }

        // ============================================================
        // STATE SYNC
        // ============================================================

        private void UpdateActivePoints()
        {
            if (!HasStateAuthority) return;

            for (int i = 0; i < _points.Count; i++)
            {
                _points[i].IsActive = (i == CurrentIndex);
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
        
        public PlayerRespawnPoint GetCurrentPoint()
        {
            if (_activationStack.Count == 0)
                return null;

            return _activationStack[^1];
        }
    }
}