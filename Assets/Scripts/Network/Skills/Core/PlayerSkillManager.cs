using Fusion;
using Network;
using UnityEngine;
using UI;
using Events;

namespace Skills.Core
{
    public class PlayerSkillManager : NetworkBehaviour
    {
        [Header("Skill Slots")]
        [SerializeField] private NetworkSkill _slot1;
        [SerializeField] private NetworkSkill _slot2;
        
        // PUBLIC PROPERTIES FOR UI
        public NetworkSkill Slot1 => _slot1;
        public NetworkSkill Slot2 => _slot2;

        [Networked] 
        private NetworkButtons _previousButtons { get; set; }
        
        private void OnEnable()
        {
            EventController.Instance.AddListener<SkillPointConsumedEvent>(OnSkillPointConsumed);
        }
        
        private void OnDisable()
        {
            EventController.Instance.RemoveListener<SkillPointConsumedEvent>(OnSkillPointConsumed);
        }

        public override void Spawned()
        {
            base.Spawned();

            // Hook for the UI to read the skills when the local player spawns
            if (HasInputAuthority)
            {
                if (PlayerSkillUIManager.Instance != null)
                    PlayerSkillUIManager.Instance.InitializeSkillBar(this);
                
                if (InventorySkillPanelUI.Instance != null)
                    InventorySkillPanelUI.Instance.InitializeInventorySkills(this);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputPlayer input))
            {
                NetworkButtons pressedButtons = input.Buttons.GetPressed(_previousButtons);
                _previousButtons = input.Buttons;
                
                bool isUpgradeModifierHeld = input.Buttons.IsSet(NetworkInputPlayer.UPGRADE_MODIFIER);
                
                if (pressedButtons.IsSet(NetworkInputPlayer.SKILL1_BUTTON))
                {
                    if (isUpgradeModifierHeld)
                        RPC_RequestUpgradeSkill(1);
                    else if (_slot1 != null && _slot1.CanCast(Runner))
                    {
                        _slot1.OnExecute(Runner);
                    }
                }
                
                if (pressedButtons.IsSet(NetworkInputPlayer.SKILL2_BUTTON))
                {
                    if (isUpgradeModifierHeld)
                        RPC_RequestUpgradeSkill(2);
                    else if (_slot2 != null && _slot2.CanCast(Runner))
                    {
                        _slot2.OnExecute(Runner);
                    }
                }
            }
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestUpgradeSkill(int slotIndex)
        {
            if (!HasStateAuthority) return;

            // Validation to prevent consuming points if the skill is already maxed
            NetworkSkill targetSkill = (slotIndex == 1) ? _slot1 : _slot2;
            if (targetSkill != null && targetSkill.Data != null)
            {
                if (targetSkill.CurrentLevel >= targetSkill.Data.MaxLevel)
                {
                    Debug.Log($"[SERVER] Upgrade denied: Slot {slotIndex} is already at Max Level.");
                    return;
                }
            }

            EventController.Instance.TriggerEvent(new SkillUpgradeRequestedEvent 
            { 
                Player = Object, 
                SlotIndex = slotIndex 
            });
        }
        
        private void OnSkillPointConsumed(SkillPointConsumedEvent evt)
        {
            if (!HasStateAuthority) return;

            if (evt.Player == Object)
            {
                if (evt.SlotIndex == 1 && _slot1 != null)
                {
                    _slot1.UpgradeSkill();
                    Debug.Log($"[SERVER] Slot 1 upgraded to Level {_slot1.CurrentLevel}");
                }
                else if (evt.SlotIndex == 2 && _slot2 != null)
                {
                    _slot2.UpgradeSkill();
                    Debug.Log($"[SERVER] Slot 2 upgraded to Level {_slot2.CurrentLevel}");
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