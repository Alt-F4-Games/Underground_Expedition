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
}