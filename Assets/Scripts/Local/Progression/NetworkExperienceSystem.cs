using System;
using Events;
using Fusion;
using UI;
using UnityEngine;

namespace Local.Progression
{
    public class NetworkExperienceSystem : NetworkBehaviour
    {
        public event Action<int> OnExperienceGained;
        public event Action<int> OnLevelUp;

        [Networked] private int CurrentExp { get; set; }
        [Networked] private int Level { get; set; } = 1;
        [Networked] private int BaseExp { get; set; } = 100;

        [SerializeField] private float percentageExp = 20f;
        [SerializeField] private int maxLevel = 50;

        private int _lastExp;
        private int _lastLevel;

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                ProgressionUI.Instance?.RegisterPlayer(this);
                
            }
        }

        private void OnEnable()
        {
            EventController.Instance.AddListener<EnemyDiedEvent>(OnEnemyDied);
        }
        
        private void OnDisable()
        {
            EventController.Instance.RemoveListener<EnemyDiedEvent>(OnEnemyDied);
        }


        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (Object.Runner.LocalPlayer == evt.killer)
            {
                RPC_RequestAddXP(evt.exp);
            }
            
        }
        
        // ==================================================
        // CLIENT -> SERVER
        // ==================================================
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_RequestAddXP(int amount)
        {
            Server_AddXP(amount);
        }

        // ==================================================
        // SERVER LOGIC
        // ==================================================
        private void Server_AddXP(int amount)
        {
            if (!HasStateAuthority) return;

            if (Level >= maxLevel)
            {
                if (CurrentExp != 0)
                    CurrentExp = 0;

                return;
            }

            CurrentExp += amount;

            while (CurrentExp >= BaseExp && Level < maxLevel)
            {
                int leftoverExp = CurrentExp - BaseExp;

                Level++;
                IncrementBaseExp();

                CurrentExp = leftoverExp;

                if (Level >= maxLevel)
                {
                    CurrentExp = 0;
                    return;
                }
            }
        }

        private void IncrementBaseExp()
        {
            float factor = 1f + (percentageExp / 100f);
            BaseExp = Mathf.CeilToInt((BaseExp * factor) / 10f) * 10;
        }

        // ==================================================
        // CLIENT SIDE (SYNC EVENTS)
        // ==================================================
        public override void Render()
        {
            // XP changed
            if (_lastExp != CurrentExp)
            {
                _lastExp = CurrentExp;
                OnExperienceGained?.Invoke(CurrentExp);
            }

            // Level changed
            if (_lastLevel != Level)
            {
                _lastLevel = Level;
                OnLevelUp?.Invoke(Level);
            }
        }

        // ==================================================
        // HELPERS
        // ==================================================
        public int GetCurrentXp() => CurrentExp;
        public int GetLevel() => Level;
        public int GetMaxExp() => BaseExp;
        public int MaxLevel => maxLevel;
    }
}