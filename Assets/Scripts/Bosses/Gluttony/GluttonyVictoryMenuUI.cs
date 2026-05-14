using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GluttonyVictoryMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossProgressionManager bossProgression;
    [SerializeField] private RunSaveManager runSaveManager;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text narrativeText;
    [SerializeField] private TMP_Text clueTitleText;
    [SerializeField] private TMP_Text clueText;
    [SerializeField] private TMP_Text endingText;
    [SerializeField] private Button continueButton;

    [Header("Content")]
    [SerializeField] private string title = "GLUTTONY DEFEATED";

    [TextArea(3, 8)]
    [SerializeField] private string narrative =
        "Gluttony's excessive injuries rupture his swollen body.\n\n" +
        "The hellhounds he had devoured tear him apart from within.";

    [SerializeField] private string clueTitle = "HUNGER CLUE RECEIVED";
    [SerializeField] private string hungerClue = "+69";
    [SerializeField] private string ending = "MVP COMPLETE\n\nThe next hunger waits deeper in Hell.";

    [Header("Behavior")]
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private bool saveWhenShown = true;
    [SerializeField] private bool saveWhenClosed = true;
    [SerializeField] private float showDelaySeconds = 0.75f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool isOpen;
    private Coroutine showRoutine;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        AutoFindReferences();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(Close);
            continueButton.onClick.AddListener(Close);
        }

        ApplyStaticText();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        AutoFindReferences();

        if (bossProgression != null)
            bossProgression.OnGluttonyBossDefeated += HandleGluttonyBossDefeated;
    }

    private void OnDisable()
    {
        if (bossProgression != null)
            bossProgression.OnGluttonyBossDefeated -= HandleGluttonyBossDefeated;

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        if (isOpen)
            ForceClose();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            Close();
        }
    }

    private void AutoFindReferences()
    {
        if (bossProgression == null)
            bossProgression = BossProgressionManager.Instance;

        if (bossProgression == null)
            bossProgression = FindFirstObjectByType<BossProgressionManager>();

        if (runSaveManager == null)
            runSaveManager = FindFirstObjectByType<RunSaveManager>();
    }

    private void HandleGluttonyBossDefeated()
    {
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(CoShowAfterDelay());
    }

    private IEnumerator CoShowAfterDelay()
    {
        float timer = 0f;

        while (timer < showDelaySeconds)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        showRoutine = null;
        Show();
    }

    public void Show()
    {
        if (isOpen)
            return;

        isOpen = true;

        ApplyStaticText();

        if (panelRoot != null)
            panelRoot.SetActive(true);

        UIInputBlocker.BlockGameplayInput = true;
        UIInputBlocker.BlockUpgradeMenuToggle = true;

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        if (saveWhenShown && runSaveManager != null)
            runSaveManager.Save();

        Log("Gluttony victory menu opened.");
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        UIInputBlocker.BlockGameplayInput = false;
        UIInputBlocker.BlockUpgradeMenuToggle = false;

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;

        if (saveWhenClosed && runSaveManager != null)
            runSaveManager.Save();

        Log("Gluttony victory menu closed.");
    }

    private void ForceClose()
    {
        isOpen = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        UIInputBlocker.BlockGameplayInput = false;
        UIInputBlocker.BlockUpgradeMenuToggle = false;

        if (pauseGameWhileOpen)
            Time.timeScale = previousTimeScale;
    }

    private void ApplyStaticText()
    {
        if (titleText != null)
            titleText.text = title;

        if (narrativeText != null)
            narrativeText.text = narrative;

        if (clueTitleText != null)
            clueTitleText.text = clueTitle;

        if (clueText != null)
            clueText.text = hungerClue;

        if (endingText != null)
            endingText.text = ending;
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[GluttonyVictoryMenuUI] " + message, this);
    }

    [ContextMenu("Debug Show Victory Menu")]
    private void DebugShowVictoryMenu()
    {
        Show();
    }
}