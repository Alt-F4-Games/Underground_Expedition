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
            if (!NetworkPlayerController.Local)
                return;

            Debug.Log("Quest database loaded");
            QuestWindowUI.Instance.Open(this);
        }
    }
}