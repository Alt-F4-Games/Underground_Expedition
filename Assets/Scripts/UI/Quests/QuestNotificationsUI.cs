using System.Collections;
using Network.Quests.Enums;
using UnityEngine;

namespace UI.Quests
{
    public class QuestNotificationsUI : MonoBehaviour
    {
        public static QuestNotificationsUI Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform content;
        [SerializeField] private QuestNotificationEntryUI notificationPrefab;

        [Header("Settings")]
        [SerializeField] private float notificationDuration = 4f;
        [SerializeField] private Color acceptedColor = Color.yellow;
        [SerializeField] private Color completedColor = Color.green;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ShowNotification(QuestNotificationType type, string questName)
        {
            string message = BuildMessage(type, questName);

            Color color = GetColor(type);

            QuestNotificationEntryUI entry = Instantiate(notificationPrefab, content);

            entry.transform.SetAsFirstSibling();

            entry.SetMessage(message, color);

            StartCoroutine(RemoveAfterTime(entry.gameObject));
        }
        
        private Color GetColor(QuestNotificationType type)
        {
            return type switch
            {
                QuestNotificationType.Accepted => acceptedColor,
                QuestNotificationType.Completed => completedColor,
                _ => Color.white
            };
        }

        private string BuildMessage(QuestNotificationType type, string questName)
        {
            return type switch
            {
                QuestNotificationType.Accepted =>
                    $"Quest Accepted: {questName}",

                QuestNotificationType.Completed =>
                    $"Quest Completed: {questName}",

                _ => questName
            };
        }

        private IEnumerator RemoveAfterTime(GameObject notification)
        {
            yield return new WaitForSeconds(notificationDuration);

            if (notification)
                Destroy(notification);
        }
    }
}