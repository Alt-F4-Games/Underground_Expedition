using Fusion;
using UnityEngine;

namespace Network.Enemies
{
    /// <summary>
    /// Base controller for swarm-type enemies. 
    /// Features a "snitch" mechanic to alert nearby allies and a dynamic aggro range (leash) system.
    /// </summary>
    public class NetworkSwarmController : NetworkEnemyController
    {
        [Header("Swarm Snitch & Chase Settings")]
        public float SnitchRange = 10f; // Distance to alert other swarm members
        public float SnitchListenRange = 25f; // Max aggro range when idle/searching
        public float ChaseRange = 15f; // Min aggro range during active chase
        
        [Header("Aggro Dynamics")]
        public float AggroShrinkSpeed = 3f; // Speed at which the detection radius shrinks during chase
        public float AggroGrowSpeed = 5f; // Speed at which the detection radius expands when losing target
        
        [Tooltip("Layer used to identify other allies for snitching")]
        public LayerMask SwarmLayer; 

        protected float _currentAggroRange;

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
                    SnitchToOtherSwarmMembers(closestPlayerInVision);
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

        // Broadcaster: Informs nearby swarm members about the detected target
        private void SnitchToOtherSwarmMembers(NetworkObject newTarget)
        {
            Collider[] allies = Physics.OverlapSphere(transform.position, SnitchRange, SwarmLayer);
            
            foreach (var hit in allies)
            {
                var friendly = hit.GetComponentInParent<NetworkSwarmController>();
                
                if (friendly != null && friendly != this)
                {
                    friendly.ReceiveSnitch(newTarget);
                }
            }
        }
        
        // Listener: Called when another swarm member provides a target
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
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
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