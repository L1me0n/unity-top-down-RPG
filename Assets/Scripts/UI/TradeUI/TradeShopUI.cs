using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeShopUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Button closeButton;

    [Header("Shop Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text soulsText;
    [SerializeField] private TMP_Text messageText;

    [Header("Item Cards")]
    [SerializeField] private TradeShopItemButtonUI[] itemCards;

    [Header("References")]
    [SerializeField] private RunCurrency currency;
    [SerializeField] private TradeItemInventory inventory;

    [Header("Input")]
    [SerializeField] private KeyCode closeKey = KeyCode.E;

    [Header("Pause")]
    [SerializeField] private bool pauseGameWhileOpen = true;

    [Header("Message")]
    [SerializeField] private float messageVisibleSeconds = 1.6f;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    public bool IsOpen => rootPanel != null && rootPanel.activeSelf;
    public KeyCode CloseKey => closeKey;

    public event Action OnOpened;
    public event Action OnClosed;

    private float clearMessageAtUnscaledTime = -1f;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (currency == null)
            currency = FindFirstObjectByType<RunCurrency>();

        if (inventory == null)
            inventory = FindFirstObjectByType<TradeItemInventory>();

        if (itemCards == null || itemCards.Length == 0)
            itemCards = GetComponentsInChildren<TradeShopItemButtonUI>(true);

        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        SetupItemCards();

        if (titleText != null)
            titleText.text = "UNDER THE CROSSROADS";

        ClearMessage();
        RefreshAll();
    }

    private void OnEnable()
    {
        if (currency != null)
            currency.OnSoulsChanged += HandleSoulsChanged;

        if (inventory != null)
            inventory.OnInventoryChanged += HandleInventoryChanged;
    }

    private void OnDisable()
    {
        if (currency != null)
            currency.OnSoulsChanged -= HandleSoulsChanged;

        if (inventory != null)
            inventory.OnInventoryChanged -= HandleInventoryChanged;

        if (IsOpen)
            ForceCloseWithoutEvents();
    }

    private void Update()
    {
        if (!IsOpen)
            return;

        if (Input.GetKeyDown(closeKey))
            Close();

        if (clearMessageAtUnscaledTime > 0f && Time.unscaledTime >= clearMessageAtUnscaledTime)
            ClearMessage();
    }

    public void Open()
    {
        if (rootPanel == null)
        {
            Debug.LogWarning("[TradeShopUI] Cannot open shop because rootPanel is missing.", this);
            return;
        }

        if (IsOpen)
            return;

        rootPanel.SetActive(true);

        UIInputBlocker.SetGameplayBlocked(UIInputBlocker.LockTradeShop, true);
        UIInputBlocker.SetUpgradeMenuBlocked(UIInputBlocker.LockTradeShop, true);
        UIInputBlocker.SetTradeItemHotkeysBlocked(UIInputBlocker.LockTradeShop, true);
        UIInputBlocker.SetClueMenuToggleBlocked(UIInputBlocker.LockTradeShop, true);
        UIInputBlocker.SetInventoryToggleBlocked(UIInputBlocker.LockTradeShop, true);
        UIInputBlocker.SetPauseToggleBlocked(UIInputBlocker.LockTradeShop, true);

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        RefreshAll();
        ClearMessage();

        if (log)
            Debug.Log("[TradeShopUI] Shop opened.", this);

        OnOpened?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        rootPanel.SetActive(false);

        UIInputBlocker.ReleaseOwner(UIInputBlocker.LockTradeShop);

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        ClearMessage();

        if (log)
            Debug.Log("[TradeShopUI] Shop closed.", this);

        OnClosed?.Invoke();
    }

    private void ForceCloseWithoutEvents()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        UIInputBlocker.ReleaseOwner(UIInputBlocker.LockTradeShop);

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        ClearMessage();
    }

    private void SetupItemCards()
    {
        if (itemCards == null)
            return;

        for (int i = 0; i < itemCards.Length; i++)
        {
            TradeShopItemButtonUI card = itemCards[i];

            if (card == null)
                continue;

            card.RefreshStaticText();

            TradeItemType capturedType = card.ItemType;
            card.AddClickListener(() => TryBuyItem(capturedType));
        }
    }

    private void TryBuyItem(TradeItemType itemType)
    {
        if (currency == null)
        {
            ShowMessage("Currency system missing.");
            Debug.LogWarning("[TradeShopUI] Cannot buy because RunCurrency is missing.", this);
            return;
        }

        if (inventory == null)
        {
            ShowMessage("Inventory system missing.");
            Debug.LogWarning("[TradeShopUI] Cannot buy because TradeItemInventory is missing.", this);
            return;
        }

        TradeItemDefinition definition = TradeItemCatalog.Get(itemType);

        if (definition == null)
        {
            ShowMessage("Unknown item.");
            Debug.LogWarning($"[TradeShopUI] Cannot buy unknown item type: {itemType}", this);
            return;
        }

        if (inventory.IsFull(itemType))
        {
            ShowMessage($"{definition.displayName} is full.");
            RefreshAll();
            return;
        }

        if (!currency.CanSpendSouls(definition.soulCost))
        {
            ShowMessage("Not enough Souls.");
            RefreshAll();
            return;
        }

        if (!currency.TrySpendSouls(definition.soulCost))
        {
            ShowMessage("Not enough Souls.");
            RefreshAll();
            return;
        }

        bool added = inventory.TryAdd(itemType);

        if (!added)
        {
            // Safety refund. This should rarely happen because we already checked IsFull.
            currency.AddSouls(definition.soulCost);
            ShowMessage($"{definition.displayName} is full.");
            RefreshAll();
            return;
        }

        ShowMessage($"Purchased {definition.displayName}.");

        if (log)
        {
            Debug.Log(
                $"[TradeShopUI] Purchased {definition.displayName} for {definition.soulCost} Souls. " +
                $"Inventory: {inventory.GetDebugSummary()}",
                this
            );
        }

        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshSoulsText();
        RefreshItemCards();
    }

    private void RefreshSoulsText()
    {
        if (soulsText == null)
            return;

        int souls = currency != null ? currency.Souls : 0;
        soulsText.text = $"Souls: {souls}";
    }

    private void RefreshItemCards()
    {
        if (itemCards == null)
            return;

        for (int i = 0; i < itemCards.Length; i++)
        {
            TradeShopItemButtonUI card = itemCards[i];

            if (card == null)
                continue;

            TradeItemType itemType = card.ItemType;
            TradeItemDefinition definition = TradeItemCatalog.Get(itemType);

            if (definition == null)
            {
                card.RefreshDynamicState(0, false);
                continue;
            }

            int count = inventory != null ? inventory.GetCount(itemType) : 0;
            bool hasEnoughSouls = currency != null && currency.Souls >= definition.soulCost;
            bool hasSpace = inventory != null && !inventory.IsFull(itemType);

            bool canBuy = hasEnoughSouls && hasSpace;

            card.RefreshStaticText();
            card.RefreshDynamicState(count, canBuy);
        }
    }

    private void HandleSoulsChanged(int _)
    {
        if (IsOpen)
            RefreshAll();
    }

    private void HandleInventoryChanged()
    {
        if (IsOpen)
            RefreshAll();
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;
        clearMessageAtUnscaledTime = Time.unscaledTime + messageVisibleSeconds;
    }

    private void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";

        clearMessageAtUnscaledTime = -1f;
    }
}