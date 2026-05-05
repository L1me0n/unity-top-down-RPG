using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeShopItemButtonUI : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private TradeItemType itemType = TradeItemType.None;

    [Header("Button")]
    [SerializeField] private Button button;

    [Header("Texts")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;

    [Header("Debug")]
    [SerializeField] private bool logClicks;

    public TradeItemType ItemType => itemType;
    public Button Button => button;

    private TradeItemDefinition cachedDefinition;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        RefreshStaticText();
    }

    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();

        RefreshStaticText();
    }

    public void SetItemType(TradeItemType newItemType)
    {
        itemType = newItemType;
        RefreshStaticText();
    }

    public void RefreshStaticText()
    {
        cachedDefinition = TradeItemCatalog.Get(itemType);

        if (cachedDefinition == null)
        {
            if (nameText != null)
                nameText.text = "Unknown Item";

            if (countText != null)
                countText.text = "0/0";

            if (descriptionText != null)
                descriptionText.text = "";

            if (priceText != null)
                priceText.text = "";

            if (button != null)
                button.interactable = false;

            return;
        }

        if (nameText != null)
            nameText.text = cachedDefinition.displayName;

        if (descriptionText != null)
            descriptionText.text = cachedDefinition.description;

        if (priceText != null)
            priceText.text = $"{cachedDefinition.soulCost} Souls";
    }

    public void RefreshDynamicState(int currentCount, bool canBuy)
    {
        cachedDefinition = TradeItemCatalog.Get(itemType);

        if (cachedDefinition == null)
        {
            if (countText != null)
                countText.text = "0/0";

            if (button != null)
                button.interactable = false;

            return;
        }

        int clampedCount = Mathf.Clamp(currentCount, 0, cachedDefinition.maxStack);

        if (countText != null)
            countText.text = $"{clampedCount}/{cachedDefinition.maxStack}";

        if (button != null)
            button.interactable = canBuy;
    }

    public void AddClickListener(UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);

        if (logClicks)
        {
            button.onClick.AddListener(() =>
            {
                Debug.Log($"[TradeShopItemButtonUI] Clicked {itemType}", this);
            });
        }
    }
}