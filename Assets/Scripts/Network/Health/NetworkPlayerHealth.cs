using Fusion;
using UnityEngine;

namespace Health
{
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkPlayerHealth : NetworkHealthSystem
    {
        protected override void Death()
        {
            base.Death();

            if (!HasStateAuthority) return;

            Debug.Log($"{gameObject.name} died");
            
        }
        
    }
}