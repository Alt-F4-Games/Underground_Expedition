using Fusion;
using UnityEngine;
using Local.Progression;
using Health;

public class NetworkSessionManager : NetworkBehaviour
{
    private NetworkInventorySystem _inventory;
    private NetworkPlayerHealth _health;
    private NetworkPlayerController _controller;
    private NetworkExperienceSystem _experience;
    private NetworkLevelSystem _levelSystem;

    public static string LocalPlayerId => SystemInfo.deviceUniqueIdentifier;

    private string GetSessionKey()
    {
        string roomName = (Runner != null && Runner.SessionInfo.IsValid) ? Runner.SessionInfo.Name : "OfflineRoom";
        return $"{roomName}_{LocalPlayerId}";
    }

    public override void Spawned()
    {
        _inventory = GetComponent<NetworkInventorySystem>();
        _health = GetComponent<NetworkPlayerHealth>();
        _controller = GetComponent<NetworkPlayerController>();
        _experience = GetComponent<NetworkExperienceSystem>();
        _levelSystem = GetComponent<NetworkLevelSystem>();

        if (HasInputAuthority)
        {
            LoadLocalAndSyncToServer();
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority) SaveLocalSession();
    }

    private void OnApplicationQuit()
    {
        if (HasInputAuthority) SaveLocalSession();
    }
    
    private void LoadLocalAndSyncToServer()
    {
        var savedData = SessionSaveSystem.Load(GetSessionKey());
        
        if (savedData == null) savedData = new SavedSessionData();

        string json = JsonUtility.ToJson(savedData);
        RPC_SendSessionData(json);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SendSessionData(string json)
    {
        if (!HasStateAuthority || string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<SavedSessionData>(json);
        
        if (_inventory != null) _inventory.LoadFromSessionData(data);
        
        if (_experience != null) _experience.Server_SetProgression(data.level, data.currentExp, data.baseExp);
        if (_levelSystem != null) _levelSystem.Server_SetSkillPoints(data.skillPoints);
        
        if (_health != null)
        {
            if (data.currentHealth != -1) 
                _health.Server_SetHealth(data.currentHealth);
            else 
                _health.Server_SetHealth(_health.MaxHealth);
        }

        if (_controller != null)
        {
            if (data.currentStamina != -1f) 
                _controller.Server_SetStamina(data.currentStamina);
            else 
                _controller.Server_SetStamina(_controller.MaxStamina);
        }
        
        Debug.Log("[NetworkSession] Session successfully injected into the Host.");
    }
    
    public void SaveLocalSession()
    {
        var data = new SavedSessionData();

        // Gather data
        if (_inventory != null) _inventory.PopulateSessionData(data);
        if (_health != null) data.currentHealth = _health.CurrentHealth;
        if (_controller != null) data.currentStamina = _controller.CurrentStamina;
    
        if (_experience != null)
        {
            data.level = _experience.GetLevel();
            data.currentExp = _experience.GetCurrentXp();
            data.baseExp = _experience.GetMaxExp();
        }
    
        if (_levelSystem != null) data.skillPoints = _levelSystem.GetSkillPoints();

        // Write to disk
        SessionSaveSystem.Save(GetSessionKey(), data);
    }
}