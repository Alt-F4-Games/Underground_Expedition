using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spawn System/Spawn Database", fileName = "SpawnDatabase")]
public class SpawnDatabase : ScriptableObject
{
    public List<SpawnableObject> spawnables = new List<SpawnableObject>();

    public SpawnableObject GetSpawnableByID(string id)
    {
        return spawnables.Find(s => s.objectID == id);
    }
}