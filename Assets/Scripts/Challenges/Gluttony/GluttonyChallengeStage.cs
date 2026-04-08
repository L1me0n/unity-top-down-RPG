using System.Collections;
using UnityEngine;

public class GluttonyChallengeStage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChallengeRoomController roomController;
    [SerializeField] private GluttonyChallengeRuntime gluttonyRuntime;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private GluttonyChallengePanelUI gluttonyPanelUI;
    [SerializeField] private GluttonyMonsterVisual monsterVisual;
    [SerializeField] private GluttonyFeedbackPopup feedbackPopup;

    [Header("Stage Objects")]
    [SerializeField] private GameObject stageRoot;
    [SerializeField] private Transform playerStandPoint;
    [SerializeField] private Transform stageCameraFocusPoint;

    [Header("Camera")]
    [SerializeField] private float gluttonyZoomSize = 9.5f;

    [Header("Ending Sequence")]
    [SerializeField] private float successShowSeconds = 1.8f;
    [SerializeField] private float failShowSeconds = 2.0f;
    [SerializeField] private float guardedFailShowSeconds = 1.6f;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool stageShown;
    private Coroutine endSequenceRoutine;

    private void Awake()
    {
        if (roomController == null)
            roomController = GetComponent<ChallengeRoomController>();

        if (gluttonyRuntime == null)
            gluttonyRuntime = GetComponent<GluttonyChallengeRuntime>();

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

        if (gluttonyPanelUI == null)
            gluttonyPanelUI = FindFirstObjectByType<GluttonyChallengePanelUI>();

        if (feedbackPopup == null)
            feedbackPopup = FindFirstObjectByType<GluttonyFeedbackPopup>();

        if (stageRoot != null)
            stageRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (gluttonyRuntime != null)
        {
            gluttonyRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;
            gluttonyRuntime.OnTurnResolved += HandleTurnResolved;
        }

        if (roomController != null)
        {
            roomController.OnChallengeRoomInitialized += HandleChallengeRoomInitialized;
            roomController.OnChallengeResolved += HandleChallengeResolved;
        }
    }

    private void OnDisable()
    {
        if (gluttonyRuntime != null)
        {
            gluttonyRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
            gluttonyRuntime.OnTurnResolved -= HandleTurnResolved;
        }

        if (roomController != null)
        {
            roomController.OnChallengeRoomInitialized -= HandleChallengeRoomInitialized;
            roomController.OnChallengeResolved -= HandleChallengeResolved;
        }
    }

    private void HandleChallengeRoomInitialized(ChallengeRoomController controller)
    {
        if (controller == null || controller != roomController)
            return;

        if (controller.ChallengeType != ChallengeType.Gluttony)
            return;

        StopEndSequence();

        if (monsterVisual != null)
            monsterVisual.ResetVisualInstant();

        SyncFromRuntimeState();
    }

    private void HandleRuntimeStateChanged(
        GluttonyChallengeRuntime runtime,
        GluttonyChallengeRuntime.GluttonyRuntimeState newState)
    {
        if (runtime == null || runtime != gluttonyRuntime)
            return;

        SyncFromRuntimeState();
    }

    private void HandleTurnResolved(GluttonyChallengeRuntime runtime, GluttonyTurnResult result)
    {
        if (runtime == null || runtime != gluttonyRuntime || result == null)
            return;

        if (monsterVisual != null)
            monsterVisual.SetFullnessVisual(runtime.FullnessCurrent, runtime.FullnessTarget);

        switch (result.outcome)
        {
            case GluttonyTurnResult.TurnOutcome.ExactSuccess:
                if (monsterVisual != null)
                    monsterVisual.PlaySuccessPulse();
                break;

            case GluttonyTurnResult.TurnOutcome.OverfedFail:
                if (monsterVisual != null)
                    monsterVisual.PlayExplode();
                break;
        }
    }

    private void HandleChallengeResolved(ChallengeRoomController controller, ChallengeResult result)
    {
        if (controller == null || controller != roomController || result == null)
            return;

        if (controller.ChallengeType != ChallengeType.Gluttony)
            return;

        StopEndSequence();
        endSequenceRoutine = StartCoroutine(CoPlayEndSequence(result));
    }

    private IEnumerator CoPlayEndSequence(ChallengeResult result)
    {
        UIInputBlocker.BlockGameplayInput = true;

        if (gluttonyPanelUI != null)
            gluttonyPanelUI.HidePanel();

        string popupMessage;
        float holdSeconds;

        if (result.IsSuccess)
        {
            popupMessage = $"GLUTTONY WON  HP +{gluttonyRuntime.SuccessBonusMaxHP}";
            holdSeconds = successShowSeconds;

            if (feedbackPopup != null)
                feedbackPopup.ShowSuccessMessage(popupMessage);
        }
        else if (result.summaryText != null && result.summaryText.Contains("preserve 1 HP"))
        {
            popupMessage = "GLUTTONY STOPPED  1 HP SAVED";
            holdSeconds = guardedFailShowSeconds;

            if (feedbackPopup != null)
                feedbackPopup.ShowFailMessage(popupMessage);
        }
        else
        {
            popupMessage = "GLUTTONY FAILED. 50% AP LOST";
            holdSeconds = failShowSeconds;

            if (feedbackPopup != null)
                feedbackPopup.ShowFailMessage(popupMessage);
        }

        yield return new WaitForSecondsRealtime(holdSeconds);

        HideStageImmediate();

        UIInputBlocker.BlockGameplayInput = false;
        endSequenceRoutine = null;
    }

    private void SyncFromRuntimeState()
    {
        if (gluttonyRuntime == null)
            return;

        bool shouldShowStage =
            gluttonyRuntime.RuntimeState == GluttonyChallengeRuntime.GluttonyRuntimeState.AwaitingInput ||
            gluttonyRuntime.RuntimeState == GluttonyChallengeRuntime.GluttonyRuntimeState.ResolvingTurn ||
            gluttonyRuntime.RuntimeState == GluttonyChallengeRuntime.GluttonyRuntimeState.Resolved;

        if (shouldShowStage)
            ShowStage();
        else
            HideStageImmediate();

        if (gluttonyPanelUI != null)
        {
            bool shouldShowPanel =
                gluttonyRuntime.RuntimeState == GluttonyChallengeRuntime.GluttonyRuntimeState.AwaitingInput ||
                gluttonyRuntime.RuntimeState == GluttonyChallengeRuntime.GluttonyRuntimeState.ResolvingTurn;

            if (shouldShowPanel)
                gluttonyPanelUI.ShowForRuntime(gluttonyRuntime);
            else
                gluttonyPanelUI.HidePanel();
        }
    }

    private void ShowStage()
    {
        if (stageShown)
            return;

        stageShown = true;

        if (stageRoot != null)
            stageRoot.SetActive(true);

        if (monsterVisual != null)
        {
            monsterVisual.ResetVisualInstant();
            monsterVisual.SetFullnessVisual(gluttonyRuntime.FullnessCurrent, gluttonyRuntime.FullnessTarget);
        }

        SnapPlayerToStandPoint();
        ApplyCameraStageView();

        Log("Gluttony stage shown.");
    }

    public void HideStageImmediate()
    {
        if (!stageShown && stageRoot != null && !stageRoot.activeSelf)
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

        Log("Gluttony stage hidden.");
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

        cameraFollow.SetTemporaryZoom(gluttonyZoomSize);

        if (stageCameraFocusPoint != null)
            cameraFollow.SetTemporaryFocusTarget(stageCameraFocusPoint);
    }

    private void StopEndSequence()
    {
        if (endSequenceRoutine != null)
        {
            StopCoroutine(endSequenceRoutine);
            endSequenceRoutine = null;
        }
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[GluttonyChallengeStage] {message}", this);
    }
}