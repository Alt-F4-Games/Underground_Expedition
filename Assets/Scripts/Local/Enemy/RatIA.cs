using UnityEngine;
using Enemy.States.Rat;
using Enemy.States.Base;

namespace Enemy
{
    public class RatIA : EnemyAI
    {
        [Header("Rat Jump Settings")]
        public float jumpExtraDistance = 2f;
        public float jumpHeight = 3f;
        public float jumpSpeed = 8f;
        public float jumpChargeTime = 1f;

        [Header("Rat Jump Range")]
        public float minJumpDistance = 3f;
        public float maxJumpDistance = 8f;

        protected override void Start()
        {
            base.Start();
            stateMachine.Initialize(new RatPatrolState(this));
        }

        public bool CanJumpAttack()
        {
            if (player == null)
                return false;

            float dist = DistanceToPlayer();
            return dist >= minJumpDistance && dist <= maxJumpDistance && CanSeePlayer();
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, minJumpDistance);
            Gizmos.color = new Color(1f, 0.6f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, maxJumpDistance);
        }
    }
}