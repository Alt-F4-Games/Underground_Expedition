using UnityEngine;
using Enemy.States.Skull;
using Enemy.States.Base;

namespace Enemy
{
    public class SkullIA : EnemyAI
    {
        [Header("Floating Settings")]
        public float floatHeight = 2.5f;
        public float floatAmplitude = 0.25f;
        public float floatSpeed = 2f;

        private float baseOffset;

        protected override void Start()
        {
            base.Start();

            agent.updateUpAxis = false; 
            agent.updateRotation = true;

            baseOffset = floatHeight;
            agent.baseOffset = floatHeight;

            stateMachine.Initialize(new SkullPatrolState(this));
        }

        private void LateUpdate()
        {
            agent.baseOffset = baseOffset + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        }
    }
}