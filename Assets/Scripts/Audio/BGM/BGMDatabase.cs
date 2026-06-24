using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio.BGM
{
    [CreateAssetMenu(fileName = "BGMDatabase", menuName = "Audio/BGM Database")]
    public class BGMDatabase : ScriptableObject
    {
        [Serializable]
        public class SceneMusicEntry
        {
            public string SceneName;
            public string AudioKey;
        }

        [SerializeField]
        private List<SceneMusicEntry> entries = new();

        public bool TryGetMusic(string sceneName, out string audioKey)
        {
            foreach (var entry in entries)
            {
                if (entry.SceneName == sceneName)
                {
                    audioKey = entry.AudioKey;
                    return true;
                }
            }

            audioKey = null;
            return false;
        }
    }
}