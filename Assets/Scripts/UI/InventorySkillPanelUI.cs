using UnityEngine;
using TMPro;
using Skills.Core;
using Local.Progression;

namespace UI
{
    public class InventorySkillPanelUI : MonoBehaviour
    {
        public static InventorySkillPanelUI Instance;

        [Header("Skill Slots")]
        [SerializeField] private InventorySkillSlotUI _slot1UI;
        [SerializeField] private InventorySkillSlotUI _slot2UI;
        
        [Header("Global Info")]
        [SerializeField] private TextMeshProUGUI _availablePointsText;

        private NetworkLevelSystem _levelSystem;
        
        // Control flag to prevent double initialization
        private bool _isInitialized = false;

        private void Awake()
        {
            Instance = this;
        }

        // When the panel is enabled (Inventory opened), check if we missed the initialization
        private void OnEnable()
        {
            if (!_isInitialized)
            {
                // Find all skill managers in the scene
                PlayerSkillManager[] managers = FindObjectsOfType<PlayerSkillManager>();
                
                foreach (var manager in managers)
                {
                    if (manager.HasInputAuthority)
                    {
                        InitializeInventorySkills(manager);
                        break;
                    }
                }
            }
        }

        public void InitializeInventorySkills(PlayerSkillManager manager)
        {
            if (_isInitialized) return;

            _levelSystem = manager.GetComponent<NetworkLevelSystem>();

            // Setup each slot with its corresponding skill and index (1 or 2)
            if (_slot1UI != null)
                _slot1UI.AssignSkill(manager.Slot1, manager, _levelSystem, 1);
            
            if (_slot2UI != null)
                _slot2UI.AssignSkill(manager.Slot2, manager, _levelSystem, 2);

            // Subscribe to skill points changes to update the global label
            if (_levelSystem != null)
            {
                _levelSystem.OnSkillPointsChanged += UpdatePointsText;
                UpdatePointsText(_levelSystem.GetSkillPoints());
            }

            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (_levelSystem != null)
            {
                _levelSystem.OnSkillPointsChanged -= UpdatePointsText;
            }
        }

        private void UpdatePointsText(int points)
        {
            if (_availablePointsText != null)
            {
                _availablePointsText.text = $"Available skill points: {points}";
            }
        }
    }
}