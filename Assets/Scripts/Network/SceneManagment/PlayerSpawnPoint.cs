using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform _point;

    public Vector3 GetPosition()
    {
        return _point != null ? _point.position : transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(GetPosition(), 0.3f);
    }
}