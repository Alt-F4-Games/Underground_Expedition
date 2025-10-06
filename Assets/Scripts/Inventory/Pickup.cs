using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemSO item;
    [SerializeField, Min(1)] private int quantity = 1;
    
}