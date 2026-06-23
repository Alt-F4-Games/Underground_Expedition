using System;
using Events;
using Fusion;
using Network;
using Tools.EventSystem;
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
                // NUEVO: Cargar datos locales y enviarlos al host al nacer
                LoadLocalAndSyncToServer();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (HasInputAuthority) SaveLocalProgression();
        }

        private void OnApplicationQuit()
        {
            if (HasInputAuthority) SaveLocalProgression();
        }

        private void OnEnable()
        {
            EventController.Instance.AddListener<EnemyDiedEvent>(OnEnemyDied);
            EventController.Instance.AddListener<PlayerDiedEvent>(OnPlayerDead);
        }
        
        private void OnDisable()
        {
            EventController.Instance.RemoveListener<EnemyDiedEvent>(OnEnemyDied);
            EventController.Instance.RemoveListener<PlayerDiedEvent>(OnPlayerDead);
        }

        // ==================================================
        // SERVER LOGIC (Eventos escuchados en el Servidor)
        // ==================================================

        private void OnEnemyDied(EnemyDiedEvent evt)
        {
            if (!HasStateAuthority) return;
            
            if (Object.InputAuthority == evt.killer)
            {
                Server_AddXP(evt.exp);
            }
            
        }

        private void OnPlayerDead(PlayerDiedEvent evt)
        {
            if (!HasStateAuthority) return;
            
            ResetActualXP(evt.IsAlive);
        }

        // ==================================================
        // SERVER LOGIC
        // ==================================================
        public void Server_AddXP(int amount)
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
                ApplyLevelUpStats();
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

        private void ResetActualXP(bool isAlive)
        {
            if (!HasStateAuthority) return;

            if (!isAlive)
            {
                if (CurrentExp != 0)
                    CurrentExp = 0;
            }
        }

        private void ApplyLevelUpStats()
        {
            var stats = GetComponent<PlayerStatsManager>();
            if (stats != null)
            {
                stats.ApplyStatsServer();
            }
        }
        
        // ==================================================
        // CLIENT SIDE (SYNC EVENTS FOR UI)
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

        // ==================================================
        // PERSISTENCIA DE SESIÓN (NUEVA IMPLEMENTACIÓN)
        // ==================================================

        public static string LocalPlayerId => SystemInfo.deviceUniqueIdentifier;

        private string GetSessionKey()
        {
            string roomName = (Runner != null && Runner.SessionInfo.IsValid) ? Runner.SessionInfo.Name : "OfflineRoom";
            return $"{roomName}_{LocalPlayerId}_Progression"; // Sufijo para no pisar el inventario
        }

        private void LoadLocalAndSyncToServer()
        {
            if (!HasInputAuthority) return;

            string id = GetSessionKey();
            string json = PlayerPrefs.GetString(id, ""); 
            
            if (string.IsNullOrEmpty(json)) return;

            RPC_SendSavedProgressionJson(json);
        }

        public void SaveLocalProgression()
        {
            if (!HasInputAuthority) return;

            int sp = 0;
            if (TryGetComponent(out NetworkLevelSystem levelSystem))
                sp = levelSystem.GetSkillPoints();

            var data = new SavedProgressionData
            {
                level = Level,
                currentExp = CurrentExp,
                baseExp = BaseExp,
                skillPoints = sp
            };

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(GetSessionKey(), json);
            PlayerPrefs.Save();
            
            Debug.Log($"[Progression] Saved progression for Session-Player key: {GetSessionKey()}");
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SendSavedProgressionJson(string json)
        {
            if (!HasStateAuthority || string.IsNullOrEmpty(json)) return;

            try
            {
                var saved = JsonUtility.FromJson<SavedProgressionData>(json);
                if (saved != null)
                {
                    Level = saved.level;
                    CurrentExp = saved.currentExp;
                    BaseExp = saved.baseExp;
                    
                    if (TryGetComponent(out NetworkLevelSystem levelSystem))
                    {
                        levelSystem.Server_SetSkillPoints(saved.skillPoints);
                    }

                    // CRÍTICO: Reaplicar los stats pasivos según el nivel cargado
                    if (saved.level > 1 && TryGetComponent(out PlayerStatsManager stats))
                    {
                        for (int i = 0; i < (saved.level - 1); i++)
                        {
                            stats.ApplyStatsServer();
                        }
                    }

                    Debug.Log("[Progression] Server applied saved progression from client.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Progression] Failed to parse saved JSON: {ex.Message}");
            }
        }
    }

    // CLASE DE DATOS PARA SERIALIZAR
    [Serializable]
    public class SavedProgressionData
    {
        public int level;
        public int currentExp;
        public int baseExp;
        public int skillPoints;
    }
}