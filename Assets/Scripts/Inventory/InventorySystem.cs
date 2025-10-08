using System.Collections.Generic;
using UnityEngine;

public enum SlotType { Base, Equip, Hotbar }

[System.Serializable]
public class InventorySlot
{
    public ItemSO item;
    public int quantity;
}

public class InventorySystem : MonoBehaviour
{
    [Header("Capacities")]
    [SerializeField] private int baseCapacity = 3;   
    [SerializeField] private int equipCapacity = 3;  
    [SerializeField] private int hotbarCapacity = 3; 
    [SerializeField] private int maxBaseCapacity = 9; 

    private List<InventorySlot> baseSlots = new();
    private List<InventorySlot> equipSlots = new();
    private List<InventorySlot> hotbarSlots = new();

    public System.Action OnInventoryChanged;

    private void Awake()
    {
        EnsureCapacity(baseSlots, baseCapacity);
        EnsureCapacity(equipSlots, equipCapacity);
        EnsureCapacity(hotbarSlots, hotbarCapacity);
    }

    private void EnsureCapacity(List<InventorySlot> list, int capacity)
    {
        while (list.Count < capacity)
            list.Add(new InventorySlot());
    }

   
}