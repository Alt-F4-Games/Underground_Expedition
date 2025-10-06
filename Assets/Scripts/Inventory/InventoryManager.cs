using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InventorySystem))]
public class InventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventorySystem inventorySystem;
    
}