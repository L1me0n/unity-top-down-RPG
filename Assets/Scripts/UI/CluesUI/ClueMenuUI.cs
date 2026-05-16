using TMPro;
using UnityEngine;

public class ClueMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text hintText;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private bool blockGameplayWhileOpen = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private BossProgressionManager bossProgressionManager;
    private bool isOpen;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        bossProgressionManager = FindFirstObjectByType<BossProgressionManager>();

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (titleText != null)
            titleText.text = "CLUES";

        if (hintText != null)
            hintText.text = "Press C or Esc to close";

        RefreshText();
    }

    private void OnEnable()
    {
        if (bossProgressionManager == null)
            bossProgressionManager = FindFirstObjectByType<BossProgressionManager>();

        if (bossProgressionManager != null)
            bossProgressionManager.OnBossProgressionChanged += HandleBossProgressionChanged;
    }

    private void OnDisable()
    {
        if (bossProgressionManager != null)
            bossProgressionManager.OnBossProgressionChanged -= HandleBossProgressionChanged;

        if (isOpen)
            ForceCloseWithoutEvents();
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

    public void TryOpen()
    {
        if (isOpen)
            return;

        if (UIInputBlocker.BlockGameplayInput || UIInputBlocker.BlockClueMenuToggle)
        {
            Log("Cannot open clue menu because gameplay input is already blocked.");
            return;
        }

        Open();
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        RefreshText();

        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (blockGameplayWhileOpen)
        {
            UIInputBlocker.SetGameplayBlocked(UIInputBlocker.LockClueMenu, true);
            UIInputBlocker.SetUpgradeMenuBlocked(UIInputBlocker.LockClueMenu, true);
            UIInputBlocker.SetPauseToggleBlocked(UIInputBlocker.LockClueMenu, true);
            UIInputBlocker.SetInventoryToggleBlocked(UIInputBlocker.LockClueMenu, true);
            UIInputBlocker.SetTradeItemHotkeysBlocked(UIInputBlocker.LockClueMenu, true);
        }

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        Log("Opened clue menu.");
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (blockGameplayWhileOpen)
        {
            UIInputBlocker.ReleaseOwner(UIInputBlocker.LockClueMenu);
        }

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        Log("Closed clue menu.");
    }

    private void ForceCloseWithoutEvents()
    {
        isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (blockGameplayWhileOpen)
        {
            UIInputBlocker.ReleaseOwner(UIInputBlocker.LockClueMenu);
        }

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;
    }

    private void HandleBossProgressionChanged()
    {
        RefreshText();
    }

    private void RefreshText()
    {
        if (bodyText == null)
            return;

        if (bossProgressionManager == null)
            bossProgressionManager = FindFirstObjectByType<BossProgressionManager>();

        if (bossProgressionManager == null)
        {
            bodyText.text = "No clues found.";
            return;
        }

        bodyText.text = bossProgressionManager.GetFullClueDisplayText();
    }

    public void OpenFromExternalMenu()
    {
        if (isOpen)
            return;

        Open();
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[ClueMenuUI] " + message, this);
    }
}