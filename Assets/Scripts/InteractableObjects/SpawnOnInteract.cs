using UnityEngine;

public class SpawnOnInteract : MonoBehaviour, IInteractable
{
    [Header("Spawn Configuration")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private string objectID;
    [SerializeField] private string spawnID;

    [Header("Options")]
    [Tooltip("If enabled, can only be used once.")]
    [SerializeField] private bool singleUse = false;

    private bool hasBeenUsed = false;

    public void Interact(PlayerInteraction interactor)
    {
        if (singleUse && hasBeenUsed)
            return;

        if (spawner == null)
        {
            Debug.LogWarning($"[SpawnOnInteract] No Spawner assigned in {gameObject.name}");
            return;
        }

        spawner.Spawn(objectID, spawnID);

        Debug.Log($"[SpawnOnInteract] Spawned '{objectID}' at '{spawnID}'");

        hasBeenUsed = true;
    }

    public void Release()
    {
        // No action required when releasing interaction
    }
}