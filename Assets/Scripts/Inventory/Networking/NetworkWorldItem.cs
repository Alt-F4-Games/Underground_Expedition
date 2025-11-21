using Fusion;
using UnityEngine;
using System.Collections;

/// <summary>
/// Representa un item físico en el mundo.
/// Se sincronizan ItemId y Quantity.
/// El objeto visual se instancia desde ItemDatabase.
/// Incluye protección contra múltiples pickups y timeout.
/// </summary>
public class NetworkWorldItem : NetworkBehaviour
{
    // -----------------------------
    //  NETWORKED PROPERTIES
    // -----------------------------
    [Networked] public int ItemId { get; set; }
    [Networked] public int Quantity { get; set; }

    // -----------------------------
    //  REFERENCES
    // -----------------------------
    [SerializeField] private Transform visualContainer;

    // -----------------------------
    //  LOCAL STATE
    // -----------------------------
    private int _currentVisualId = -1;
    private bool _isPickupRequested = false;
    private Coroutine _pickupTimeoutCoroutine;

    // ============================================================
    //  INITIALIZATION
    // ============================================================
    
    public void Init(int id, int qty)   // Initializes the world item when spawned by the server.
    {
        ItemId = id;
        Quantity = qty;
        _isPickupRequested = false;
    }

    public override void Spawned()
    {
        _isPickupRequested = false;
        UpdateVisuals();
    }

    public override void Render()
    {
        UpdateVisuals();
    }

    // ============================================================
    //  VISUALS
    // ============================================================
    
    private void UpdateVisuals()    //Updates the visual model based on ItemId. Only rebuilds visuals if the item changed.
    {
        //No need to rebuild if the item is the same as before.
        if (ItemId == _currentVisualId)
            return;

        ClearVisualContainer();

        // If ItemId is 0 or invalid, remove visuals.
        if (ItemId <= 0)
        {
            _currentVisualId = -1;
            return;
        }

        var prefab = ItemDatabase.Instance.GetEquipPrefab(ItemId);

        if (prefab != null)
            Instantiate(prefab, visualContainer);

        _currentVisualId = ItemId;
    }
    
    private void ClearVisualContainer()   //Destroys all children inside visualContainer.
    {
        for (int i = visualContainer.childCount - 1; i >= 0; i--)
            Destroy(visualContainer.GetChild(i).gameObject);
    }

    // ============================================================
    //  PICKUP LOGIC
    // ============================================================

    private void OnTriggerEnter(Collider other)
    {
        if (_isPickupRequested)         // Prevent repeated pickup requests.
            return;

        var playerManager = other.GetComponent<NetworkInventoryManager>();
        
        if (playerManager != null && playerManager.HasInputAuthority)       // Only the local player should request the pickup.
        {
            RequestPickup(playerManager);
        }
    }
    
    private void RequestPickup(NetworkInventoryManager playerManager)   // Called when a player enters the trigger and requests to pick up the item.
    {
        _isPickupRequested = true;
        playerManager.RequestPickupItem(this);

        RestartPickupTimeout(2.0f);
    }   
    
    private void RestartPickupTimeout(float seconds)    // Restarts the pickup timeout coroutine, ensuring only one is running.
    {
        if (_pickupTimeoutCoroutine != null)
            StopCoroutine(_pickupTimeoutCoroutine);

        _pickupTimeoutCoroutine = StartCoroutine(PickupTimeoutReset(seconds));
    }
    
    public void ResetPickupRequest()    //Called by the server (via RPC) when the pickup fails. Allows the player to try picking up the item again.
    {
        _isPickupRequested = false;

        if (_pickupTimeoutCoroutine != null)
        {
            StopCoroutine(_pickupTimeoutCoroutine);
            _pickupTimeoutCoroutine = null;
        }
    }

    
    private IEnumerator PickupTimeoutReset(float seconds)   //After a small delay, the item becomes pickable again. Prevents spam/accidental multiple pickup attempts.
    {
        yield return new WaitForSeconds(seconds);
        _isPickupRequested = false;
        _pickupTimeoutCoroutine = null;
    }
}