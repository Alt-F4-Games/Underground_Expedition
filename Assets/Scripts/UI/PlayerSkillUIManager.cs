using UnityEngine;
using Skills.Core;

namespace UI
{
    public class PlayerSkillUIManager : MonoBehaviour
    {
        public static PlayerSkillUIManager Instance;

        [SerializeField] private SkillSlotUI _slot1UI;
        [SerializeField] private SkillSlotUI _slot2UI;

        private void Awake()
        {
            Instance = this;
        }

        // Called by PlayerSkillManager when the local player spawns
        public void InitializeSkillBar(PlayerSkillManager manager)
        {
            _slot1UI.AssignSkill(manager.Slot1);
            _slot2UI.AssignSkill(manager.Slot2);
        }
    }
}