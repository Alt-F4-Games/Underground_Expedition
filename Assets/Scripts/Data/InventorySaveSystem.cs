using UnityEngine;
using System.IO;

/// <summary>
/// Simple JSON save/load system for player inventory.
/// Saves to: Application.persistentDataPath/inv_<PlayerID>.json
/// </summary>
public static class InventorySaveSystem
{
    private static string GetPath(string playerId)
        => Path.Combine(Application.persistentDataPath, $"inv_{playerId}.json");

    public static void Save(string playerId, SavedInventoryData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(playerId), json);
#if UNITY_EDITOR
        Debug.Log($"[InventorySaveSystem] Saved inventory for {playerId} to {GetPath(playerId)}");
#endif
    }

    public static SavedInventoryData Load(string playerId)
    {
        string path = GetPath(playerId);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SavedInventoryData>(json);
    }
}