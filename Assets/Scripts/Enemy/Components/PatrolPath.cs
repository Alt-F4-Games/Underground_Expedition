/*
    PatrolPath.cs
    Description:
    Defines a patrol route for an enemy using a list of waypoints.
    Provides access to individual waypoints and visualizes the patrol path in the Unity editor.

    Dependencies:
    - UnityEngine for Transform, Gizmos, MonoBehaviour.
    - Used by enemy AI systems that require a patrol movement pattern.
*/

using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class PatrolPath : MonoBehaviour
    {
        // List of waypoints that define the patrol route
        public List<Transform> Waypoints = new List<Transform>();

        // Returns the total number of waypoints
        public int Count => Waypoints.Count;

        // Returns the waypoint at the requested index, looping around if index is out of range
        public Transform GetWaypoint(int index)
        {
            if (Waypoints == null || Waypoints.Count == 0) return null;
            return Waypoints[index % Waypoints.Count];
        }

        // Draws gizmos in the editor to visualize the patrol route when this object is selected
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i] == null) continue;

                // Draws a small marker to show each waypoint position
                Gizmos.DrawSphere(Waypoints[i].position, 0.2f);

                // Draws a line connecting this waypoint to the next to form the path
                var next = Waypoints[(i + 1) % Waypoints.Count];
                if (next != null) Gizmos.DrawLine(Waypoints[i].position, next.position);
            }
        }
    }
}