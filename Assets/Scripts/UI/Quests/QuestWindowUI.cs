using System.Collections.Generic;
using Events;
using Network.Interaction.QuestNPC;
using Network.Quests;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Tools.EventSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Quests
{
    public class QuestWindowUI : MonoBehaviour
    {
        public static QuestWindowUI Instance
        { get; private set; }

        [Header("List")]
        [SerializeField]
        private Transform questListContainer;

        [SerializeField]
        private QuestListEntryUI listEntryPrefab;

        [Header("Details")]
        [SerializeField]
        private QuestDetailsUI detailsUI;

        [Header("Buttons")]
        [SerializeField]
        private Button acceptButton;

        [SerializeField]
        private Button claimButton;

        [SerializeField]
        private Button closeButton;

        private readonly List<QuestListEntryUI>
            _entries = new();

        private QuestNpc _currentNpc;

        private QuestDefinitionSO _selectedQuest;

        private bool _selectedLocked;

        private void Awake()
        {
            Instance = this;

            gameObject.SetActive(false);
        }

        private void Start()
        {
            acceptButton.onClick
                .AddListener(
                    AcceptSelectedQuest);

            claimButton.onClick
                .AddListener(
                    ClaimSelectedQuest);

            closeButton.onClick
                .AddListener(
                    Close);

            EventController.Instance
                .AddListener<QuestUIRefreshEvent>(
                    OnQuestUIRefresh);
        }

        private void OnDestroy()
        {
            EventController.Instance
                .RemoveListener<QuestUIRefreshEvent>(
                    OnQuestUIRefresh);
        }

        public void Open(
            QuestNpc npc)
        {
            _currentNpc = npc;

            BuildQuestList();

            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void BuildQuestList()
        {
            ClearList();

            foreach (var quest
                     in _currentNpc
                         .questDatabase
                         .Quests)
            {
                bool locked =
                    NetworkQuestManager.Local
                        .IsQuestLocked(quest);

                QuestListEntryUI entry =
                    Instantiate(
                        listEntryPrefab,
                        questListContainer);

                entry.Bind(
                    quest,
                    this,
                    locked);

                _entries.Add(entry);
            }
        }

        private void ClearList()
        {
            foreach (var entry in _entries)
            {
                Destroy(entry.gameObject);
            }

            _entries.Clear();
        }

        public void SelectQuest(
            QuestDefinitionSO definition,
            bool locked)
        {
            _selectedQuest =
                definition;

            if (locked)
            {
                detailsUI.Clear();

                acceptButton.gameObject
                    .SetActive(false);

                claimButton.gameObject
                    .SetActive(false);

                return;
            }

            NetworkQuestManager.Local
                .TryGetQuest(
                    definition.questId,
                    out QuestRuntime runtime);

            detailsUI.ShowQuest(
                definition,
                runtime);

            RefreshButtons(runtime);
        }

        private void RefreshButtons(
            QuestRuntime runtime)
        {
            bool hasQuest =
                runtime != null;

            acceptButton.gameObject
                .SetActive(!hasQuest);

            claimButton.gameObject
                .SetActive(
                    hasQuest &&
                    runtime.State.isCompleted &&
                    !NetworkQuestManager.Local
                        .IsQuestRewardClaimed(
                            runtime.QuestId));
        }

        private void AcceptSelectedQuest()
        {
            if (_selectedQuest == null)
                return;

            if (_selectedLocked)
                return;

            NetworkQuestManager.Local
                .RPC_AcceptQuest(
                    _selectedQuest.questId);
        }

        private void ClaimSelectedQuest()
        {
            if (_selectedQuest == null)
                return;

            NetworkQuestManager.Local
                .RPC_ClaimReward(
                    _selectedQuest.questId);
        }

        private void OnQuestUIRefresh(
            QuestUIRefreshEvent evt)
        {
            if (!gameObject.activeSelf)
                return;

            BuildQuestList();
        }
    }
}