using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string keyName;
        public ItemSO item;
        public GameObject equipPrefab;
    }

    [SerializeField] private List<Entry> entries = new();

    private Dictionary<int, Entry> idToEntry = new();
    private Dictionary<string, int> nameToId = new();

    private void OnEnable()
    {
        Rebuild();
    }

    [ContextMenu("Rebuild Database")]
    public void Rebuild()
    {
        idToEntry.Clear();
        nameToId.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            idToEntry[i] = e;

            if (e.item == null)
                continue;

            string key = string.IsNullOrEmpty(e.keyName)
                ? e.item.itemName
                : e.keyName;

            nameToId[key] = i;
        }
    }

    public int GetId(ItemSO item)
    {
        if (item == null)
            return -1;

        string key = item.itemName;

        if (nameToId.TryGetValue(key, out int id))
            return id;

        foreach (var kv in idToEntry)
        {
            if (kv.Value.item == item)
                return kv.Key;
        }

        return -1;
    }

    public ItemSO GetItemById(int id)
    {
        if (idToEntry.TryGetValue(id, out var e))
            return e.item;

        return null;
    }

    public GameObject GetEquipPrefabById(int id)
    {
        if (idToEntry.TryGetValue(id, out var e))
            return e.equipPrefab;

        return null;
    }
}