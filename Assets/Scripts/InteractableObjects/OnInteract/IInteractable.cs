using Player;
using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerInteraction interactor);
    void Release();
}