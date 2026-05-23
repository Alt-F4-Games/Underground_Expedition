using System;

namespace Network.Items
{
    [Serializable]
    public abstract class ItemEffect
    {
        /// <summary>
        /// Executes exclusively on the Server (State Authority).
        /// Returns TRUE if the effect was successfully applied to the player.
        /// </summary>
        public abstract bool Apply(NetworkPlayerController player);
    }
}