/*
 * IHoldable
 * ---------
 * Interface for objects that can be picked up and held by the player.
 * Extends IInteractable so holdable objects can also be interacted with normally.
 */

using UnityEngine;

public interface IHoldable : IInteractable
{
    // No extra methods → simply marks an IInteractable as holdable
}