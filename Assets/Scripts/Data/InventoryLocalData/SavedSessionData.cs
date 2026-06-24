using System;
using System.Collections.Generic;

[Serializable]
public class SavedSessionData
{
    public List<SavedSlot> baseSlots = new();   
    public List<SavedSlot> equipSlots = new();  
    public List<SavedSlot> hotbarSlots = new(); 
    
    public int level = 1;
    public int currentExp = 0;
    public int baseExp = 100;     
    public int skillPoints = 0;
    
    public int currentHealth = -1; 
    public float currentStamina = -1f; 
}

[Serializable]
public class SavedSlot
{
    public int itemId;
    public int quantity;

    public SavedSlot(int id, int qty)
    {
        itemId = id;
        quantity = qty;
    }
}