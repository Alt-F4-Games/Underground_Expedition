using UnityEngine;

namespace Network.Enemies
{
    public class AhPuchStatNode : MonoBehaviour
    {
        [Header("NavMeshAgent Overrides (0 = No change)")]
        public float NewSpeed = 0f;
        public float NewAngularSpeed = 0f;
        public float NewAcceleration = 0f;

        [Header("Detection Overrides (0 = No change)")]
        public float NewVisionRange = 0f;
        
        [Tooltip("Modifies the AttackRange and automatically scales the aura radius.")]
        public float NewAttackRange = 0f;

        [Header("Combat Overrides (0 = No change)")]
        public float NewAttackCooldown = 0f;

        [Header("Dash Overrides (0 = No change)")]
        public float NewDashSpeedBoost = 0f;
        public float NewDashDurationSuccess = 0f;
        public float NewDashDurationFail = 0f;
    }
}