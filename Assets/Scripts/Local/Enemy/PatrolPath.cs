using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class PatrolPath : MonoBehaviour
    {
        public List<Transform> Waypoints = new List<Transform>();
        public int Count => Waypoints.Count;

        public Transform GetWaypoint(int index)
        {
            if (Waypoints == null || Waypoints.Count == 0) return null;
            return Waypoints[index % Waypoints.Count];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i] == null) continue;
                Gizmos.DrawSphere(Waypoints[i].position, 0.2f);
                var next = Waypoints[(i + 1) % Waypoints.Count];
                if (next != null) Gizmos.DrawLine(Waypoints[i].position, next.position);
            }
        }
    }
}