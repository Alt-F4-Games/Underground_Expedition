using UnityEngine;

public class ExperienceObject : MonoBehaviour, IInteractable
{
    [SerializeField] private int xpAmount = 50;  
    

    public void Interact(PlayerInteraction interactor)
    {
        ExperienceSystem.instance.AddXP(xpAmount);
        Debug.Log("get " + xpAmount + " XP.");
    }

    public void Release()
    {
    }
}