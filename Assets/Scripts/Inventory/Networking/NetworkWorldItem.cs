using Fusion;
using UnityEngine;
using System.Collections;

public class NetworkWorldItem : NetworkBehaviour
{
    [Networked] public int ItemId { get; set; }
    [Networked] public int Quantity { get; set; }

    [SerializeField] private Transform visualContainer;

    private int _currentVisualId = -1;
    private bool _isPickupRequested = false; 
    private Coroutine _pickupTimeoutCoroutine;

    public void Init(int id, int qty)
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

    private void UpdateVisuals()
    {
        if (ItemId == _currentVisualId) return;

        foreach(Transform child in visualContainer) Destroy(child.gameObject);

        var itemSO = ItemDatabase.Instance.GetItemById(ItemId);
        if (itemSO != null) 
        {
            GameObject prefab = ItemDatabase.Instance.GetEquipPrefab(ItemId);
            if(prefab != null) Instantiate(prefab, visualContainer);
        }
        _currentVisualId = ItemId;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isPickupRequested) return;
        var playerManager = other.GetComponent<NetworkInventoryManager>();
        
        if (playerManager != null && playerManager.HasInputAuthority)
        {
            _isPickupRequested = true;
            playerManager.RequestPickupItem(this);
            if (_pickupTimeoutCoroutine != null) StopCoroutine(_pickupTimeoutCoroutine);
            _pickupTimeoutCoroutine = StartCoroutine(PickupTimeoutReset(2.0f));
        }
    }
    
    public void ResetPickupRequest() {
        _isPickupRequested = false;
        if (_pickupTimeoutCoroutine != null) { StopCoroutine(_pickupTimeoutCoroutine); _pickupTimeoutCoroutine = null; }
    }
    
    private IEnumerator PickupTimeoutReset(float seconds) {
        yield return new WaitForSeconds(seconds);
        _isPickupRequested = false;
        _pickupTimeoutCoroutine = null;
    }
}