using System.Collections.Generic;
using UnityEngine;

namespace Network.Enemies
{
    // Simple container for the patrol waypoints
    public class NetworkPatrolPath : MonoBehaviour
    {
        [Header("Waypoints")]
        public List<Transform> Waypoints = new List<Transform>();
        public float WaypointTolerance = 0.5f; // Distance to consider a waypoint reached

        // Returns the waypoint safely looping back to 0
        public virtual Transform GetWaypoint(int index)
        {
            if (Waypoints == null || Waypoints.Count == 0) return null;
            return Waypoints[index % Waypoints.Count];
        }

        // Visualizes the path in the Unity Editor
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i] == null) continue;
                Gizmos.DrawSphere(Waypoints[i].position, 0.3f);
                
                var next = Waypoints[(i + 1) % Waypoints.Count];
                if (next != null) Gizmos.DrawLine(Waypoints[i].position, next.position);
            }
        }
    }
}