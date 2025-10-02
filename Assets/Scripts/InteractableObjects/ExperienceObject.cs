using UnityEngine;

public class ExperienceObject : MonoBehaviour, IInteractable
{
    [SerializeField] private int xpAmount = 20; 

    public void Interact(PlayerInteraction player)
    {
        Debug.Log($"Has recibido {xpAmount} de experiencia");
        
        if (ExperienceSystem.instance != null)
        {
            ExperienceSystem.instance.AddXP(xpAmount);
        }
        else
        {
            Debug.LogError("No se encontró un ExperienceSystem en la escena.");
        }
    }

    public void Release()
    {
        
    }
}