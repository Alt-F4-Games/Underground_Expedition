/*
    SpawnDatabase.cs
    A searchable database of spawnable ScriptableObjects.

    Behavior:
    - Stores all spawnable object definitions.
    - Allows lookup by ID, used by the Spawner.

    Intended for:
    - Designers to configure all spawnable objects in one place.
*/

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spawn System/Spawn Database", fileName = "SpawnDatabase")]
public class SpawnDatabase : ScriptableObject
{
    // List of all registered spawnable objects (prefabs and enemies)
    public List<SpawnableObject> spawnables = new List<SpawnableObject>();

    // Retrieve a spawnable configuration by its string ID
    public SpawnableObject GetSpawnableByID(string id)
    {
        return spawnables.Find(s => s.objectID == id);
    }
}