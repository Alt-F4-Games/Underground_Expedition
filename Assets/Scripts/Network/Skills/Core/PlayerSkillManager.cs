using Fusion;
using Network;
using UnityEngine;

namespace Skills.Core
{
    public class PlayerSkillManager : NetworkBehaviour
    {
        [Header("Skill Slots")]
        [SerializeField] private NetworkSkill _slot1;
        [SerializeField] private NetworkSkill _slot2;
        
        [Networked] 
        private NetworkButtons _previousButtons { get; set; }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputPlayer input))
            {
                NetworkButtons pressedButtons = input.Buttons.GetPressed(_previousButtons);
                _previousButtons = input.Buttons;
                
                if (pressedButtons.IsSet(NetworkInputPlayer.SKILL1_BUTTON))
                {
                    if (_slot1 != null && _slot1.CanCast(Runner))
                    {
                        _slot1.OnExecute(Runner);
                    }
                }
                
                if (pressedButtons.IsSet(NetworkInputPlayer.SKILL2_BUTTON))
                {
                    if (_slot2 != null && _slot2.CanCast(Runner))
                    {
                        _slot2.OnExecute(Runner);
                    }
                }
            }
        }

        // ============================================================
        // EXTERNAL INTERFACE (For the combat system)
        // ============================================================

        /// <summary>
        /// Processes the base damage through all equipped skills 
        /// and returns the final calculated damage polymorphically.
        /// </summary>
        public int GetModifiedDamage(int baseDamage)
        {
            int currentDamage = baseDamage;

            if (_slot1 != null) currentDamage = _slot1.ModifyAttackDamage(currentDamage);
            if (_slot2 != null) currentDamage = _slot2.ModifyAttackDamage(currentDamage);

            return currentDamage;
        }
    }
}