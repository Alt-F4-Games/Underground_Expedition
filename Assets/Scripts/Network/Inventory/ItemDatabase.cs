using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    // ------------------------------------------------------------
    //  STATIC ACCESS
    // ------------------------------------------------------------
    public static ItemDatabase Instance { get; private set; }   

    // ------------------------------------------------------------
    //  ENTRY STRUCTURE
    // ------------------------------------------------------------
    [System.Serializable]
    public struct Entry
    {
        [Header("Network")]
        public int networkId;                 // Stable ID used in networking and saves
        
        [Header("Gameplay")]
        public string gameplayId;
        
        [Header("Data")]
        public ItemSO item;            // Reference to the ScriptableObject defining the item
        public GameObject equipPrefab; // Visual model used in the player's hand
    }

    // ------------------------------------------------------------
    //  DATABASE CONTENT
    // ------------------------------------------------------------
    [SerializeField] 
    private List<Entry> entries = new();

    private Dictionary<int, Entry> _networkLookup = new();
    private Dictionary<string, Entry> _gameplayLookup = new();

    // ------------------------------------------------------------
    //  INITIALIZATION
    // ------------------------------------------------------------
    public void Initialize()    // Called automatically when the database asset is loaded. Builds the lookup dictionary for extremely fast access.
    {
        Instance = this;

        _networkLookup.Clear();
        _gameplayLookup.Clear();

        foreach (var entry in entries)
        {
            // Network lookup
            if (!_networkLookup.ContainsKey(entry.networkId))
            {
                _networkLookup.Add(entry.networkId, entry);
            }

            // Gameplay lookup
            if (!_gameplayLookup.ContainsKey(entry.gameplayId))
            {
                _gameplayLookup.Add(entry.gameplayId, entry);
            }
        }

        Debug.Log($"[ItemDatabase] Initialized with {entries.Count} items.");
    }
    
    // IMPORTANT:
    // - Requires "ItemDatabase.asset" to be inside a Resources folder.
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadDatabase()      // Automatically loads ItemDatabase from Resources before any scene is loaded.
    {
        var db = Resources.Load<ItemDatabase>("ItemDatabase");
        
        if (db != null)
            db.Initialize();
        else
            Debug.LogWarning("[ItemDatabase] No ItemDatabase found in Resources!");
    }

    // =========================================================
    // NETWORK LOOKUP
    // =========================================================
    
    public ItemSO GetItemByNetworkId(int networkId)   // Returns the ItemSO associated with the given ID. Returns null if the ID does not exist.
    {
        return _networkLookup.TryGetValue(networkId, out var entry) 
            ? entry.item 
            : null;
    }

    public GameObject GetEquipPrefabByNetworkId(int networkId)    // Returns the prefab used for equipping this item (hand visual).
    {
        return _networkLookup.TryGetValue(networkId, out var entry) 
            ? entry.equipPrefab 
            : null;
    }
    
    public string GetGameplayId(int networkId)
    {
        return _networkLookup.TryGetValue(networkId, out var entry)
            ? entry.gameplayId
            : string.Empty;
    }
    
    // =========================================================
    // GAMEPLAY LOOKUP
    // =========================================================

    public ItemSO GetItemByGameplayId(string gameplayId)
    {
        return _gameplayLookup.TryGetValue(gameplayId, out var entry)
            ? entry.item
            : null;
    }

    public int GetNetworkId(string gameplayId)
    {
        return _gameplayLookup.TryGetValue(gameplayId, out var entry)
            ? entry.networkId
            : -1;
    }
}