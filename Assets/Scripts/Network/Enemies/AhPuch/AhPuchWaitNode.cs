using UnityEngine;

namespace Network.Enemies
{
    public class AhPuchWaitNode : MonoBehaviour
    {
        [Tooltip("Tiempo en segundos que el jefe permanecerá en estado Idle en este nodo.")]
        public float WaitTime = 3f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}