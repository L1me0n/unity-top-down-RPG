using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button savesButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button exitGameButton;

    [Header("References")]
    [SerializeField] private RunSaveManager runSaveManager;
    [SerializeField] private SavesMenuUI savesMenuUI;
    [SerializeField] private TipsCodexUI tipsCodexUI;
    [SerializeField] private ClueMenuUI clueMenuUI;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    [Header("Time")]
    [SerializeField] private bool pauseGameWhileOpen = true;

    [Header("Feedback")]
    [SerializeField] private float feedbackDuration = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private bool isOpen;
    private float previousTimeScale = 1f;
    private float feedbackTimer;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();

        if (savesMenuUI == null)
            savesMenuUI = FindFirstObjectByType<SavesMenuUI>();

        if (tipsCodexUI == null)
            tipsCodexUI = FindFirstObjectByType<TipsCodexUI>();

        if (clueMenuUI == null)
            clueMenuUI = FindFirstObjectByType<ClueMenuUI>();

        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (titleText != null)
            titleText.text = "PAUSED";

        ClearFeedback();

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (saveGameButton != null)
            saveGameButton.onClick.AddListener(SaveGame);

        if (savesButton != null)
            savesButton.onClick.AddListener(OpenSavesMenu);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenTipsMenu);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OpenCluesMenu);

        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(ExitGame);
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(Resume);

        if (saveGameButton != null)
            saveGameButton.onClick.RemoveListener(SaveGame);

        if (savesButton != null)
            savesButton.onClick.RemoveListener(OpenSavesMenu);

        if (optionsButton != null)
            optionsButton.onClick.RemoveListener(OpenTipsMenu);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OpenCluesMenu);

        if (exitGameButton != null)
            exitGameButton.onClick.RemoveListener(ExitGame);

        if (isOpen)
            ReleaseLocksAndTime();
    }

    private void OnDisable()
    {
        if (isOpen)
            ForceClose();
    }

    private void Update()
    {
        UpdateFeedbackTimer();

        if (!Input.GetKeyDown(toggleKey))
            return;

        if (isOpen)
        {
            Close();
            return;
        }

        TryOpen();
    }

    public bool TryOpen()
    {
        if (isOpen)
            return false;

        if (UIInputBlocker.BlockGameplayInput || UIInputBlocker.BlockPauseToggle)
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

        if (rootPanel != null)
            rootPanel.SetActive(true);

        ClearFeedback();
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

        ClearFeedback();
        ReleaseLocksAndTime();

        Log("Closed.");
    }

    public void Resume()
    {
        Close();
    }

    public void SaveGame()
    {
        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();

        if (runSaveManager == null)
        {
            ShowFeedback("Save failed: no save manager found.");
            Debug.LogWarning("[PauseMenuUI] Save failed because RunSaveManager was not found.", this);
            return;
        }

        runSaveManager.AllowManualSavingAgain();
        runSaveManager.Save();
        ShowFeedback("Game saved.");

        Log("Saved game from pause menu.");
    }

    private void OpenSavesMenu()
    {
        if (savesMenuUI == null)
            savesMenuUI = FindFirstObjectByType<SavesMenuUI>();

        if (savesMenuUI == null)
        {
            ShowFeedback("Saves menu not assigned.");
            Debug.LogWarning("[PauseMenuUI] Saves button clicked, but SavesMenuUI was not found.", this);
            return;
        }

        savesMenuUI.OpenFromPause(this);
        Log("Opened Saves menu.");
    }

    private void OpenTipsMenu()
    {
        if (tipsCodexUI == null)
            tipsCodexUI = FindFirstObjectByType<TipsCodexUI>();

        if (tipsCodexUI == null)
        {
            ShowFeedback("Tips menu not assigned.");
            Debug.LogWarning("[PauseMenuUI] Tips button clicked, but TipsCodexUI was not found.", this);
            return;
        }

        CloseForExternalMenuAction();
        tipsCodexUI.OpenTips();

        Log("Opened Tips menu.");
    }

    private void OpenCluesMenu()
    {
        if (clueMenuUI == null)
            clueMenuUI = FindFirstObjectByType<ClueMenuUI>();

        if (clueMenuUI == null)
        {
            ShowFeedback("Clue menu not assigned.");
            Debug.LogWarning("[PauseMenuUI] Clues button clicked, but ClueMenuUI was not found.", this);
            return;
        }

        CloseForExternalMenuAction();
        clueMenuUI.OpenFromExternalMenu();

        Log("Opened Clue menu.");
    }

    public void ExitGame()
    {
        Log("Exit game requested.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ForceClose()
    {
        isOpen = false;

        if (rootPanel != null)
            rootPanel.SetActive(false);

        ClearFeedback();
        ReleaseLocksAndTime();

        Log("Force closed.");
    }

    public void HidePanelForSubmenu()
    {
        if (!isOpen)
            return;

        if (rootPanel != null)
            rootPanel.SetActive(false);

        ClearFeedback();
    }

    public void ShowPanelAfterSubmenu()
    {
        if (!isOpen)
            return;

        if (rootPanel != null)
            rootPanel.SetActive(true);

        ClearFeedback();
    }

    public void CloseForExternalMenuAction()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (rootPanel != null)
            rootPanel.SetActive(false);

        ClearFeedback();
        ReleaseLocksAndTime();

        Log("Closed for external menu action.");
    }

    private void ApplyLocksAndTime()
    {
        UIInputBlocker.SetGameplayBlocked(UIInputBlocker.LockPauseMenu, true);
        UIInputBlocker.SetUpgradeMenuBlocked(UIInputBlocker.LockPauseMenu, true);
        UIInputBlocker.SetInventoryToggleBlocked(UIInputBlocker.LockPauseMenu, true);
        UIInputBlocker.SetClueMenuToggleBlocked(UIInputBlocker.LockPauseMenu, true);
        UIInputBlocker.SetTradeItemHotkeysBlocked(UIInputBlocker.LockPauseMenu, true);

        // Important:
        // We do NOT set PauseToggleBlocked here because this menu itself uses Esc to close.

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    private void ReleaseLocksAndTime()
    {
        UIInputBlocker.ReleaseOwner(UIInputBlocker.LockPauseMenu);

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackTimer = feedbackDuration;
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";

        feedbackTimer = 0f;
    }

    private void UpdateFeedbackTimer()
    {
        if (feedbackText == null)
            return;

        if (feedbackTimer <= 0f)
            return;

        feedbackTimer -= Time.unscaledDeltaTime;

        if (feedbackTimer <= 0f)
            feedbackText.text = "";
    }

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log($"[PauseMenuUI] {message} | {UIInputBlocker.GetDebugSummary()}", this);
    }
}