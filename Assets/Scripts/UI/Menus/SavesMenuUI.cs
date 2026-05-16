using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavesMenuUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text saveInfoText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Buttons")]
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;

    [Header("References")]
    [SerializeField] private RunSaveManager runSaveManager;

    [Header("Delete Confirmation")]
    [SerializeField] private float deleteConfirmSeconds = 3f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private PauseMenuUI parentPauseMenu;

    private bool isOpen;
    private bool waitingForDeleteConfirm;
    private float deleteConfirmTimer;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();

        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (titleText != null)
            titleText.text = "SAVES";

        ClearFeedback();

        if (loadButton != null)
            loadButton.onClick.AddListener(LoadSave);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteSave);

        if (backButton != null)
            backButton.onClick.AddListener(Back);
    }

    private void OnDestroy()
    {
        if (loadButton != null)
            loadButton.onClick.RemoveListener(LoadSave);

        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(DeleteSave);

        if (backButton != null)
            backButton.onClick.RemoveListener(Back);
    }

    private void Update()
    {
        UpdateDeleteConfirmTimer();

        if (!isOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Back();
    }

    public void OpenFromPause(PauseMenuUI pauseMenu)
    {
        parentPauseMenu = pauseMenu;

        if (parentPauseMenu != null)
            parentPauseMenu.HidePanelForSubmenu();

        Open();
    }

    public void OpenStandalone()
    {
        parentPauseMenu = null;
        Open();
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (rootPanel != null)
            rootPanel.SetActive(true);

        waitingForDeleteConfirm = false;
        deleteConfirmTimer = 0f;

        ClearFeedback();
        Refresh();

        // If opened from Pause, Pause already owns gameplay/time blocking.
        // If opened standalone later from Main Menu or another place, this lock is harmless
        // and keeps the panel modal.
        UIInputBlocker.SetGameplayBlocked(UIInputBlocker.LockSavesMenu, true);
        UIInputBlocker.SetUpgradeMenuBlocked(UIInputBlocker.LockSavesMenu, true);
        UIInputBlocker.SetPauseToggleBlocked(UIInputBlocker.LockSavesMenu, true);
        UIInputBlocker.SetInventoryToggleBlocked(UIInputBlocker.LockSavesMenu, true);
        UIInputBlocker.SetClueMenuToggleBlocked(UIInputBlocker.LockSavesMenu, true);
        UIInputBlocker.SetTradeItemHotkeysBlocked(UIInputBlocker.LockSavesMenu, true);

        Log("Opened.");
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (rootPanel != null)
            rootPanel.SetActive(false);

        waitingForDeleteConfirm = false;
        deleteConfirmTimer = 0f;

        ClearFeedback();

        UIInputBlocker.ReleaseOwner(UIInputBlocker.LockSavesMenu);

        Log("Closed.");
    }

    public void Back()
    {
        Close();

        if (parentPauseMenu != null)
            parentPauseMenu.ShowPanelAfterSubmenu();
    }

    public void Refresh()
    {
        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();

        bool hasSave = runSaveManager != null && runSaveManager.HasSaveFile();

        if (loadButton != null)
            loadButton.interactable = hasSave;

        if (deleteButton != null)
            deleteButton.interactable = hasSave;

        if (saveInfoText == null)
            return;

        if (!hasSave)
        {
            saveInfoText.text = "No save found.";
            return;
        }

        if (runSaveManager.TryGetSavePreview(out RunSavePreviewData preview) && preview.hasSave)
        {
            saveInfoText.text =
                "Current Save Found\n\n" +
                $"Room: {preview.roomX}, {preview.roomY}\n" +
                $"Level: {preview.level}\n" +
                $"Unspent Points: {preview.unspentPoints}\n" +
                $"Souls: {preview.souls}\n" +
                $"XP: {preview.xp}";
        }
        else
        {
            saveInfoText.text = "Save found, but preview could not be read.";
        }
    }

    private void LoadSave()
    {
        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();

        if (runSaveManager == null)
        {
            ShowFeedback("Load failed: no save manager found.");
            return;
        }

        if (!runSaveManager.HasSaveFile())
        {
            ShowFeedback("No save found.");
            Refresh();
            return;
        }

        // Close UI locks before loading so gameplay does not remain frozen.
        Close();

        if (parentPauseMenu != null)
            parentPauseMenu.CloseForExternalMenuAction();

        Time.timeScale = 1f;

        bool loaded = runSaveManager.TryLoad();

        if (!loaded)
        {
            // If load somehow failed, reopen the pause panel so the player is not stranded.
            if (parentPauseMenu != null)
                parentPauseMenu.Open();

            ShowFeedback("Load failed.");
            return;
        }

        Log("Loaded save.");
    }

    private void DeleteSave()
    {
        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();

        if (runSaveManager == null)
        {
            ShowFeedback("Delete failed: no save manager found.");
            return;
        }

        if (!runSaveManager.HasSaveFile())
        {
            ShowFeedback("No save found.");
            Refresh();
            return;
        }

        if (!waitingForDeleteConfirm)
        {
            waitingForDeleteConfirm = true;
            deleteConfirmTimer = deleteConfirmSeconds;
            ShowFeedback("Click Delete again to confirm.");
            return;
        }

        bool deleted = runSaveManager.TryDeleteSave();

        waitingForDeleteConfirm = false;
        deleteConfirmTimer = 0f;

        if (deleted)
            ShowFeedback("Save deleted.");
        else
            ShowFeedback("No save found.");

        Refresh();
    }

    private void UpdateDeleteConfirmTimer()
    {
        if (!waitingForDeleteConfirm)
            return;

        deleteConfirmTimer -= Time.unscaledDeltaTime;

        if (deleteConfirmTimer > 0f)
            return;

        waitingForDeleteConfirm = false;
        deleteConfirmTimer = 0f;

        if (isOpen)
            ShowFeedback("Delete confirmation expired.");
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }

    private string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log($"[SavesMenuUI] {message} | {UIInputBlocker.GetDebugSummary()}", this);
    }
}