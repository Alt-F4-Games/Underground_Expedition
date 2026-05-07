using Fusion;
using Network.Interaction;
using UI.Crafting;
using UnityEngine;


namespace Network.Interaction.CraftingTable
{
    public class CraftingTable : InteractableBase
    {
        public override void OnInteract(NetworkPlayerController player)
        {
            RPC_OpenUI(player.Object.InputAuthority);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void RPC_OpenUI([RpcTarget] PlayerRef playerRef)
        {
            CraftingUIController.Instance.Open(NetworkPlayerController.Local);
        }
    }
}