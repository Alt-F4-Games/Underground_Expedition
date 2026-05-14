using System.Collections.Generic;
using UnityEngine;

namespace Network.Inventory
{
    [CreateAssetMenu(menuName = "Inventory/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public static ItemDatabase Instance { get; private set; }

        [System.Serializable]
        public struct Entry
        {
            [Header("Network")]
            public int networkId;

            [Header("Gameplay")]
            public string gameplayId;

            [Header("Data")]
            public ItemSO item;

            public GameObject equipPrefab;
        }

        [SerializeField]
        private List<Entry> entries = new();

        private Dictionary<int, Entry> _networkLookup = new();
        private Dictionary<string, Entry> _gameplayLookup = new();

        public void Initialize()
        {
            Instance = this;

            _networkLookup.Clear();
            _gameplayLookup.Clear();

            foreach (var entry in entries)
            {
                if (!_networkLookup.ContainsKey(entry.networkId))
                {
                    _networkLookup.Add(entry.networkId, entry);
                }

                if (!string.IsNullOrWhiteSpace(entry.gameplayId))
                {
                    if (!_gameplayLookup.ContainsKey(entry.gameplayId))
                    {
                        _gameplayLookup.Add(entry.gameplayId, entry);
                    }
                }
            }

            Debug.Log($"[ItemDatabase] Initialized with {entries.Count} items.");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadDatabase()
        {
            var db = Resources.Load<ItemDatabase>("ItemDatabase");

            if (db != null)
                db.Initialize();
        }

        // =====================================================
        // NETWORK LOOKUP
        // =====================================================

        public ItemSO GetItemByNetworkId(int networkId)
        {
            return _networkLookup.TryGetValue(networkId, out var entry)
                ? entry.item
                : null;
        }

        public GameObject GetEquipPrefabByNetworkId(int networkId)
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

        // =====================================================
        // GAMEPLAY LOOKUP
        // =====================================================

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

        public GameObject GetEquipPrefabByGameplayId(string gameplayId)
        {
            return _gameplayLookup.TryGetValue(gameplayId, out var entry)
                ? entry.equipPrefab
                : null;
        }
    }
}