using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace Network.SceneManagement
{
    [RequireComponent(typeof(Collider))]
    public class PlayerAreaGate : NetworkBehaviour
    {
        private readonly HashSet<PlayerRef> _playersInside = new();

        public bool AreAllPlayersInside
        {
            get
            {
                if (Runner == null) return false;
                return _playersInside.Count == Runner.ActivePlayers.Count();
            }
        }

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Object.HasStateAuthority) return;

            var no = other.GetComponentInParent<NetworkObject>();
            if (no == null) return;

            _playersInside.Add(no.InputAuthority);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Object.HasStateAuthority) return;

            var no = other.GetComponentInParent<NetworkObject>();
            if (no == null) return;

            _playersInside.Remove(no.InputAuthority);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _playersInside.Clear();
        }
    }
}