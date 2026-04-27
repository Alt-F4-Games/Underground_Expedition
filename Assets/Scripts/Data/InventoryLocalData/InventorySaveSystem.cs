using UnityEngine;
using System.IO;

/// <summary>
/// Lightweight JSON-based save system for player inventory.
/// Each player's inventory is stored separately in:
///
///     Application.persistentDataPath / inv_<PlayerID>.json
///
/// This system:
/// - Serializes inventory to JSON
/// - Writes/reads local files
/// - Handles missing files gracefully
/// - Is fully static and can be called from anywhere
/// </summary>
public static class InventorySaveSystem
{
    // ------------------------------------------------------------
    //  PATH GENERATION
    // ------------------------------------------------------------
    
    private static string GetPath(string playerId)      // Builds the absolute path where the inventory JSON will be stored.
    {
        return Path.Combine(Application.persistentDataPath, $"inv_{playerId}.json");
    }

    // ------------------------------------------------------------
    //  SAVE
    // ------------------------------------------------------------
    
    public static void Save(string playerId, SavedInventoryData data)   // Saves the given SavedInventoryData to disk for this player.
    {
        if (string.IsNullOrWhiteSpace(playerId)) { Debug.LogError("[InventorySaveSystem] Save failed: PlayerID is null or empty."); return; }

        if (data == null) { Debug.LogError("[InventorySaveSystem] Save failed: Data is null."); return; }

        string path = GetPath(playerId);

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);

#if UNITY_EDITOR
            Debug.Log($"[InventorySaveSystem] Saved inventory for '{playerId}' at:\n  {path}");
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InventorySaveSystem] Failed to save inventory for '{playerId}'.\n{ex}");
        }
    }

    // ------------------------------------------------------------
    //  LOAD
    // ------------------------------------------------------------
    
    //  Returns:
    // - SavedInventoryData if found
    // - null if the file does not exist or fails to load
    
    public static SavedInventoryData Load(string playerId)      // Loads the player's inventory from disk.
    {
        if (string.IsNullOrWhiteSpace(playerId)) { Debug.LogError("[InventorySaveSystem] Load failed: PlayerID is null or empty."); return null; }

        string path = GetPath(playerId);

        if (!File.Exists(path))
        {
#if UNITY_EDITOR
            Debug.Log($"[InventorySaveSystem] No save file found for '{playerId}'. (Expected: {path})");
#endif
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<SavedInventoryData>(json);

            if (data == null)
            {
                Debug.LogWarning($"[InventorySaveSystem] JSON file exists but could not be parsed for '{playerId}'.");
            }

            return data;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[InventorySaveSystem] Failed to load inventory for '{playerId}'.\n{ex}");
            return null;
        }
    }
}