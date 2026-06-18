using Fusion;
using Network.Interaction;
using UI.Crafting;
using UnityEngine;


namespace Network.Interaction.CraftingTable
{
    public class CraftingTable : InteractableBase, ILocalInteractable
    {
        public override void OnInteract(
            NetworkPlayerController player)
        {
        }

        public void OnLocalInteract()
        {
            CraftingUIController.Instance.Open(
                NetworkPlayerController.Local);
        }
    }
}