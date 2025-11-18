using Player;
using UnityEngine;

public class HealthObject : MonoBehaviour, IInteractable
{
    [SerializeField] private int healAmount = 20;

    public void Interact(PlayerInteraction player)
    {
        Debug.Log("Has recibido curación");
        
        var health = player.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.Heal(healAmount);
        }
    }

    public void Release()
    {
        
    }
}