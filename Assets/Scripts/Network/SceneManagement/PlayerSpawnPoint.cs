using UnityEngine;
using Fusion;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform _point;
    [SerializeField] private float _radius = 2.5f;

    public Vector3 GetPosition(PlayerRef player)
    {
        Vector3 basePos = _point != null ? _point.position : transform.position;

        int index = player.RawEncoded; 

        float angle = index * 137.5f; 
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(rad),
            0,
            Mathf.Sin(rad)
        ) * _radius;

        return basePos + offset;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}