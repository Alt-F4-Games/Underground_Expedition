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
}