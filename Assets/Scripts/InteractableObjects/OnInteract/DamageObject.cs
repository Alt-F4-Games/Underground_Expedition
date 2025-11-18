using Player;
using UnityEngine;

public class DamageObject : MonoBehaviour, IInteractable
{
    [SerializeField] private int damage = 20;

    public void Interact(PlayerInteraction player)
    {
        Debug.Log("Haz recibido daño");
        
        var health = player.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }

    public void Release()
    {
        
    }
}