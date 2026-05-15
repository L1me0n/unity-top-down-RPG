using UnityEngine;

public class BlockingMenuPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Lock Owner")]
    [SerializeField] private string ownerKey = UIInputBlocker.LockDebugPanel;

    [Header("Blocking Rules")]
    [SerializeField] private bool blockGameplay = true;
    [SerializeField] private bool blockUpgradeMenu = true;
    [SerializeField] private bool blockPauseToggle = true;
    [SerializeField] private bool blockInventoryToggle = true;
    [SerializeField] private bool blockClueMenuToggle = true;
    [SerializeField] private bool blockTradeItemHotkeys = true;

    [Header("Time")]
    [SerializeField] private bool pauseGameWhileOpen = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    public bool IsOpen { get; private set; }

    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnDisable()
    {
        if (IsOpen)
            ForceClose();
    }

    public bool TryOpen()
    {
        if (IsOpen)
            return false;

        if (UIInputBlocker.BlockGameplayInput)
        {
            Log("Cannot open because another blocking UI is already open.");
            return false;
        }

        Open();
        return true;
    }

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        ApplyLocks(true);

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        Log("Opened.");
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        ApplyLocks(false);

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        Log("Closed.");
    }

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            TryOpen();
    }

    public void ForceClose()
    {
        IsOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        ApplyLocks(false);

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        Log("Force closed.");
    }

    private void ApplyLocks(bool blocked)
    {
        if (blockGameplay)
            UIInputBlocker.SetGameplayBlocked(ownerKey, blocked);

        if (blockUpgradeMenu)
            UIInputBlocker.SetUpgradeMenuBlocked(ownerKey, blocked);

        if (blockPauseToggle)
            UIInputBlocker.SetPauseToggleBlocked(ownerKey, blocked);

        if (blockInventoryToggle)
            UIInputBlocker.SetInventoryToggleBlocked(ownerKey, blocked);

        if (blockClueMenuToggle)
            UIInputBlocker.SetClueMenuToggleBlocked(ownerKey, blocked);

        if (blockTradeItemHotkeys)
            UIInputBlocker.SetTradeItemHotkeysBlocked(ownerKey, blocked);
    }

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log($"[BlockingMenuPanel:{ownerKey}] {message} | {UIInputBlocker.GetDebugSummary()}", this);
    }
}