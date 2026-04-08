using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlothChallengePanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rulesText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text resultText;

    [Header("Bar UI")]
    [SerializeField] private RectTransform barArea;
    [SerializeField] private RectTransform marker;
    [SerializeField] private RectTransform successZone;

    [Header("Buttons")]
    [SerializeField] private Button stopButton;

    [Header("Button Labels")]
    [SerializeField] private TMP_Text stopButtonLabel;

    [Header("Behavior")]
    [SerializeField] private bool hideOnAwake = true;
    [SerializeField] private bool clearResultOnShow = true;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private SlothChallengeRuntime boundRuntime;

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    private void Awake()
    {

        if (stopButton != null)
            stopButton.onClick.AddListener(HandleStopClicked);

        if (hideOnAwake)
            HideRootImmediate();
    }

    private void OnDestroy()
    {
        UnbindRuntime();

        if (stopButton != null)
            stopButton.onClick.RemoveListener(HandleStopClicked);
    }

    public void ShowForRuntime(SlothChallengeRuntime runtime)
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

        boundRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;
        boundRuntime.OnStateChanged += HandleRuntimeStateOrDataChanged;
        boundRuntime.OnTimingStopped += HandleTimingStopped;

        ShowRoot();

        if (clearResultOnShow && resultText != null)
            resultText.text = "";

        RefreshAll();
        Log("Panel shown and runtime bound.");
    }

    public void HidePanel()
    {
        UnbindRuntime();
        HideRootImmediate();
        Log("Panel hidden.");
    }

    private void HandleStopClicked()
    {
        if (boundRuntime == null)
            return;

        bool stopped = boundRuntime.TryRequestStop();
        Log($"Stop clicked | stopped={stopped}");
    }

    private void HandleRuntimeStateChanged(SlothChallengeRuntime runtime, SlothChallengeRuntime.LocalFlowState newState)
    {
        if (runtime != boundRuntime)
            return;

        RefreshAll();
    }

    private void HandleRuntimeStateOrDataChanged(SlothChallengeRuntime runtime)
    {
        if (runtime != boundRuntime)
            return;

        RefreshAll();
    }

    private void HandleTimingStopped(SlothChallengeRuntime runtime, bool success)
    {
        if (runtime != boundRuntime)
            return;

        if (resultText != null)
        {
            resultText.text = success
                ? "SUCCESS! DP x1.5 until next Challenge room."
                : "FAILED!";
        }

        RefreshAll();
    }

    private void RefreshAll()
    {
        if (boundRuntime == null)
            return;

        if (titleText != null)
            titleText.text = "SLOTH";

        if (rulesText != null)
            rulesText.text = "Stop the marker inside the safe zone.\nMISS = DEATH\nSUCCESS = DP x1.5";

        if (stateText != null)
            stateText.text =
                $"State: {boundRuntime.FlowState}\n" +
                $"Marker: {boundRuntime.MarkerNormalized:0.000}\n" +
                $"Zone: {boundRuntime.SuccessZoneMin:0.000} - {boundRuntime.SuccessZoneMax:0.000}";

        if (stopButton != null)
            stopButton.interactable = boundRuntime.CanStopRun;

        if (stopButtonLabel != null)
            stopButtonLabel.text = boundRuntime.IsResolved ? "DONE" : "STOP";

        RefreshBarVisuals();
    }

    private void RefreshBarVisuals()
    {
        if (barArea == null)
            return;

        float width = barArea.rect.width;
        if (width <= 0.001f)
            return;

        if (successZone != null)
        {
            float zoneWidth = width * boundRuntime.SuccessZoneWidth;
            float zoneCenterX = width * boundRuntime.SuccessZoneCenter;

            successZone.anchorMin = new Vector2(0f, successZone.anchorMin.y);
            successZone.anchorMax = new Vector2(0f, successZone.anchorMax.y);
            successZone.pivot = new Vector2(0.5f, successZone.pivot.y);
            successZone.sizeDelta = new Vector2(zoneWidth, successZone.sizeDelta.y);
            successZone.anchoredPosition = new Vector2(zoneCenterX, successZone.anchoredPosition.y);
        }

        if (marker != null)
        {
            float markerX = width * boundRuntime.MarkerNormalized;

            marker.anchorMin = new Vector2(0f, marker.anchorMin.y);
            marker.anchorMax = new Vector2(0f, marker.anchorMax.y);
            marker.pivot = new Vector2(0.5f, marker.pivot.y);
            marker.anchoredPosition = new Vector2(markerX, marker.anchoredPosition.y);
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
        if (boundRuntime == null)
            return;

        boundRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
        boundRuntime.OnStateChanged -= HandleRuntimeStateOrDataChanged;
        boundRuntime.OnTimingStopped -= HandleTimingStopped;
        boundRuntime = null;
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[SlothChallengePanelUI] {message}", this);
    }
}