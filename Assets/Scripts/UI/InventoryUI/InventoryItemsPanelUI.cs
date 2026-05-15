using System.Text;
using UnityEngine;
using TMPro;

public class InventoryItemsPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TradeItemInventory inventory;
    [SerializeField] private TradeItemEffectManager effectManager;

    [Header("Text")]
    [SerializeField] private TMP_Text itemsTitleText;
    [SerializeField] private TMP_Text itemsListText;

    [Header("Options")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool showActiveTradeItemTimers = true;

    private void Awake()
    {
        FindReferencesIfNeeded();

        if (itemsTitleText != null)
            itemsTitleText.text = "Shop Items";
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged += HandleInventoryChanged;

        if (effectManager != null)
            effectManager.OnEffectsChanged += HandleTradeEffectsChanged;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= HandleInventoryChanged;

        if (effectManager != null)
            effectManager.OnEffectsChanged -= HandleTradeEffectsChanged;
    }

    public void Refresh()
    {
        FindReferencesIfNeeded();

        if (itemsListText == null)
            return;

        if (inventory == null)
        {
            itemsListText.text = "No item inventory found.";
            return;
        }

        StringBuilder sb = new StringBuilder();

        foreach (TradeItemDefinition definition in TradeItemCatalog.AllItems)
        {
            if (definition == null)
                continue;

            int count = inventory.GetCount(definition.itemType);

            if (count <= 0)
                continue;

            sb.Append("• ");
            sb.Append(definition.displayName);
            sb.Append(" x");
            sb.Append(count);
            sb.Append("/");
            sb.Append(definition.maxStack);

            string activeText = GetActiveTimerText(definition.itemType);
            if (!string.IsNullOrWhiteSpace(activeText))
            {
                sb.Append(" ");
                sb.Append(activeText);
            }

            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(definition.description))
            {
                sb.Append("  ");
                sb.Append(definition.description.Replace("\n", " "));
                sb.AppendLine();
            }
        }

        itemsListText.text = sb.Length > 0
            ? sb.ToString()
            : "No shop items owned.";
    }

    private void HandleInventoryChanged()
    {
        Refresh();
    }

    private void HandleTradeEffectsChanged()
    {
        Refresh();
    }

    private void FindReferencesIfNeeded()
    {
        if (!autoFindReferences)
            return;

        if (inventory == null)
            inventory = FindFirstObjectByType<TradeItemInventory>();

        if (effectManager == null)
            effectManager = FindFirstObjectByType<TradeItemEffectManager>();
    }

    private string GetActiveTimerText(TradeItemType itemType)
    {
        if (!showActiveTradeItemTimers)
            return "";

        if (effectManager == null)
            return "";

        if (!effectManager.IsActive(itemType))
            return "";

        return $"({effectManager.GetRemaining(itemType):0.0}s active)";
    }
}