using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/New Item")]
public class ItemSO :  ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 10;
    public bool isActiveTool;
    public bool isTool;
}
