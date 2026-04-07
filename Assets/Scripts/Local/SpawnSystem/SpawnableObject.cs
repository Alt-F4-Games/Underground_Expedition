using UnityEngine;

[CreateAssetMenu(menuName = "Spawn System/Spawnable Object", fileName = "NewSpawnableObject")]
public class SpawnableObject : ScriptableObject
{
    [Header("ID")]
    public string objectID;

    [Header("Prefab")]
    public GameObject prefab;

    [Tooltip("Life time (0 = infinite)")]
    public float lifeTime = 0f;

    [Tooltip("Maximum simultaneous quantity (0 = no limit)")]
    public int maxInstances = 0;
}