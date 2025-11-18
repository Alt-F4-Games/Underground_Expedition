using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private GameObject slotPrefab;

    private List<InventorySlotUI> _slots = new();
    private NetworkInventoryManager _currentManager;

    private void Start()
    {
        if (NetworkInventoryManager.Local != null) ConnectToLocalPlayer();
        NetworkInventoryManager.OnLocalPlayerSpawned += ConnectToLocalPlayer;
    }

    private void OnDestroy()
    {
        NetworkInventoryManager.OnLocalPlayerSpawned -= ConnectToLocalPlayer;
        if (_currentManager != null)
        {
            var sys = _currentManager.GetComponent<NetworkInventorySystem>();
            if (sys != null) sys.OnInventoryChanged -= Refresh;
        }
    }

    private void ConnectToLocalPlayer()
    {
        _currentManager = NetworkInventoryManager.Local;
        var sys = _currentManager.GetComponent<NetworkInventorySystem>();
        
        sys.OnInventoryChanged += Refresh;
        
        CreateSlots(sys.GetCapacity(SlotType.Hotbar));
        Refresh();
    }

    private void CreateSlots(int count)
    {
        foreach (Transform child in hotbarContainer) Destroy(child.gameObject);
        _slots.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, hotbarContainer);
            var ui = go.GetComponent<InventorySlotUI>();
            
            ui.SlotIndex = i;
            ui.SlotType = SlotType.Hotbar;
            ui.Manager = _currentManager;
            
            _slots.Add(ui);
        }
    }

    public void Refresh()
    {
        if (_currentManager == null) return;
        var sys = _currentManager.GetComponent<NetworkInventorySystem>();
        
        for(int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Refresh(sys.HotbarSlots[i]);
            
            // Opcional: Highlight si es el seleccionado
            bool isSelected = (_currentManager.SelectedHotbarIndex == i);
            _slots[i].SetHighlight(isSelected);
        }
    }
    
    // Update para input de selección de hotbar (Mouse scroll)
    private void Update()
    {
        if (_currentManager == null || !Application.isFocused) return;
        
        // Evitar scroll si el inventario está abierto
        // Aquí podrías chequear InventoryUI.IsVisible() si tienes referencia

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            int current = _currentManager.SelectedHotbarIndex;
            int max = _slots.Count;
            
            // Lógica circular
            if (scroll > 0) current--;
            else current++;

            if (current < 0) current = max - 1;
            if (current >= max) current = 0;

            // Enviar RPC para cambiar selección
            _currentManager.Input_SetSelectedHotbar(current);
        }
    }
}