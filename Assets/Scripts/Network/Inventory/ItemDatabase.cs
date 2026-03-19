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
        public int id;                 // Stable ID used in networking and saves
        public ItemSO item;            // Reference to the ScriptableObject defining the item
        public GameObject equipPrefab; // Visual model used in the player's hand
    }

    // ------------------------------------------------------------
    //  DATABASE CONTENT
    // ------------------------------------------------------------
    [SerializeField] 
    private List<Entry> entries = new();

    private Dictionary<int, Entry> _lookup = new();

    // ------------------------------------------------------------
    //  INITIALIZATION
    // ------------------------------------------------------------
    public void Initialize()    // Called automatically when the database asset is loaded. Builds the lookup dictionary for extremely fast access.
    {
        Instance = this;

        _lookup.Clear();

        foreach (var entry in entries)
        {
            if (_lookup.ContainsKey(entry.id))
                continue;

            _lookup.Add(entry.id, entry);
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

    // ------------------------------------------------------------
    //  PUBLIC API — LOOKUP
    // ------------------------------------------------------------
    
    public ItemSO GetItemById(int id)   // Returns the ItemSO associated with the given ID. Returns null if the ID does not exist.
    {
        return _lookup.TryGetValue(id, out var entry) 
            ? entry.item 
            : null;
    }

    public GameObject GetEquipPrefab(int id)    // Returns the prefab used for equipping this item (hand visual).
    {
        return _lookup.TryGetValue(id, out var entry) 
            ? entry.equipPrefab 
            : null;
    }
}