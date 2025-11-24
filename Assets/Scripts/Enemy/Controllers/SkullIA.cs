/*
    SkullIA.cs
    Description:
    Specialized AI for the Skull enemy, extending the base EnemyAI behavior.
    The Skull floats up and down using a sinusoidal motion, and uses a custom
    NavMeshAgent configuration suited for flying-type navigation.

    Behavior:
    - Disables vertical axis updates on the NavMeshAgent (since it floats).
    - Applies a constant floating offset and animates it in LateUpdate.
    - Initializes its own state machine with skull-specific patrol state.

    Dependencies:
    - EnemyAI (base class providing core navigation, detection, attack logic)
    - EnemyStateMachine and SkullPatrolState
    - UnityEngine for transforms, Mathf, etc.
*/

using UnityEngine;
using Enemy.States.Skull;
using Enemy.States.Base;

namespace Enemy
{
    public class SkullIA : EnemyAI
    {
        [Header("Floating Settings")]
        public float floatHeight = 2.5f;        // Base height at which the skull hovers
        public float floatAmplitude = 0.25f;    // Vertical oscillation amplitude
        public float floatSpeed = 2f;           // Oscillation speed multiplier

        private float baseOffset;               // Internal offset used to track floating motion

        protected override void Start()
        {
            base.Start(); // Base class initialization (AI references, player detection, etc.)

            // Skulls hover, so NavMesh vertical axis must remain untouched:
            agent.updateUpAxis = false;   // Prevents NavMeshAgent from affecting Y-axis
            agent.updateRotation = true;  // Skull still rotates according to movement direction

            // Set initial floating height
            baseOffset = floatHeight;
            agent.baseOffset = floatHeight;

            // Start in the skull patrol behavior
            stateMachine.Initialize(new SkullPatrolState(this));
        }

        // Applies floating motion AFTER NavMeshAgent movement is resolved
        private void LateUpdate()
        {
            // Sinusoidal movement applied to the vertical offset:
            agent.baseOffset = baseOffset + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        }
    }
}
