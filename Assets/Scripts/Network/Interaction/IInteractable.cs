using Fusion;

namespace Network.Interaction
{
    /// <summary>
    /// Base contract for all interactive objects in the game
    /// </summary>
    public interface IInteractable
    {
        // What text to show when looking at the object
        string GetInteractPrompt();
        
        // How long the player must hold the interact button
        float GetInteractionDuration();
        
        // Can this specific player interact right now
        bool CanInteract(PlayerRef player);
        
        // The actual logic that happens when the interaction completes
        // only be executed on the Server
        void OnInteract(NetworkPlayerController player);
    }
}