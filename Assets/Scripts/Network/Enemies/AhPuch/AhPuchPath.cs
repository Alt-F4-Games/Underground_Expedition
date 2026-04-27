using UnityEngine;
using Network.Enemies;

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
            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i] == null) continue;
                
                var evalNode = Waypoints[i].GetComponent<AhPuchEvalNode>();
                var statNode = Waypoints[i].GetComponent<AhPuchStatNode>();

                if (evalNode != null)
                {
                    // Eval nodes are Cyan spheres
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(Waypoints[i].position, 0.6f);
                    // Draw the specific evaluation radius
                    Gizmos.DrawWireSphere(Waypoints[i].position, evalNode.EvaluationRadius);
                }
                else if (statNode != null)
                {
                    // Stat nodes are Yellow cubes
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(Waypoints[i].position, Vector3.one * 0.6f);
                    // Draw the specific detection radius
                    Gizmos.DrawWireSphere(Waypoints[i].position, statNode.DetectionRadius);
                }
                else
                {
                    // Normal nodes are Magenta spheres
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(Waypoints[i].position, 0.4f);
                }
                
                // Only draw line to the next waypoint if it's not the last one
                Gizmos.color = Color.magenta;
                if (i < Waypoints.Count - 1 && Waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(Waypoints[i].position, Waypoints[i + 1].position);
                }
            }
        }
    }
}