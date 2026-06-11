using TMPro;
using UnityEngine;

namespace UI.Quests
{
    public class QuestNotificationEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;

        public void SetMessage(string message, Color color)
        {
            messageText.text = message;
            messageText.color = color;
        }
    }
}