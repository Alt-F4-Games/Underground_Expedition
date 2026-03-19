using Fusion;

/// <summary>
/// NetworkInventorySlot
/// --------------------
/// Structure that represents a single synchronizable inventory slot.
/// Designed to be as lightweight as possible because Fusion sends this struct
/// through a NetworkArray.
/// </summary>


[System.Serializable]
public struct NetworkInventorySlot : INetworkStruct
{
    public int ItemId;      //ID of item (0 o negative = empty)  
    public int Quantity;    //Items quantity in slot
    
    //--------------Constructor-------------------
    public NetworkInventorySlot(int id, int qty)
    {
        ItemId = id;
        Quantity = qty;
    }
    public bool IsEmpty => ItemId <= 0 || Quantity <= 0;    //Empty slot validation
}