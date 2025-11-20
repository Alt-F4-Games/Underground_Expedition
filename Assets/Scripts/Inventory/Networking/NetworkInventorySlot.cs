using Fusion;

[System.Serializable]
public struct NetworkInventorySlot : INetworkStruct
{
    public int ItemId;    
    public int Quantity;
    
    public NetworkInventorySlot(int id, int qty)
    {
        ItemId = id;
        Quantity = qty;
    }
    public bool IsEmpty => ItemId <= 0 || Quantity <= 0;
}