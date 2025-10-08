using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WorldItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemSO item;
    [SerializeField, Min(1)] private int quantity = 1;

}