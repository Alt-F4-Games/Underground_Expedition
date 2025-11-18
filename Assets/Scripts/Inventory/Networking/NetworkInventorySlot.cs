using Fusion;

[System.Serializable]
public struct NetworkInventorySlot : INetworkStruct
{
    public int ItemId;    // -1 o 0 será "Vacío"
    public int Quantity;

    // Constructor helper
    public NetworkInventorySlot(int id, int qty)
    {
        ItemId = id;
        Quantity = qty;
    }
    
    // Helper para saber si está vacío rápidamente
    public bool IsEmpty => ItemId <= 0 || Quantity <= 0;
}