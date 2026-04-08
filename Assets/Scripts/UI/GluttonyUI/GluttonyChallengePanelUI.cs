using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GluttonyChallengePanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text fullnessText;
    [SerializeField] private TMP_Text remainingText;
    [SerializeField] private TMP_Text selectedFeedAmountText;
    [SerializeField] private TMP_Text tempHPLossText;
    [SerializeField] private TMP_Text summaryText;

    [Header("Optional Bar")]
    [SerializeField] private Slider fullnessSlider;
    [SerializeField] private bool useNormalizedSlider = false;

    [Header("Buttons")]
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;
    [SerializeField] private Button feedButton;

    [Header("Button Labels")]
    [SerializeField] private TMP_Text feedButtonLabel;

    [Header("Behavior")]
    [SerializeField] private bool hideRootOnAwake = true;
    [SerializeField] private bool clearSummaryOnShow = false;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private GluttonyChallengeRuntime boundRuntime;

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    private void Awake()
    {
        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(HandleDecreaseClicked);

        if (increaseButton != null)
            increaseButton.onClick.AddListener(HandleIncreaseClicked);

        if (feedButton != null)
            feedButton.onClick.AddListener(HandleFeedClicked);

        if (hideRootOnAwake)
            HideRootImmediate();
        
        if (fullnessSlider != null)
            fullnessSlider.interactable = false;
    }

    private void OnDestroy()
    {
        UnbindRuntime();

        if (decreaseButton != null)
            decreaseButton.onClick.RemoveListener(HandleDecreaseClicked);

        if (increaseButton != null)
            increaseButton.onClick.RemoveListener(HandleIncreaseClicked);

        if (feedButton != null)
            feedButton.onClick.RemoveListener(HandleFeedClicked);
    }

    public void ShowForRuntime(GluttonyChallengeRuntime runtime)
    {
        if (runtime == null)
        {
            HidePanel();
            return;
        }

        if (boundRuntime == runtime && IsVisible)
        {
            RefreshAll();
            return;
        }

        UnbindRuntime();
        boundRuntime = runtime;

        boundRuntime.OnStateChanged += HandleRuntimeStateOrDataChanged;
        boundRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;
        boundRuntime.OnTurnResolved += HandleTurnResolved;

        ShowRoot();

        if (clearSummaryOnShow && summaryText != null)
            summaryText.text = "";

        RefreshAll();

        Log("Panel shown and runtime bound.");
    }

    public void HidePanel()
    {
        UnbindRuntime();
        HideRootImmediate();
        Log("Panel hidden.");
    }

    public void RequestIncreaseAmount(int amount = 1)
    {
        if (boundRuntime == null)
            return;

        boundRuntime.TryIncreaseSelectedFeedAmount(amount);
    }

    public void RequestDecreaseAmount(int amount = 1)
    {
        if (boundRuntime == null)
            return;

        boundRuntime.TryDecreaseSelectedFeedAmount(amount);
    }

    public void RequestConfirmFeed()
    {
        if (boundRuntime == null)
            return;

        bool confirmed = boundRuntime.TryConfirmFeed();
        Log($"Feed clicked | confirmed={confirmed}");
    }

    private void HandleDecreaseClicked()
    {
        RequestDecreaseAmount(1);
    }

    private void HandleIncreaseClicked()
    {
        RequestIncreaseAmount(1);
    }

    private void HandleFeedClicked()
    {
        RequestConfirmFeed();
    }

    private void HandleRuntimeStateOrDataChanged(GluttonyChallengeRuntime runtime)
    {
        if (runtime != boundRuntime)
            return;

        RefreshAll();
    }

    private void HandleRuntimeStateChanged(
        GluttonyChallengeRuntime runtime,
        GluttonyChallengeRuntime.GluttonyRuntimeState newState)
    {
        if (runtime != boundRuntime)
            return;

        RefreshAll();
    }

    private void HandleTurnResolved(GluttonyChallengeRuntime runtime, GluttonyTurnResult result)
    {
        if (runtime != boundRuntime)
            return;

        if (summaryText != null)
            summaryText.text = result != null ? result.summaryText : "";

        RefreshAll();
    }

    private void RefreshAll()
    {
        if (boundRuntime == null)
            return;

        int fullnessCurrent = boundRuntime.FullnessCurrent;
        int fullnessTarget = Mathf.Max(1, boundRuntime.FullnessTarget);
        int remainingToTarget = boundRuntime.RemainingToTarget;
        int selectedAmount = Mathf.Max(0, boundRuntime.SelectedFeedAmount);
        int tempHpLoss = Mathf.Max(0, boundRuntime.AccumulatedTempHPLoss);

        bool isAwaitingInput = boundRuntime.IsAwaitingInput;
        bool isResolved = boundRuntime.IsResolved;

        if (fullnessSlider != null)
            fullnessSlider.interactable = false;

        if (titleText != null)
            titleText.text = "GLUTTONY";

        if (fullnessText != null)
            fullnessText.text = $"Fullness: {fullnessCurrent} / {fullnessTarget}";

        if (remainingText != null)
            remainingText.text = $"Remaining: {remainingToTarget}";

        if (selectedFeedAmountText != null)
            selectedFeedAmountText.text = $"Feed Amount: {selectedAmount}";

        if (tempHPLossText != null)
            tempHPLossText.text = $"Temp HP Loss: {tempHpLoss}";

        if (summaryText != null && string.IsNullOrWhiteSpace(summaryText.text))
            summaryText.text = boundRuntime.LastTurnSummary ?? "";

        RefreshFullnessSlider(fullnessCurrent, fullnessTarget);

        if (decreaseButton != null)
            decreaseButton.interactable =
                isAwaitingInput &&
                !isResolved &&
                selectedAmount > boundRuntime.MinSelectableFeedAmount;

        if (increaseButton != null)
            increaseButton.interactable =
                isAwaitingInput &&
                !isResolved &&
                selectedAmount < boundRuntime.MaxSelectableFeedAmount;

        if (feedButton != null)
        {
            feedButton.gameObject.SetActive(true);
            feedButton.interactable = isAwaitingInput && !isResolved;
        }

        if (feedButtonLabel != null)
            feedButtonLabel.text = isResolved ? "DONE" : "FEED";
    }

    private void RefreshFullnessSlider(int fullnessCurrent, int fullnessTarget)
    {
        if (fullnessSlider == null)
            return;

        if (useNormalizedSlider)
        {
            fullnessSlider.minValue = 0f;
            fullnessSlider.maxValue = 1f;
            fullnessSlider.value = fullnessTarget <= 0
                ? 0f
                : Mathf.Clamp01((float)fullnessCurrent / fullnessTarget);
        }
        else
        {
            fullnessSlider.minValue = 0f;
            fullnessSlider.maxValue = Mathf.Max(1, fullnessTarget);
            fullnessSlider.value = Mathf.Clamp(fullnessCurrent, 0, fullnessTarget);
        }
    }

    private void ShowRoot()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);
    }

    private void HideRootImmediate()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    private void UnbindRuntime()
    {
        if (boundRuntime != null)
        {
            boundRuntime.OnStateChanged -= HandleRuntimeStateOrDataChanged;
            boundRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
            boundRuntime.OnTurnResolved -= HandleTurnResolved;
            boundRuntime = null;
        }
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[GluttonyChallengePanelUI] {message}", this);
    }
}