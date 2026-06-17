using UnityEngine;

namespace Network.Items
{
    public abstract class ItemEffect : ScriptableObject
    {
        public abstract bool Apply(NetworkPlayerController player);
    }
}