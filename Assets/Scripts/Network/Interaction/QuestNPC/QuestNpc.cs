using Fusion;
using Network.Quests;
using UI.Quests;
using UnityEngine;

namespace Network.Interaction.QuestNPC
{
    public class QuestNpc : InteractableBase, ILocalInteractable
    {
        [Header("Quest Database")]
        [SerializeField]
        internal QuestDatabase questDatabase;

        public override void OnInteract(NetworkPlayerController player)
        {
            
        }
        
        public void OnLocalInteract()
        {
            QuestWindowUI.Instance.Open(this);
        }
    }
}