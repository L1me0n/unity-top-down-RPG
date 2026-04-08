using System.Collections;
using UnityEngine;

public class SlothChallengeStage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChallengeRoomController roomController;
    [SerializeField] private SlothChallengeRuntime slothRuntime;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private SlothChallengePanelUI slothPanelUI;

    [Header("Stage Objects")]
    [SerializeField] private GameObject stageRoot;
    [SerializeField] private Transform playerStandPoint;
    [SerializeField] private Transform stageCameraFocusPoint;

    [Header("Camera")]
    [SerializeField] private float slothZoomSize = 9.5f;

    [Header("Ending Sequence")]
    [SerializeField] private float successShowSeconds = 1.4f;
    [SerializeField] private float failShowSeconds = 1.2f;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool stageShown;
    private Coroutine endSequenceRoutine;

    private void Awake()
    {
        if (roomController == null)
            roomController = GetComponent<ChallengeRoomController>();

        if (slothRuntime == null)
            slothRuntime = GetComponentInChildren<SlothChallengeRuntime>(true);

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

        if (slothPanelUI == null)
            slothPanelUI = FindFirstObjectByType<SlothChallengePanelUI>();

        if (stageRoot != null)
            stageRoot.SetActive(false);

        if (slothPanelUI != null)
            slothPanelUI.HidePanel();
    }

    private void OnEnable()
    {
        if (slothRuntime != null)
        {
            slothRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;
            slothRuntime.OnTimingStopped += HandleTimingStopped;
        }

        if (roomController != null)
        {
            roomController.OnChallengeRoomInitialized += HandleChallengeRoomInitialized;
            roomController.OnChallengeResolved += HandleChallengeResolved;
        }
    }

    private void OnDisable()
    {
        if (slothRuntime != null)
        {
            slothRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
            slothRuntime.OnTimingStopped -= HandleTimingStopped;
        }

        if (roomController != null)
        {
            roomController.OnChallengeRoomInitialized -= HandleChallengeRoomInitialized;
            roomController.OnChallengeResolved -= HandleChallengeResolved;
        }

        StopEndSequence();
        HideStageImmediate();
    }

    private void HandleChallengeRoomInitialized(ChallengeRoomController controller)
    {
        if (controller == null || controller != roomController)
            return;

        if (controller.ChallengeType != ChallengeType.Sloth)
            return;

        StopEndSequence();

        if (controller.IsChallengeCompleted || controller.IsResolved)
        {
            HideStageImmediate();
            UIInputBlocker.BlockGameplayInput = false;

            Log("Challenge room initialized in completed/resolved state. Force-hid Sloth stage.");
            return;
        }

        SyncFromRuntimeState();
    }

    private void HandleRuntimeStateChanged(
        SlothChallengeRuntime runtime,
        SlothChallengeRuntime.LocalFlowState newState)
    {
        if (runtime == null || runtime != slothRuntime)
            return;

        SyncFromRuntimeState();
    }

    private void HandleTimingStopped(SlothChallengeRuntime runtime, bool success)
    {
        if (runtime == null || runtime != slothRuntime)
            return;

        StopEndSequence();
        endSequenceRoutine = StartCoroutine(CoShowStopResult(success));
    }

    private void HandleChallengeResolved(ChallengeRoomController controller, ChallengeResult result)
    {
        if (controller == null || controller != roomController || result == null)
            return;

        if (controller.ChallengeType != ChallengeType.Sloth)
            return;

        StopEndSequence();
        endSequenceRoutine = StartCoroutine(CoFinishResolvedRoom(result));
    }

    private IEnumerator CoShowStopResult(bool success)
    {
        if (slothPanelUI != null)
            slothPanelUI.ShowForRuntime(slothRuntime);

        yield return new WaitForSecondsRealtime(success ? successShowSeconds : failShowSeconds);

        if (!success)
        {
            HideStageImmediate();
            UIInputBlocker.BlockGameplayInput = false;

            if (slothRuntime != null)
                slothRuntime.ExecuteFailureDeath();
        }

        endSequenceRoutine = null;
    }

    private IEnumerator CoFinishResolvedRoom(ChallengeResult result)
    {
        if (result.IsSuccess)
        {
            if (slothPanelUI != null)
                slothPanelUI.ShowForRuntime(slothRuntime);

            yield return new WaitForSecondsRealtime(successShowSeconds);
        }

        HideStageImmediate();
        UIInputBlocker.BlockGameplayInput = false;
        endSequenceRoutine = null;
    }

    private void SyncFromRuntimeState()
    {
        if (slothRuntime == null)
            return;

        if (roomController != null && (roomController.IsChallengeCompleted || roomController.IsResolved))
        {
            HideStageImmediate();
            UIInputBlocker.BlockGameplayInput = false;
            return;
        }

        bool shouldShowStage =
            slothRuntime.FlowState == SlothChallengeRuntime.LocalFlowState.AwaitingStart ||
            slothRuntime.FlowState == SlothChallengeRuntime.LocalFlowState.Running;

        if (shouldShowStage)
            ShowStage();
        else
            HideStageImmediate();

        if (slothPanelUI != null)
        {
            bool shouldShowPanel =
                slothRuntime.FlowState == SlothChallengeRuntime.LocalFlowState.AwaitingStart ||
                slothRuntime.FlowState == SlothChallengeRuntime.LocalFlowState.Running;

            if (shouldShowPanel)
                slothPanelUI.ShowForRuntime(slothRuntime);
            else
                slothPanelUI.HidePanel();
        }
    }

    private void ShowStage()
    {
        if (stageShown)
            return;

        stageShown = true;

        if (stageRoot != null)
            stageRoot.SetActive(true);

        SnapPlayerToStandPoint();
        ApplyCameraStageView();
        UIInputBlocker.BlockGameplayInput = true;

        Log("Sloth stage shown.");
    }

    public void HideStageImmediate()
    {
        stageShown = false;

        if (stageRoot != null)
            stageRoot.SetActive(false);

        if (slothPanelUI != null)
            slothPanelUI.HidePanel();

        if (cameraFollow != null)
        {
            cameraFollow.ClearTemporaryZoom();
            cameraFollow.ClearTemporaryFocusTarget();
        }

        ZeroPlayerVelocity();
        Log("Sloth stage hidden.");
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

        cameraFollow.SetTemporaryZoom(slothZoomSize);

        if (stageCameraFocusPoint != null)
            cameraFollow.SetTemporaryFocusTarget(stageCameraFocusPoint);
    }

    private void StopEndSequence()
    {
        if (endSequenceRoutine == null)
            return;

        StopCoroutine(endSequenceRoutine);
        endSequenceRoutine = null;
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[SlothChallengeStage] {message}", this);
    }
}