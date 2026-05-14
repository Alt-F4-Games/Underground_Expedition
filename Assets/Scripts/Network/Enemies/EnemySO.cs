using UnityEngine;

namespace Network.Enemies
{
    [CreateAssetMenu(menuName = "Enemies/Enemy")]
    public class EnemySO : ScriptableObject
    {
        [Header("Visual")]
        public string displayName;
        
        [Header("Identity")]
        public string enemyId;

        [Header("Prefab")]
        public GameObject prefab;

        [Header("UI")]
        public Sprite icon;
        
        [TextArea]
        public string description;
    }
}