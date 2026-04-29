using System;
using Fusion;
using Player;
using UnityEngine;

namespace Local.Progression
{
    public class NetworkLevelSystem : NetworkBehaviour
    {
        public event Action<int> OnSkillPointsChanged;

        [Networked] private int SkillPoints { get; set; }

        [SerializeField] private int levelsPerSkillPoint = 3;

        private int _lastSkillPoints;
        private NetworkExperienceSystem _expSystem;

        public override void Spawned()
        {
            _expSystem = GetComponent<NetworkExperienceSystem>();

            if (_expSystem != null)
            {
                _expSystem.OnLevelUp += HandleLevelUp;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            
            if (!Object.HasStateAuthority) return;

            bool shouldGivePoint = newLevel % levelsPerSkillPoint == 0 || newLevel >= _expSystem.GetLevel() && newLevel == GetMaxLevel();

            if (shouldGivePoint)
            {
                SkillPoints++;
                Debug.Log("Skill points: " + SkillPoints);
            }
        }

        // ==================================================
        // SYNC CLIENTS
        // ==================================================
        public override void Render()
        {
            if (_lastSkillPoints != SkillPoints)
            {
                _lastSkillPoints = SkillPoints;
                OnSkillPointsChanged?.Invoke(SkillPoints);
            }
        }

        // ==================================================
        // HELPERS
        // ==================================================
        public int GetSkillPoints() => SkillPoints;

        private int GetMaxLevel()
        {
            return _expSystem != null ? _expSystem.MaxLevel : 0;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_expSystem != null)
            {
                _expSystem.OnLevelUp -= HandleLevelUp;
            }
        }
    }
}