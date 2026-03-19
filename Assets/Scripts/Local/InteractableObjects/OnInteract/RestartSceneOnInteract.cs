using UnityEngine;
using Player;

public class RestartSceneOnInteract : MonoBehaviour, IInteractable
{
    [Header("Feedback")]
    [SerializeField] private string interactMessage = "Reiniciando escena...";

    public void Interact(PlayerInteraction player)
    {
        Debug.Log(interactMessage);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartLevel();
        }
        else
        {
            Debug.LogWarning("No se encontró GameManager en escena!");
        }
    }

    public void Release()
    {
    }
}