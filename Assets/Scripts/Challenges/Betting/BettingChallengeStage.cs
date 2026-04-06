using System.Collections.Generic;
using UnityEngine;

public class BettingChallengeStage : MonoBehaviour
{
    [System.Serializable]
    public class LaneVisual
    {
        public Transform token;
        public Transform tokenStartPoint;
        public Transform tokenFinishPoint;
        public SpriteRenderer[] highlightRenderers;
    }

    [Header("References")]
    [SerializeField] private ChallengeRoomController roomController;
    [SerializeField] private BettingChallengeRuntime bettingRuntime;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private BettingChallengePanelUI bettingPanelUI;

    [Header("Stage Objects")]
    [SerializeField] private GameObject stageRoot;
    [SerializeField] private Transform playerStandPoint;
    [SerializeField] private Transform stageCameraFocusPoint;
    [SerializeField] private List<LaneVisual> laneVisuals = new List<LaneVisual>();

    [Header("Camera")]
    [SerializeField] private float bettingZoomSize = 9.5f;

    [Header("Highlight")]
    [SerializeField] private Color winnerHighlightColor = Color.yellow;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool stageShown;
    private readonly Dictionary<SpriteRenderer, Color> originalRendererColors = new Dictionary<SpriteRenderer, Color>();

    private void Awake()
    {
        if (roomController == null)
            roomController = GetComponent<ChallengeRoomController>();

        if (bettingRuntime == null)
            bettingRuntime = GetComponent<BettingChallengeRuntime>();

        if (cameraFollow == null)
            cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (playerTransform == null)
        {
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
            if (movement != null)
                playerTransform = movement.transform;
        }

        if (playerRb == null && playerTransform != null)
            playerRb = playerTransform.GetComponent<Rigidbody2D>();

        if (bettingPanelUI == null)
            bettingPanelUI = FindFirstObjectByType<BettingChallengePanelUI>();

        CacheOriginalHighlightColors();

        if (stageRoot != null)
            stageRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (bettingRuntime != null)
            bettingRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;

        if (roomController != null)
            roomController.OnChallengeResolved += HandleChallengeResolved;
    }

    private void OnDisable()
    {
        if (bettingRuntime != null)
            bettingRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;

        if (roomController != null)
            roomController.OnChallengeResolved -= HandleChallengeResolved;
    }

    private void HandleRuntimeStateChanged(BettingChallengeRuntime runtime, BettingChallengeRuntime.BettingRuntimeState newState)
    {
        if (runtime == null)
            return;

        bool shouldShowStage =
            newState == BettingChallengeRuntime.BettingRuntimeState.AwaitingBet ||
            newState == BettingChallengeRuntime.BettingRuntimeState.RaceStarting ||
            newState == BettingChallengeRuntime.BettingRuntimeState.RaceRunning ||
            newState == BettingChallengeRuntime.BettingRuntimeState.RaceFinishedShowingWinner;

        if (shouldShowStage)
            ShowStage();
        else
            HideStage();

        if (bettingPanelUI != null)
        {
            if (newState == BettingChallengeRuntime.BettingRuntimeState.AwaitingBet)
                bettingPanelUI.ShowForRuntime(runtime);
            else
                bettingPanelUI.HidePanel();
        }
    }

    private void HandleChallengeResolved(ChallengeRoomController controller, ChallengeResult result)
    {
        if (controller == null || controller != roomController)
            return;

        HideStage();

        if (bettingPanelUI != null)
            bettingPanelUI.HidePanel();
    }

    private void ShowStage()
    {
        if (stageShown)
            return;

        stageShown = true;

        if (stageRoot != null)
            stageRoot.SetActive(true);

        ResetLaneTokensToStart();
        ResetWinnerHighlight();
        SnapPlayerToStandPoint();
        ApplyCameraStageView();

        Log("Betting stage shown.");
    }

    private void HideStage()
    {
        if (!stageShown)
            return;

        stageShown = false;

        if (stageRoot != null)
            stageRoot.SetActive(false);

        if (cameraFollow != null)
        {
            cameraFollow.ClearTemporaryZoom();
            cameraFollow.ClearTemporaryFocusTarget();
        }

        ZeroPlayerVelocity();

        Log("Betting stage hidden.");
    }

    private void SnapPlayerToStandPoint()
    {
        if (playerTransform == null || playerStandPoint == null)
            return;

        ZeroPlayerVelocity();
        playerTransform.position = playerStandPoint.position;
        ZeroPlayerVelocity();
    }

    private void ZeroPlayerVelocity()
    {
        if (playerRb == null)
            return;

        playerRb.linearVelocity = Vector2.zero;
        playerRb.angularVelocity = 0f;
    }

    private void ApplyCameraStageView()
    {
        if (cameraFollow == null)
            return;

        cameraFollow.SetTemporaryZoom(bettingZoomSize);

        if (stageCameraFocusPoint != null)
            cameraFollow.SetTemporaryFocusTarget(stageCameraFocusPoint);
    }

    public void ResetLaneTokensToStart()
    {
        for (int i = 0; i < laneVisuals.Count; i++)
        {
            LaneVisual lane = laneVisuals[i];
            if (lane == null)
                continue;

            if (lane.token != null && lane.tokenStartPoint != null)
                lane.token.position = lane.tokenStartPoint.position;
        }
    }

    public void SetLaneTokenProgress(int laneIndex, float normalizedProgress)
    {
        if (laneIndex < 0 || laneIndex >= laneVisuals.Count)
            return;

        LaneVisual lane = laneVisuals[laneIndex];
        if (lane == null)
            return;

        if (lane.token == null || lane.tokenStartPoint == null || lane.tokenFinishPoint == null)
            return;

        normalizedProgress = Mathf.Clamp01(normalizedProgress);
        lane.token.position = Vector3.Lerp(
            lane.tokenStartPoint.position,
            lane.tokenFinishPoint.position,
            normalizedProgress
        );
    }

    public void HighlightWinningLane(int laneIndex)
    {
        ResetWinnerHighlight();

        if (laneIndex < 0 || laneIndex >= laneVisuals.Count)
            return;

        LaneVisual lane = laneVisuals[laneIndex];
        if (lane == null || lane.highlightRenderers == null)
            return;

        for (int i = 0; i < lane.highlightRenderers.Length; i++)
        {
            SpriteRenderer sr = lane.highlightRenderers[i];
            if (sr == null)
                continue;

            sr.color = winnerHighlightColor;
        }
    }

    public void ResetWinnerHighlight()
    {
        for (int i = 0; i < laneVisuals.Count; i++)
        {
            LaneVisual lane = laneVisuals[i];
            if (lane == null || lane.highlightRenderers == null)
                continue;

            for (int j = 0; j < lane.highlightRenderers.Length; j++)
            {
                SpriteRenderer sr = lane.highlightRenderers[j];
                if (sr == null)
                    continue;

                if (originalRendererColors.TryGetValue(sr, out Color original))
                    sr.color = original;
            }
        }
    }

    private void CacheOriginalHighlightColors()
    {
        originalRendererColors.Clear();

        for (int i = 0; i < laneVisuals.Count; i++)
        {
            LaneVisual lane = laneVisuals[i];
            if (lane == null || lane.highlightRenderers == null)
                continue;

            for (int j = 0; j < lane.highlightRenderers.Length; j++)
            {
                SpriteRenderer sr = lane.highlightRenderers[j];
                if (sr == null)
                    continue;

                if (!originalRendererColors.ContainsKey(sr))
                    originalRendererColors.Add(sr, sr.color);
            }
        }
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[BettingChallengeStage] {message}", this);
    }
}