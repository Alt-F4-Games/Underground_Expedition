using UnityEngine;

public class RespawnActivator : MonoBehaviour, IInteractable
{
    private RespawnPoint point;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;

    [Header("Settings")]
    [SerializeField] private bool useOnlyOnce = false;

    private bool alreadyUsed = false;

    private void Awake()
    {
        point = GetComponent<RespawnPoint>();

        // Set initial color
        if (targetRenderer != null)
            targetRenderer.material.color = inactiveColor;
    }

    public void Interact(Player.PlayerInteraction player)
    {
        Debug.Log("PLAYER PRESSED INTERACT ON " + gameObject.name);

        if (point == null) return;

        // Prevent reactivation
        if (useOnlyOnce && alreadyUsed)
            return;

        Debug.Log("Checkpoint activated: " + point.respawnID);
        GameManager.Instance.SetRespawnPoint(point);

        // Set active color
        if (targetRenderer != null)
            targetRenderer.material.color = activeColor;

        // Mark as used if needed
        if (useOnlyOnce)
            alreadyUsed = true;
    }

    public void Release() {}
}