using Fusion;
using UnityEngine;

namespace Network.Enemies.Variants
{
    public class NetworkRatController : NetworkEnemyController
    {
        [Header("Rat Snitch Settings")]
        public float SnitchRange = 10f;
        public LayerMask RatLayer; // Layer to detect other rats

        [Header("Rat Jump Settings")]
        public float JumpChargeTime = 1.2f;
        public float JumpSpeed = 8f;
        public float JumpExtraDistance = 2f;

        // Overrides the base detection to add the Snitch mechanic
        protected override void FindTargetPlayer()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, VisionRange, PlayerLayer);
            float closestDistance = float.MaxValue;
            NetworkObject closestPlayer = null;

            // Find the closest player in vision
            foreach (var hit in hits)
            {
                var netObj = hit.GetComponentInParent<NetworkObject>();
                if (netObj != null)
                {
                    float distance = Vector3.Distance(transform.position, netObj.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = netObj;
                    }
                }
            }

            // Evaluate if we should change target and snitch
            if (closestPlayer != null)
            {
                float oldDistance = TargetPlayer != null ? Vector3.Distance(transform.position, TargetPlayer.transform.position) : float.MaxValue;

                // If the new player is closer than our current target (or we had no target)
                if (closestDistance < oldDistance)
                {
                    TargetPlayer = closestPlayer;
                    SnitchToOtherRats(closestPlayer);
                }
            }
            else
            {
                // If NO players are in vision, we lose the target (allows returning to patrol)
                TargetPlayer = null;
            }
        }

        // Alerts other rats in the SnitchRange
        private void SnitchToOtherRats(NetworkObject newTarget)
        {
            Collider[] rats = Physics.OverlapSphere(transform.position, SnitchRange, RatLayer);
            
            foreach (var hit in rats)
            {
                var friendlyRat = hit.GetComponentInParent<NetworkRatController>();
                
                // If we found a rat, and it's not ourselves
                if (friendlyRat != null && friendlyRat != this)
                {
                    friendlyRat.ReceiveSnitch(newTarget);
                }
            }
        }

        // Called by another rat when they spot a player
        public void ReceiveSnitch(NetworkObject snitchedTarget)
        {
            if (snitchedTarget == null) return;

            float newDistance = Vector3.Distance(transform.position, snitchedTarget.transform.position);
            float currentDistance = TargetPlayer != null ? Vector3.Distance(transform.position, TargetPlayer.transform.position) : float.MaxValue;

            // Only accept the snitch if the new target is closer than our current one
            if (newDistance < currentDistance)
            {
                TargetPlayer = snitchedTarget;
                
                // Force the FSM to start chasing immediately if we were just idling/patrolling
                if (CurrentState == NetworkEnemyState.Patrolling || CurrentState == NetworkEnemyState.Idle)
                {
                    StateMachine.ChangeState(new NetworkChaseState());
                }
            }
        }

        // Visual debug for the Snitch Range
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // Snitch range
            Gizmos.DrawWireSphere(transform.position, SnitchRange);
        }
    }
}