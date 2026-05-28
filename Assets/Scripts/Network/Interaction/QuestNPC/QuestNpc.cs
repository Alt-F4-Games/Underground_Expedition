using Fusion;
using Network.Quests;
using UI.Quests;
using UnityEngine;

namespace Network.Interaction.QuestNPC
{
    public class QuestNpc : InteractableBase
    {
        [Header("Quest Database")]
        [SerializeField]
        internal QuestDatabase questDatabase;

        public override void OnInteract(NetworkPlayerController player)
        {
            if (!player.Object.HasInputAuthority)
                return;

            QuestWindowUI.Instance.Open(this);
        }
    }
}