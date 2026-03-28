using Fusion;
using UnityEngine;

namespace Network.Enemies
{
    /// <summary>
    /// Specialized controller for the Rat enemy. 
    /// Features a "snitch" mechanic to alert nearby rats and a dynamic aggro range (leash) system.
    /// </summary>
    public class NetworkRatController : NetworkEnemyController
    {
        [Header("Rat Snitch & Chase Settings")]
        public float SnitchRange = 10f; // Distance to alert other rats
        public float SnitchListenRange = 25f; // Max aggro range when idle/searching
        public float ChaseRange = 15f; // Min aggro range during active chase
        
        [Header("Aggro Dynamics")]
        public float AggroShrinkSpeed = 3f; // Speed at which the detection radius shrinks during chase
        public float AggroGrowSpeed = 5f; // Speed at which the detection radius expands when losing target
        
        public LayerMask RatLayer; // Layer used to identify other rats for snitching

        [Header("Rat Jump Settings")]
        public float JumpChargeTime = 1.2f;
        public float JumpSpeed = 8f;
        public float JumpExtraDistance = 2f;

        private float _currentAggroRange;

        // Scans for targets and manages the dynamic aggro leash
        protected override void FindTargetPlayer()
        {
            // Base detection logic inherited from NetworkEnemyController
            Collider[] hits = Physics.OverlapSphere(transform.position, VisionRange, PlayerLayer);
            float closestDistance = float.MaxValue;
            NetworkObject closestPlayerInVision = null;
            
            foreach (var hit in hits)
            {
                var netObj = hit.GetComponentInParent<NetworkObject>();
                if (netObj != null)
                {
                    float distance = Vector3.Distance(transform.position, netObj.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayerInVision = netObj;
                    }
                }
            }
            
            // VISUAL DETECTION: If seen directly, alert others and tighten the leash
            if (closestPlayerInVision != null)
            {
                if (TargetPlayer != closestPlayerInVision)
                {
                    TargetPlayer = closestPlayerInVision;
                    SnitchToOtherRats(closestPlayerInVision);
                }
                
                // Tighten the leash towards combat minimum (ChaseRange)
                _currentAggroRange = Mathf.MoveTowards(_currentAggroRange, ChaseRange, AggroShrinkSpeed * 2f * Runner.DeltaTime);
            }
            
            // LEASH DYNAMICS (Shrinking): Adjust range based on current target distance
            if (TargetPlayer != null)
            {
                float distToTarget = Vector3.Distance(transform.position, TargetPlayer.transform.position);
                float targetRange = Mathf.Max(distToTarget, ChaseRange);

                if (_currentAggroRange > targetRange)
                {
                    _currentAggroRange = Mathf.MoveTowards(_currentAggroRange, targetRange, AggroShrinkSpeed * Runner.DeltaTime);
                }

                // TRANSITION: If player moves outside the dynamic aggro circle -> lose target
                if (distToTarget > _currentAggroRange)
                {
                    TargetPlayer = null; 
                }
            }
            
            // LEASH DYNAMICS (Growing): Expand range back to max if no target is active
            if (TargetPlayer == null) 
            {
                if (_currentAggroRange < SnitchListenRange)
                {
                    _currentAggroRange = Mathf.MoveTowards(_currentAggroRange, SnitchListenRange, AggroGrowSpeed * Runner.DeltaTime);
                }
            }
        }

        // Broadcaster: Informs nearby rats about the detected target
        private void SnitchToOtherRats(NetworkObject newTarget)
        {
            Collider[] rats = Physics.OverlapSphere(transform.position, SnitchRange, RatLayer);
            
            foreach (var hit in rats)
            {
                var friendlyRat = hit.GetComponentInParent<NetworkRatController>();
                
                if (friendlyRat != null && friendlyRat != this)
                {
                    friendlyRat.ReceiveSnitch(newTarget);
                }
            }
        }
        
        // Listener: Called when another rat provides a target
        public void ReceiveSnitch(NetworkObject snitchedTarget)
        {
            if (snitchedTarget == null) return;

            float newDistance = Vector3.Distance(transform.position, snitchedTarget.transform.position);
            
            // Only accept the snitch if the target is within our current hearing range
            if (newDistance > _currentAggroRange) return;

            float currentDistance = TargetPlayer != null ? Vector3.Distance(transform.position, TargetPlayer.transform.position) : float.MaxValue;
            
            // Prioritize the closest player
            if (newDistance < currentDistance)
            {
                TargetPlayer = snitchedTarget;
            }
        }

        // Draws debug spheres in the Unity Editor for AI ranges
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, VisionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
            
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange: Snitch alert range
            Gizmos.DrawWireSphere(transform.position, SnitchRange);

            if (Application.isPlaying)
            {
                // Active dynamic aggro circle
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, _currentAggroRange);
            }
            else
            {
                Gizmos.color = Color.cyan; // Max listening range
                Gizmos.DrawWireSphere(transform.position, SnitchListenRange);
                Gizmos.color = Color.gray; // Min chase range
                Gizmos.DrawWireSphere(transform.position, ChaseRange);
            }
        }
    }
}