using System;
using UnityEngine;

namespace Events
{
    [CreateAssetMenu(menuName = "Events/Void Event Channel")]
    public class VoidEventChannel : ScriptableObject
    {
        public event Action OnEventRaised;

        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
        }
    }
}