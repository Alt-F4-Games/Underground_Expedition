using System.Collections.Generic;
using Events;
using Network.Quests;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Network.Quests.World;
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
        
        private QuestRuntime _cachedRuntime;

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
            EventController.Instance.RemoveListener<QuestUIRefreshEvent>(
                OnQuestUIRefresh);;
        }

        // =====================================================
        // OPEN
        // =====================================================

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

        // =====================================================
        // BUILD
        // =====================================================

        private void BuildQuestList()
        {
            ClearList();

            foreach (var quest
                     in _currentNpc
                         .questDatabase
                         .GetAllQuests())
            {
                QuestListEntryUI entry =
                    Instantiate(
                        listEntryPrefab,
                        questListContainer);

                entry.Bind(quest, this);

                _entries.Add(entry);
            }
        }

        private void ClearList()
        {
            foreach (var entry
                     in _entries)
            {
                Destroy(entry.gameObject);
            }

            _entries.Clear();
        }

        // =====================================================
        // SELECTION
        // =====================================================

        public void SelectQuest(
            QuestDefinitionSO definition)
        {
            _selectedQuest =
                definition;

            NetworkQuestManager.Local
                .TryGetQuest(
                    definition.questId,
                    out QuestRuntime runtime);

            detailsUI.ShowQuest(
                definition,
                runtime);

            RefreshButtons(runtime);
        }

        // =====================================================
        // BUTTONS
        // =====================================================

        private void RefreshButtons(
            QuestRuntime runtime)
        {
            acceptButton.gameObject
                .SetActive(runtime == null);

            claimButton.gameObject
                .SetActive(
                    runtime != null &&
                    runtime.IsQuestFinished());
        }

        private void AcceptSelectedQuest()
        {
            if (_selectedQuest == null)
                return;

            NetworkQuestManager.Local
                .RPC_AcceptQuest(
                    _selectedQuest.questId);

            SelectQuest(
                _selectedQuest);
        }

        private void ClaimSelectedQuest()
        {
            if (_selectedQuest == null)
                return;

            NetworkQuestManager.Local
                .RPC_ClaimReward(
                    _selectedQuest.questId);

            SelectQuest(
                _selectedQuest);
        }
        
        private void OnQuestUIRefresh(
            QuestUIRefreshEvent evt)
        {
            if (!gameObject.activeSelf)
                return;

            if (_selectedQuest == null)
                return;

            NetworkQuestManager.Local
                .TryGetQuest(
                    _selectedQuest.questId,
                    out QuestRuntime runtime);

            _cachedRuntime = runtime;

            detailsUI.ShowQuest(
                _selectedQuest,
                runtime);

            RefreshButtons(runtime);
        }
    }
}