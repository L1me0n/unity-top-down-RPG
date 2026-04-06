using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BettingChallengePanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("References")]
    [SerializeField] private BettingLaneControlUI[] laneControls = new BettingLaneControlUI[4];

    [SerializeField] private TMP_Text snapshotAPText;
    [SerializeField] private TMP_Text minimumBetText;
    [SerializeField] private TMP_Text totalBetText;
    [SerializeField] private TMP_Text remainingAPText;

    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text startButtonLabel;

    [Header("Behavior")]
    [SerializeField] private bool hideRootOnAwake = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private BettingChallengeRuntime boundRuntime;

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(HandleStartClicked);

        for (int i = 0; i < laneControls.Length; i++)
        {
            if (laneControls[i] == null)
                continue;

            laneControls[i].Initialize(this, i);
        }

        if (hideRootOnAwake)
            HideRootImmediate();
    }

    private void OnDestroy()
    {
        UnbindRuntime();

        if (startButton != null)
            startButton.onClick.RemoveListener(HandleStartClicked);
    }

    public void ShowForRuntime(BettingChallengeRuntime runtime)
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

        boundRuntime.OnWagerStateChanged += HandleWagerStateChanged;
        boundRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;

        ShowRoot();
        RefreshAll();

        Log("Panel shown and runtime bound.");
    }

    public void HidePanel()
    {
        UnbindRuntime();
        HideRootImmediate();
        Log("Panel hidden.");
    }

    public void RequestIncreaseLane(int laneIndex)
    {
        if (boundRuntime == null)
            return;

        boundRuntime.TryIncreaseLaneWager(laneIndex, 1);
    }

    public void RequestDecreaseLane(int laneIndex)
    {
        if (boundRuntime == null)
            return;

        boundRuntime.TryDecreaseLaneWager(laneIndex, 1);
    }

    private void HandleStartClicked()
    {
        if (boundRuntime == null)
            return;

        bool started = boundRuntime.TryStartRace();
        Log($"Start clicked | started={started}");
    }

    private void HandleWagerStateChanged(BettingChallengeRuntime runtime)
    {
        if (runtime != boundRuntime)
            return;

        RefreshAll();
    }

    private void HandleRuntimeStateChanged(BettingChallengeRuntime runtime, BettingChallengeRuntime.BettingRuntimeState newState)
    {
        if (runtime != boundRuntime)
            return;

        RefreshAll();
    }

    private void RefreshAll()
    {
        if (boundRuntime == null)
            return;

        if (snapshotAPText != null)
            snapshotAPText.text = $"Snapshot AP: {boundRuntime.SnapshotAP}";

        if (minimumBetText != null)
            minimumBetText.text = $"Minimum Bet: {boundRuntime.MinimumTotalWager}";

        if (totalBetText != null)
            totalBetText.text = $"Total Bet: {boundRuntime.TotalWager}";

        if (remainingAPText != null)
            remainingAPText.text = $"Remaining: {boundRuntime.RemainingAP}";

        bool isAwaitingBet = boundRuntime.IsAwaitingBet;
        bool showStart = isAwaitingBet;
        bool canStart = boundRuntime.CanStartRace();

        if (startButton != null)
        {
            startButton.gameObject.SetActive(showStart);
            startButton.interactable = canStart;
        }

        if (startButtonLabel != null)
            startButtonLabel.text = "START";

        for (int i = 0; i < laneControls.Length; i++)
        {
            if (laneControls[i] == null)
                continue;

            int amount = boundRuntime.GetLaneWager(i);
            laneControls[i].Refresh(amount, isAwaitingBet);
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
            boundRuntime.OnWagerStateChanged -= HandleWagerStateChanged;
            boundRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
            boundRuntime = null;
        }
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[BettingChallengePanelUI] {message}", this);
    }
}