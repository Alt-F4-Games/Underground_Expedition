using Fusion;
using UnityEngine;

public class NetworkWorldItem : NetworkBehaviour
{
    [Networked] public int ItemId { get; set; }
    [Networked] public int Quantity { get; set; }

    [SerializeField] private Transform visualContainer;

    private int _currentVisualId = -1;
    
    // --- NUEVA VARIABLE: Freno local ---
    private bool _isPickupRequested = false; 

    public void Init(int id, int qty)
    {
        ItemId = id;
        Quantity = qty;
        // Reseteamos el freno por si el objeto es reutilizado (Object Pooling)
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
        // 1. Si ya pedimos recogerlo, ignoramos colisiones nuevas
        if (_isPickupRequested) return;

        var playerManager = other.GetComponent<NetworkInventoryManager>();
        
        if (playerManager != null && playerManager.HasInputAuthority)
        {
            // 2. Activamos el freno INMEDIATAMENTE
            _isPickupRequested = true;

            // 3. Enviamos la solicitud
            playerManager.RequestPickupItem(this);
        }
    }
}