using UnityEngine;
using System.IO;

public static class SessionSaveSystem
{
    private static string GetPath(string playerId) 
    {
        return Path.Combine(Application.persistentDataPath, $"session_{playerId}.json");
    }

    public static void Save(string playerId, SavedSessionData data)
    {
        if (string.IsNullOrWhiteSpace(playerId) || data == null) return;
        try 
        { 
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetPath(playerId), json); 
            Debug.Log($"[Session] Sesión guardada en: {GetPath(playerId)}");
        }
        catch (System.Exception ex) { Debug.LogError($"[Session] Error al guardar: {ex}"); }
    }

    public static SavedSessionData Load(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId) || !File.Exists(GetPath(playerId))) return null;
        try 
        { 
            return JsonUtility.FromJson<SavedSessionData>(File.ReadAllText(GetPath(playerId))); 
        }
        catch (System.Exception ex) { Debug.LogError($"[Session] Error al cargar: {ex}"); return null; }
    }
}