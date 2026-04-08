using UnityEngine;

namespace Network.Enemies.Variants
{
    // Inherits from NetworkPatrolPath to reuse node lists and tolerances
    public class AhPuchPath : NetworkPatrolPath
    {
        // Overriding to prevent mathematical looping (% Waypoints.Count)
        public override Transform GetWaypoint(int index)
        {
            if (Waypoints == null || index < 0 || index >= Waypoints.Count) return null;
            return Waypoints[index]; 
        }

        // Overriding Gizmos so the line doesn't connect the end back to the start
        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i] == null) continue;
                
                Gizmos.DrawSphere(Waypoints[i].position, 0.4f);
                
                // Only draw line to the next waypoint if it's not the last one
                if (i < Waypoints.Count - 1 && Waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(Waypoints[i].position, Waypoints[i + 1].position);
                }
            }
        }
    }
}