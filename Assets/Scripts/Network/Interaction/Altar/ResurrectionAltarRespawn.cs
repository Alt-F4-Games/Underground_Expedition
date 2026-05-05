using Fusion;
using UnityEngine;
using Network.Spawn;

namespace Network.Interaction.Altar
{
    public class ResurrectionAltarRespawn : InteractableBase
    {
        private PlayerRespawnPoint _respawnPoint;

        private void Awake()
        {
            _respawnPoint = GetComponent<PlayerRespawnPoint>();

            if (_respawnPoint == null)
            {
                Debug.LogError($"No PlayerRespawnPoint found on {gameObject.name}");
            }
        }

        public override bool CanInteract(PlayerRef player)
        {
            if (_respawnPoint == null)
                return false;
            
            if (_respawnPoint.WasActivated)
                return false;

            return true;
        }

        public override void OnInteract(NetworkPlayerController player)
        {
            if (_respawnPoint == null)
                return;
            
            RPC_RequestActivation();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestActivation(RpcInfo info = default)
        {
            if (_respawnPoint == null)
                return;

            Debug.Log($"Altar {name} activated by player");

            RespawnManager.Instance.ActivatePoint(_respawnPoint);
        }
    }
}