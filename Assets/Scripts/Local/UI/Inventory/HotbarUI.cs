using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private GameObject slotPrefab;

    [Header("Slot Sizes")]
    [SerializeField] private Vector2 normalSize = new Vector2(60, 60);
    [SerializeField] private Vector2 centerSize = new Vector2(90, 90);

    [Header("Center Slot Icon")]
    [SerializeField] private Vector2 centerIconSize = new Vector2(50, 50);

    private List<InventorySlotUI> _slots = new();
    private NetworkInventoryManager _currentManager;

    private void Start()
    {
        if (NetworkInventoryManager.Local != null)
            ConnectToLocalPlayer();

        NetworkInventoryManager.OnLocalPlayerSpawned += ConnectToLocalPlayer;
    }

    private void OnDestroy()
    {
        NetworkInventoryManager.OnLocalPlayerSpawned -= ConnectToLocalPlayer;

        if (_currentManager != null)
        {
            var sys = _currentManager.GetComponent<NetworkInventorySystem>();
            if (sys != null)
                sys.OnInventoryChanged -= Refresh;
        }
    }

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

            var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();

            bool isCenter = (i == count / 2);
            Vector2 size = isCenter ? centerSize : normalSize;

            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;

            if (isCenter)
            {
                var iconRect = ui.ItemIconRect;
                if (iconRect != null)
                    iconRect.sizeDelta = centerIconSize;
            }

            _slots.Add(ui);
        }
    }

    public void Refresh()
    {
        if (_currentManager == null) return;

        var sys = _currentManager.GetComponent<NetworkInventorySystem>();

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Refresh(sys.HotbarSlots[i]);

            _slots[i].SetHighlight(false);
        }
    }

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
        bool rotateRight = scroll < 0f; 
        _currentManager.Input_RotateHotbar(rotateRight);
    }
}