using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public static ItemDatabase Instance { get; private set; }

    [System.Serializable]
    public struct Entry
    {
        public int id; // ID manual para asegurar consistencia entre builds
        public ItemSO item;
        public GameObject equipPrefab; // El modelo 3D para la mano
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<int, Entry> _lookup = new();

    // Se ejecuta al cargar el juego o al entrar en Play Mode
    public void Initialize()
    {
        Instance = this;
        _lookup.Clear();
        foreach (var entry in entries)
        {
            if (!_lookup.ContainsKey(entry.id))
            {
                _lookup.Add(entry.id, entry);
            }
        }
        Debug.Log($"[ItemDatabase] Initialized with {entries.Count} items.");
    }

    // Llamar a esto en un script de inicio general si es necesario, 
    // o usar RuntimeInitializeOnLoadMethod
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadDatabase()
    {
        var db = Resources.Load<ItemDatabase>("ItemDatabase"); 
        if (db != null) db.Initialize();
    }

    public ItemSO GetItemById(int id)
    {
        if (_lookup.TryGetValue(id, out var entry)) return entry.item;
        return null;
    }

    public GameObject GetEquipPrefab(int id)
    {
        if (_lookup.TryGetValue(id, out var entry)) return entry.equipPrefab;
        return null;
    }

    public int GetIdByItem(ItemSO item)
    {
        foreach (var kvp in _lookup)
        {
            if (kvp.Value.item == item) return kvp.Key;
        }
        return 0; // 0 = Vacío/No encontrado
    }
}