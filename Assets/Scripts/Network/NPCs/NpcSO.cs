using UnityEngine;

namespace Network.NPCs
{
    [CreateAssetMenu(menuName = "NPCs/NPC")]
    public class NpcSO : ScriptableObject
    { 
        [Header("Prefab")]
        public GameObject prefab;
        
        [Header("Identity")]
        public string npcId;

        public string displayName;

        [Header("Visual")]
        public Sprite portrait;

        [TextArea]
        public string description;
    }
}