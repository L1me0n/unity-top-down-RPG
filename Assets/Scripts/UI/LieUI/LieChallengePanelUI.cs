using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LieChallengePanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text routeResultText;

    [Header("Buttons")]
    [SerializeField] private Button sneakPastButton;
    [SerializeField] private Button acceptTrialsButton;
    [SerializeField] private Button debugResolveSneakSuccessButton;
    [SerializeField] private Button debugTriggerSneakFailButton;

    [Header("Button Labels")]
    [SerializeField] private TMP_Text sneakPastButtonLabel;
    [SerializeField] private TMP_Text acceptTrialsButtonLabel;

    [Header("Behavior")]
    [SerializeField] private bool hideOnAwake = true;
    [SerializeField] private bool clearResultOnShow = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private LieChallengeRuntime boundRuntime;

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    private void Awake()
    {
        if (sneakPastButton != null)
            sneakPastButton.onClick.AddListener(HandleSneakPastClicked);

        if (acceptTrialsButton != null)
            acceptTrialsButton.onClick.AddListener(HandleAcceptTrialsClicked);

        if (debugResolveSneakSuccessButton != null)
            debugResolveSneakSuccessButton.onClick.AddListener(HandleDebugResolveSneakSuccessClicked);

        if (debugTriggerSneakFailButton != null)
            debugTriggerSneakFailButton.onClick.AddListener(HandleDebugTriggerSneakFailClicked);

        if (hideOnAwake)
            HideRootImmediate();
    }

    private void OnDestroy()
    {
        UnbindRuntime();

        if (sneakPastButton != null)
            sneakPastButton.onClick.RemoveListener(HandleSneakPastClicked);

        if (acceptTrialsButton != null)
            acceptTrialsButton.onClick.RemoveListener(HandleAcceptTrialsClicked);

        if (debugResolveSneakSuccessButton != null)
            debugResolveSneakSuccessButton.onClick.RemoveListener(HandleDebugResolveSneakSuccessClicked);

        if (debugTriggerSneakFailButton != null)
            debugTriggerSneakFailButton.onClick.RemoveListener(HandleDebugTriggerSneakFailClicked);
    }

    public void ShowForRuntime(LieChallengeRuntime runtime)
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

        boundRuntime.OnStateChanged += HandleRuntimeChanged;
        boundRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;
        boundRuntime.OnRouteChosen += HandleRouteChosen;
        boundRuntime.OnSneakOutcomeRolled += HandleSneakOutcomeRolled;
        boundRuntime.OnTrialSequencePrepared += HandleTrialSequencePrepared;
        boundRuntime.OnRouteStatusTextChanged += HandleRouteStatusTextChanged;

        ShowRoot();

        if (clearResultOnShow && routeResultText != null)
            routeResultText.text = "";

        RefreshAll();
        Log("Lie panel shown and bound.");
    }

    public void HidePanel()
    {
        UnbindRuntime();
        HideRootImmediate();
        Log("Lie panel hidden.");
    }

    private void HandleSneakPastClicked()
    {
        if (boundRuntime == null)
            return;

        bool success = boundRuntime.ChooseSneakPast();
        Log($"Sneak Past clicked | accepted={success}");
    }

    private void HandleAcceptTrialsClicked()
    {
        if (boundRuntime == null)
            return;

        bool success = boundRuntime.ChooseAcceptTrials();
        Log($"Accept Trials clicked | accepted={success}");
    }

    private void HandleDebugResolveSneakSuccessClicked()
    {
        if (boundRuntime == null)
            return;

        boundRuntime.ResolveSneakSuccessNow();
    }

    private void HandleDebugTriggerSneakFailClicked()
    {
        if (boundRuntime == null)
            return;

        boundRuntime.TriggerSneakFailureNow();
    }

    private void HandleRuntimeChanged(LieChallengeRuntime runtime)
    {
        if (runtime != boundRuntime)
            return;

        RefreshAll();
    }

    private void HandleRuntimeStateChanged(LieChallengeRuntime runtime, LieChallengeState.LieRuntimeState _)
    {
        if (runtime != boundRuntime)
            return;

        RefreshAll();
    }

    private void HandleRouteChosen(LieChallengeRuntime runtime, LieChallengeState.LieRoute route)
    {
        if (runtime != boundRuntime)
            return;

        if (routeResultText == null)
            return;

        switch (route)
        {
            case LieChallengeState.LieRoute.SneakPast:
                routeResultText.text = "You chose to slip through the lie.";
                break;

            case LieChallengeState.LieRoute.AcceptTrials:
                routeResultText.text = "You chose Lucifer's trials.";
                break;

            default:
                routeResultText.text = "";
                break;
        }

        RefreshAll();
    }

    private void HandleSneakOutcomeRolled(LieChallengeRuntime runtime, bool willSucceed)
    {
        if (runtime != boundRuntime)
            return;

        if (routeResultText != null)
            routeResultText.text = willSucceed
                ? "Sneak path rolled as SUCCESS."
                : "Sneak path rolled as FAILURE.";

        RefreshAll();
    }

    private void HandleTrialSequencePrepared(LieChallengeRuntime runtime, System.Collections.Generic.IReadOnlyList<LieForcedTrialType> _)
    {
        if (runtime != boundRuntime)
            return;

        if (routeResultText != null)
            routeResultText.text = $"Trial chain: {runtime.ForcedTrialSequenceSummary}";

        RefreshAll();
    }

    private void HandleRouteStatusTextChanged(LieChallengeRuntime runtime, string text)
    {
        if (runtime != boundRuntime)
            return;

        if (routeResultText != null)
            routeResultText.text = text;

        RefreshAll();
    }

    private void RefreshAll()
    {
        if (boundRuntime == null)
            return;

        if (titleText != null)
            titleText.text = "LIE";

        if (promptText != null)
            promptText.text =
                "Lucifer rules these lands.\n" +
                "You may try to pass unseen, or accept what he places before you.";

        if (infoText != null)
        {
            float chancePercent = boundRuntime.SneakSuccessChance01 * 100f;

            infoText.text =
                $"Sneak base chance: 40%\n" +
                $"Hellhound/Luck points: {boundRuntime.HellhoundLuckPoints:0.#}\n" +
                $"Sneak success now: {chancePercent:0.0}%\n" +
                $"Trials forced: 1-3";
        }

        if (stateText != null)
        {
            stateText.text =
                $"State: {boundRuntime.State.runtimeState}\n" +
                $"Route: {boundRuntime.State.chosenRoute}\n" +
                $"Trials: {boundRuntime.ForcedTrialSequenceSummary}";
        }

        bool canChoose = boundRuntime.IsAwaitingChoice && !boundRuntime.HasChosenRoute;

        if (sneakPastButton != null)
            sneakPastButton.interactable = canChoose;

        if (acceptTrialsButton != null)
            acceptTrialsButton.interactable = canChoose;

        if (sneakPastButtonLabel != null)
            sneakPastButtonLabel.text = canChoose ? "SNEAK PAST" : "LOCKED";

        if (acceptTrialsButtonLabel != null)
            acceptTrialsButtonLabel.text = canChoose ? "ACCEPT TRIALS" : "LOCKED";

        bool showSneakDebugButtons =
            boundRuntime.IsSneakRoute &&
            boundRuntime.State.runtimeState == LieChallengeState.LieRuntimeState.SneakActive;

        if (debugResolveSneakSuccessButton != null)
            debugResolveSneakSuccessButton.gameObject.SetActive(showSneakDebugButtons && boundRuntime.State.sneakWillSucceed);

        if (debugTriggerSneakFailButton != null)
            debugTriggerSneakFailButton.gameObject.SetActive(showSneakDebugButtons && !boundRuntime.State.sneakWillSucceed);
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
        if (boundRuntime == null)
            return;

        boundRuntime.OnStateChanged -= HandleRuntimeChanged;
        boundRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
        boundRuntime.OnRouteChosen -= HandleRouteChosen;
        boundRuntime.OnSneakOutcomeRolled -= HandleSneakOutcomeRolled;
        boundRuntime.OnTrialSequencePrepared -= HandleTrialSequencePrepared;
        boundRuntime.OnRouteStatusTextChanged -= HandleRouteStatusTextChanged;

        boundRuntime = null;
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[LieChallengePanelUI] {message}", this);
    }
}