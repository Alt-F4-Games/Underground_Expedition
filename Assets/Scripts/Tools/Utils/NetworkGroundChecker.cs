using System;
using Fusion;
using UnityEngine;

namespace Tools.Utils
{
    public class NetworkGroundChecker : NetworkBehaviour
    {
        [Header("Settings")]
        [Tooltip("Detection distance or radius.")]
        [SerializeField] private float checkDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;
        
        // Events
        public Action<bool> OnGrounded;
        public Action<bool> OnLeaveGround;
        
        // Variables
        private bool _isGrounded;
        
        [Networked, OnChangedRender(nameof(OnGroundedChanged))]
        public NetworkBool IsGrounded { get; set; }

        private void FixedUpdate()
        {
            if (Object.HasStateAuthority)
            {
                _isGrounded = Physics.CheckSphere(transform.position, checkDistance, groundMask);
                IsGrounded = _isGrounded;
            }
        }
        
        private void OnGroundedChanged()
        {
            if (IsGrounded) OnGrounded?.Invoke(IsGrounded);
            else OnLeaveGround?.Invoke(IsGrounded);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, checkDistance);
        }
    }
}
