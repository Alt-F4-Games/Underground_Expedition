using UnityEngine;

namespace Network.Enemies
{
    public class AhPuchEvalNode : MonoBehaviour
    {
        [Tooltip("Activation radius for the boss to detect this node during patrol or chase.")]
        public float EvaluationRadius = 2.5f;
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, EvaluationRadius);
            
            Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
            Gizmos.DrawSphere(transform.position, EvaluationRadius);
        }
    }
}