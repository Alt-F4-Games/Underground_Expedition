using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI controller for the player's Hotbar.
/// Displays each hotbar slot and reacts to item changes,
/// selection changes, and mouse-scroll input.
/// </summary>

public class HotbarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Slot Sizes")]
    [SerializeField] private Vector2 normalSize = new Vector2(60, 60);
    [SerializeField] private Vector2 centerSize = new Vector2(90, 90);

    // Internal runtime data
    private List<InventorySlotUI> _slots = new();
    private NetworkInventoryManager _currentManager;

    // =====================================================================
    // Lifecycle
    // =====================================================================

    private void Start()
    {
        if (NetworkInventoryManager.Local != null)
            ConnectToLocalPlayer();

        NetworkInventoryManager.OnLocalPlayerSpawned += ConnectToLocalPlayer;
        NetworkInventoryManager.OnHotbarIndexChanged += Refresh;
    }

    private void OnDestroy()
    {
        NetworkInventoryManager.OnLocalPlayerSpawned -= ConnectToLocalPlayer;
        NetworkInventoryManager.OnHotbarIndexChanged -= Refresh;

        if (_currentManager != null)
        {
            var sys = _currentManager.GetComponent<NetworkInventorySystem>();
            if (sys != null)
                sys.OnInventoryChanged -= Refresh;
        }
    }

    // =====================================================================
    // Connection / Initialization
    // =====================================================================

    private void ConnectToLocalPlayer()
    {
        if (NetworkInventoryManager.Local == null)
            return;

        _currentManager = NetworkInventoryManager.Local;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();
        sys.OnInventoryChanged += Refresh;

        CreateSlots(sys.GetCapacity(SlotType.Hotbar));
        Refresh();
    }

    private void CreateSlots(int count)
    {
        foreach (Transform child in hotbarContainer)
            Destroy(child.gameObject);

        _slots.Clear();

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, hotbarContainer);
            var ui = go.GetComponent<InventorySlotUI>();

            ui.SlotIndex = i;
            ui.SlotType = SlotType.Hotbar;
            ui.Manager = _currentManager;
            
            var layout = go.GetComponent<LayoutElement>();
            if (layout == null)
                layout = go.AddComponent<LayoutElement>();

            bool isCenter = (i == count / 2);
            Vector2 size = isCenter ? centerSize : normalSize;

            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;

            _slots.Add(ui);
        }
    }

    // =====================================================================
    // UI Refresh
    // =====================================================================

    public void Refresh()
    {
        if (_currentManager == null) return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Refresh(sys.HotbarSlots[i]);

            bool isSelected = (_currentManager.SelectedHotbarIndex == i);
            _slots[i].SetHighlight(isSelected);
        }
    }

    // =====================================================================
    // Input (scroll wheel)
    // =====================================================================

    private void Update()
    {
        if (_currentManager == null || !Application.isFocused) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        HandleScrollInput(scroll);
    }

    private void HandleScrollInput(float scroll)
    {
        int current = _currentManager.SelectedHotbarIndex;
        int max = _slots.Count;

        current += (scroll > 0 ? -1 : 1);

        if (current < 0) current = max - 1;
        if (current >= max) current = 0;

        _currentManager.Input_SetSelectedHotbar(current);
    }
}