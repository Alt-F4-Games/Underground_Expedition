using Fusion;

/// <summary>
/// NetworkInventorySlot
/// --------------------
/// Estructura que representa un solo slot de inventario sincronizable.
/// Diseñada para ser lo más liviana posible porque Fusion envía este struct
/// a través de NetworkArray.
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