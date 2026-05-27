using Network.Quests.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Quests
{
    public class QuestListEntryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private TMP_Text questNameText;

        [SerializeField]
        private Button button;

        private QuestDefinitionSO _definition;

        private QuestWindowUI _window;

        // =====================================================
        // SETUP
        // =====================================================

        public void Bind(
            QuestDefinitionSO definition,
            QuestWindowUI window)
        {
            _definition = definition;

            _window = window;

            questNameText.text =
                definition.questName;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                SelectQuest);
        }

        // =====================================================
        // EVENTS
        // =====================================================

        private void SelectQuest()
        {
            _window.SelectQuest(
                _definition);
        }
    }
}