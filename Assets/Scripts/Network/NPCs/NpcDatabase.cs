using System.Collections.Generic;
using UnityEngine;

namespace Network.NPCs
{
    [CreateAssetMenu(menuName = "NPCs/NPC Database")]
    public class NpcDatabase : ScriptableObject
    {
        public static NpcDatabase Instance { get; private set; }

        [SerializeField]
        private List<NpcSO> npcs = new();

        private Dictionary<string, NpcSO> _lookup = new();

        public void Initialize()
        {
            Instance = this;

            _lookup.Clear();

            foreach (var npc in npcs)
            {
                if (npc == null)
                    continue;

                if (string.IsNullOrWhiteSpace(npc.npcId))
                    continue;

                if (_lookup.ContainsKey(npc.npcId))
                    continue;

                _lookup.Add(npc.npcId, npc);
            }

            Debug.Log(
                $"[NpcDatabase] Initialized with {_lookup.Count} NPCs.");
        }

        public NpcSO GetNpcById(string npcId)
        {
            return _lookup.GetValueOrDefault(npcId);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadDatabase()
        {
            var db =
                Resources.Load<NpcDatabase>("NpcDatabase");

            if (db != null)
            {
                db.Initialize();
            }
        }
    }
}