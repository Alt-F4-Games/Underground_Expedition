using UnityEngine;

public class RespawnActivator : MonoBehaviour, IInteractable
{
    [Header("Visual Feedback")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Settings")]
    [SerializeField] private bool useOnlyOnce = false;

    [Tooltip("ID of the RespawnPoint to activate")]
    [SerializeField] private string targetSpawnID;

    private bool alreadyUsed = false;

    private void Awake()
    {
        if (targetRenderer != null)
            targetRenderer.material.color = inactiveColor;
    }

    public void Interact(Player.PlayerInteraction player)
    {
        var point = GameManager.Instance.FindSpawnPointByID(targetSpawnID);

        if (point == null)
        {
            Debug.LogWarning("RespawnActivator: Could not find RespawnPoint with ID: " + targetSpawnID);
            return;
        }

        if (useOnlyOnce && alreadyUsed)
            return;

        Debug.Log("Checkpoint activated: " + point.respawnID);

        RespawnSystem.Instance.SetRespawnPoint(point.respawnID);

        if (targetRenderer != null)
            targetRenderer.material.color = activeColor;

        if (useOnlyOnce)
            alreadyUsed = true;
    }

    public void Release() { }
}