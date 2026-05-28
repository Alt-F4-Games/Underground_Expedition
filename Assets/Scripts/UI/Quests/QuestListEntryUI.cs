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

        private bool _locked;

        public void Bind(
            QuestDefinitionSO definition,
            QuestWindowUI window,
            bool locked)
        {
            _definition = definition;

            _window = window;

            _locked = locked;

            questNameText.text =
                locked
                    ? "????"
                    : definition.questName;

            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(
                SelectQuest);
        }

        private void SelectQuest()
        {
            _window.SelectQuest(
                _definition,
                _locked);
        }
    }
}