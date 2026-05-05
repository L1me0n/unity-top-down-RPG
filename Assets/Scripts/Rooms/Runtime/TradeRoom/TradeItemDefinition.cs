using System;

[Serializable]
public class TradeItemDefinition
{
    public TradeItemType itemType;
    public string displayName;
    public string description;
    public int soulCost;
    public int maxStack;
    public string hotkeyLabel;
    public bool isPassive;

    public TradeItemDefinition(
        TradeItemType itemType,
        string displayName,
        string description,
        int soulCost,
        int maxStack,
        string hotkeyLabel,
        bool isPassive)
    {
        this.itemType = itemType;
        this.displayName = displayName;
        this.description = description;
        this.soulCost = soulCost;
        this.maxStack = maxStack;
        this.hotkeyLabel = hotkeyLabel;
        this.isPassive = isPassive;
    }
}