/*
 * IInteractable
 * -------------
 * Base interface for all objects the player can interact with.
 * Provides:
 *  - Interact(PlayerInteraction interactor): called when the player interacts
 *  - Release(): optional method for releasing/picking up logic
 */

using Player;
using UnityEngine;

public interface IInteractable
{
    // Called when the player initiates interaction with the object
    void Interact(PlayerInteraction interactor);

    // Called when the interaction ends or the object is released
    void Release();
}