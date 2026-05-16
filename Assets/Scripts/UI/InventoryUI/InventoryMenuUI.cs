using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryMenuUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;

    [Header("Header")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text closeHintText;
    [SerializeField] private Button closeButton;

    [Header("Sub Panels")]
    [SerializeField] private InventoryStatsPanelUI statsPanel;
    [SerializeField] private InventoryEffectsPanelUI effectsPanel;
    [SerializeField] private InventoryItemsPanelUI itemsPanel;
    [SerializeField] private InventoryBranchesPanelUI branchesPanel;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.L;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Time")]
    [SerializeField] private bool pauseGameWhileOpen = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool isOpen;
    private float previousTimeScale = 1f;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (titleText != null)
            titleText.text = "Sinner's Ledger";

        if (closeHintText != null)
            closeHintText.text = "Press L or Esc to close";

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        if (isOpen)
            ReleaseLocksAndTime();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen)
                Close();
            else
                TryOpen();
        }

        if (isOpen && Input.GetKeyDown(closeKey))
            Close();
    }

    public bool TryOpen()
    {
        if (isOpen)
            return false;

        if (UIInputBlocker.BlockGameplayInput || UIInputBlocker.BlockInventoryToggle)
        {
            Log("Cannot open because another blocking UI is active.");
            return false;
        }

        Open();
        return true;
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        RefreshAll();

        if (rootPanel != null)
            rootPanel.SetActive(true);

        ApplyLocksAndTime();

        Log("Opened.");
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (rootPanel != null)
            rootPanel.SetActive(false);

        ReleaseLocksAndTime();

        Log("Closed.");
    }

    public void RefreshAll()
    {
        if (statsPanel != null)
            statsPanel.Refresh();

        if (effectsPanel != null)
            effectsPanel.Refresh();

        if (itemsPanel != null)
            itemsPanel.Refresh();

        if (branchesPanel != null)
            branchesPanel.Refresh();
    }

    private void ApplyLocksAndTime()
    {
        UIInputBlocker.SetGameplayBlocked(UIInputBlocker.LockInventoryMenu, true);
        UIInputBlocker.SetUpgradeMenuBlocked(UIInputBlocker.LockInventoryMenu, true);
        UIInputBlocker.SetPauseToggleBlocked(UIInputBlocker.LockInventoryMenu, true);
        UIInputBlocker.SetClueMenuToggleBlocked(UIInputBlocker.LockInventoryMenu, true);
        UIInputBlocker.SetTradeItemHotkeysBlocked(UIInputBlocker.LockInventoryMenu, true);

        // Important:
        // We do NOT set InventoryToggleBlocked here because this menu itself uses I to close.
        // If another menu wants to block Inventory, it will use BlockInventoryToggle.

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    private void ReleaseLocksAndTime()
    {
        UIInputBlocker.ReleaseOwner(UIInputBlocker.LockInventoryMenu);

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;
    }

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log($"[InventoryMenuUI] {message} | {UIInputBlocker.GetDebugSummary()}", this);
    }
}