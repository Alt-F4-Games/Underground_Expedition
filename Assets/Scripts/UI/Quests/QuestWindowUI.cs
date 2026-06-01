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

        [Header("Root")]
        [SerializeField] private GameObject root;
        
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

            root.SetActive(false);
            
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
            if (npc == null)
                return;
            
            _currentNpc = npc;

            BuildQuestList();

            root.SetActive(true);
            InputManager.SetMode(InputMode.UI);
        }

        public void Close()
        {
            root.SetActive(false);
            InputManager.SetMode(InputMode.Game);
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
            _selectedQuest = definition;
            _selectedLocked = locked;

            if (locked)
            {
                string requiredQuestName =
                    definition.requiredQuestId;

                var requiredQuest =
                    QuestDatabase.Instance
                        .GetQuestById(
                            definition.requiredQuestId);

                if (requiredQuest != null)
                {
                    requiredQuestName =
                        requiredQuest.questName;
                }

                detailsUI.ShowLockedQuest(
                    definition,
                    requiredQuestName);

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
            if (_selectedQuest == null)
                return;

            bool hasQuest =
                runtime != null;

            bool completed =
                hasQuest &&
                runtime.State.isCompleted;

            bool claimed =
                hasQuest &&
                NetworkQuestManager.Local
                    .IsQuestRewardClaimed(
                        runtime.QuestId);

            bool locked =
                _selectedLocked;

            // ACCEPT

            acceptButton.gameObject.SetActive(
                !locked &&
                !hasQuest);

            // CLAIM

            claimButton.gameObject.SetActive(
                !locked &&
                completed &&
                !claimed);
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
            if (!root.activeSelf)
                return;

            BuildQuestList();

            if (_selectedQuest == null)
                return;

            SelectQuest(
                _selectedQuest,
                _selectedLocked);
        }
    }
}