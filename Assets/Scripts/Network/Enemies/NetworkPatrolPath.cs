using System.Collections.Generic;
using UnityEngine;

namespace Network.Enemies
{
    public class NetworkPatrolPath : MonoBehaviour
    {
        [Header("Waypoints")]
        public List<Transform> Waypoints = new List<Transform>();
        public float WaypointTolerance = 0.5f;

        public Transform GetWaypoint(int index)
        {
            if (Waypoints == null || Waypoints.Count == 0) return null;
            return Waypoints[index % Waypoints.Count];
        }

        private void OnDrawGizmosSelected()
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