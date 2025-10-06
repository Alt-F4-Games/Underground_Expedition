using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemSO item;
    [SerializeField, Min(1)] private int quantity = 1;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

}