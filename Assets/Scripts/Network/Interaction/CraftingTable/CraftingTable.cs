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
            RPC_OpenUI();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OpenUI()
        {
            if (!NetworkPlayerController.Local)
                return;

            CraftingUIController.Instance.Open(NetworkPlayerController.Local);
        }
    }
}