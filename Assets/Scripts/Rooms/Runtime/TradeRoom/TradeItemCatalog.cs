using System.Collections.Generic;
using UnityEngine;

public static class TradeItemCatalog
{
    private static readonly TradeItemDefinition[] allItems =
    {
        new TradeItemDefinition(
            TradeItemType.ChronosSpell,
            "Chronos Spell",
            "Time is frozen\n5 seconds",
            60,
            5,
            "1",
            false
        ),

        new TradeItemDefinition(
            TradeItemType.BloodlustPotion,
            "Bloodlust Potion",
            "No AP loss on Attack\n15 seconds",
            100,
            5,
            "2",
            false
        ),

        new TradeItemDefinition(
            TradeItemType.EctoplasmPotion,
            "Ectoplasm Potion",
            "+3 sec Disappear\n30 seconds",
            35,
            5,
            "3",
            false
        ),

        new TradeItemDefinition(
            TradeItemType.HorsemenRing,
            "Horsemen Ring",
            "Prevents one death\nRestores full HP",
            200,
            5,
            "4",
            true
        )
    };

    public static IReadOnlyList<TradeItemDefinition> AllItems => allItems;

    public static bool IsValidItem(TradeItemType itemType)
    {
        return itemType != TradeItemType.None && Get(itemType) != null;
    }

    public static TradeItemDefinition Get(TradeItemType itemType)
    {
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i].itemType == itemType)
                return allItems[i];
        }

        return null;
    }

    public static int GetMaxStack(TradeItemType itemType)
    {
        TradeItemDefinition definition = Get(itemType);
        return definition != null ? definition.maxStack : 0;
    }

    public static int GetSoulCost(TradeItemType itemType)
    {
        TradeItemDefinition definition = Get(itemType);
        return definition != null ? definition.soulCost : 0;
    }

    public static string GetDisplayName(TradeItemType itemType)
    {
        TradeItemDefinition definition = Get(itemType);
        return definition != null ? definition.displayName : itemType.ToString();
    }

    public static void DebugPrintCatalog()
    {
        for (int i = 0; i < allItems.Length; i++)
        {
            TradeItemDefinition item = allItems[i];

            Debug.Log(
                $"[TradeItemCatalog] {item.itemType} | " +
                $"{item.displayName} | " +
                $"Cost={item.soulCost} | " +
                $"Max={item.maxStack} | " +
                $"Hotkey={item.hotkeyLabel} | " +
                $"Passive={item.isPassive}"
            );
        }
    }
}