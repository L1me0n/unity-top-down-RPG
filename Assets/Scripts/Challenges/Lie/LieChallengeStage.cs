using System.Collections;
using UnityEngine;

public class LieChallengeStage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChallengeRoomController roomController;
    [SerializeField] private LieChallengeRuntime lieRuntime;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private LieChallengePanelUI liePanelUI;
    [SerializeField] private LieForcedStageHost forcedStageHost;
    [SerializeField] private LuciferPalmMagicVisual luciferPalmMagicVisual;

    [Header("Stage Objects")]
    [SerializeField] private GameObject stageRoot;
    [SerializeField] private GameObject introChoiceStageRoot;
    [SerializeField] private Transform playerStandPoint;
    [SerializeField] private Transform stageCameraFocusPoint;

    [Header("Camera")]
    [SerializeField] private float lieZoomSize = 10.5f;

    [Header("Sneak Timing")]
    [SerializeField] private float sneakFailureDelaySeconds = 1.25f;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool stageShown;
    private Coroutine sneakFailRoutine;
    private Coroutine delayedRestoreRoutine;

    private void Awake()
    {
        if (roomController == null)
            roomController = GetComponentInParent<ChallengeRoomController>();

        if (lieRuntime == null)
            lieRuntime = GetComponentInChildren<LieChallengeRuntime>(true);

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

        if (liePanelUI == null)
            liePanelUI = FindFirstObjectByType<LieChallengePanelUI>();

        if (forcedStageHost == null)
            forcedStageHost = FindFirstObjectByType<LieForcedStageHost>();

        if (luciferPalmMagicVisual == null)
            luciferPalmMagicVisual = FindFirstObjectByType<LuciferPalmMagicVisual>();

        if (stageRoot != null)
            stageRoot.SetActive(false);

        if (introChoiceStageRoot != null)
            introChoiceStageRoot.SetActive(false);

        if (liePanelUI != null)
            liePanelUI.HidePanel();

        if (forcedStageHost != null)
            forcedStageHost.HideAll();

        if (luciferPalmMagicVisual != null)
            luciferPalmMagicVisual.HideImmediate();

        Log($"Awake complete | palmMagicFound={(luciferPalmMagicVisual != null)}");
    }

    private void OnEnable()
    {
        if (lieRuntime != null)
        {
            lieRuntime.OnRuntimeStateChanged += HandleRuntimeStateChanged;
            lieRuntime.OnRouteChosen += HandleRouteChosen;
            lieRuntime.OnForcedTrialStarted += HandleForcedTrialStarted;
        }

        if (forcedStageHost != null)
            forcedStageHost.OnForcedTrialResolved += HandleForcedTrialResolved;

        if (roomController != null)
        {
            roomController.OnChallengeRoomInitialized += HandleChallengeRoomInitialized;
            roomController.OnChallengeResolved += HandleChallengeResolved;
        }
    }

    private void OnDisable()
    {
        if (lieRuntime != null)
        {
            lieRuntime.OnRuntimeStateChanged -= HandleRuntimeStateChanged;
            lieRuntime.OnRouteChosen -= HandleRouteChosen;
            lieRuntime.OnForcedTrialStarted -= HandleForcedTrialStarted;
        }

        if (forcedStageHost != null)
            forcedStageHost.OnForcedTrialResolved -= HandleForcedTrialResolved;

        if (roomController != null)
        {
            roomController.OnChallengeRoomInitialized -= HandleChallengeRoomInitialized;
            roomController.OnChallengeResolved -= HandleChallengeResolved;
        }

        StopSneakFailRoutine();
        StopDelayedRestoreRoutine();
        HideStageImmediate();
    }

    private void HandleChallengeRoomInitialized(ChallengeRoomController controller)
    {
        if (controller == null || controller != roomController)
            return;

        if (controller.ChallengeType != ChallengeType.Lie)
            return;

        if (controller.IsChallengeCompleted || controller.IsResolved)
        {
            HideStageImmediate();
            UIInputBlocker.BlockGameplayInput = false;
            return;
        }

        SyncFromRuntimeState(forceSnap: true);
        StartDelayedRestoreSync();
    }

    private void HandleRuntimeStateChanged(LieChallengeRuntime runtime, LieChallengeState.LieRuntimeState newState)
    {
        if (runtime == null || runtime != lieRuntime)
            return;

        bool forceSnap =
            newState == LieChallengeState.LieRuntimeState.IntroChoice ||
            newState == LieChallengeState.LieRuntimeState.SneakPreparing;

        SyncFromRuntimeState(forceSnap);

        if (newState == LieChallengeState.LieRuntimeState.SneakActive && !lieRuntime.State.sneakWillSucceed)
        {
            Log("Entering failed SneakActive. Starting sneak fail routine and showing palm magic.");
            StartSneakFailRoutine();
        }
        else
        {
            StopSneakFailRoutine();
        }
    }

    private void HandleRouteChosen(LieChallengeRuntime runtime, LieChallengeState.LieRoute route)
    {
        if (runtime == null || runtime != lieRuntime)
            return;

        SyncFromRuntimeState(forceSnap: false);
    }

    private void HandleForcedTrialStarted(LieChallengeRuntime runtime, LieForcedTrialType forcedTrialType)
    {
        if (runtime == null || runtime != lieRuntime)
            return;

        if (forcedStageHost != null)
            forcedStageHost.ShowForcedTrial(forcedTrialType, lieRuntime);
    }

    private void HandleForcedTrialResolved(LieForcedStageHost _, ChallengeResult subResult)
    {
        if (lieRuntime == null || subResult == null)
            return;

        lieRuntime.ResolveForcedTrialStep(subResult);
    }

    private void HandleChallengeResolved(ChallengeRoomController controller, ChallengeResult result)
    {
        if (controller == null || controller != roomController)
            return;

        if (controller.ChallengeType != ChallengeType.Lie)
            return;

        StopSneakFailRoutine();
        StopDelayedRestoreRoutine();
        HideStageImmediate();
        UIInputBlocker.BlockGameplayInput = false;
    }

    private void StartDelayedRestoreSync()
    {
        StopDelayedRestoreRoutine();
        delayedRestoreRoutine = StartCoroutine(CoDelayedRestoreSync());
    }

    private void StopDelayedRestoreRoutine()
    {
        if (delayedRestoreRoutine == null)
            return;

        StopCoroutine(delayedRestoreRoutine);
        delayedRestoreRoutine = null;
    }

    private IEnumerator CoDelayedRestoreSync()
    {
        yield return null;
        SyncFromRuntimeState(forceSnap: true);
        delayedRestoreRoutine = null;
    }

    private void SyncFromRuntimeState(bool forceSnap)
    {
        if (lieRuntime == null)
            return;

        if (roomController != null && (roomController.IsChallengeCompleted || roomController.IsResolved))
        {
            HideStageImmediate();
            UIInputBlocker.BlockGameplayInput = false;
            return;
        }

        LieChallengeState.LieRuntimeState runtimeState = lieRuntime.State.runtimeState;

        bool isForcedTrialState =
            runtimeState == LieChallengeState.LieRuntimeState.TrialPreparing ||
            runtimeState == LieChallengeState.LieRuntimeState.TrialRunning;

        bool shouldShowLieStageRoot =
            runtimeState == LieChallengeState.LieRuntimeState.IntroChoice ||
            runtimeState == LieChallengeState.LieRuntimeState.SneakPreparing ||
            runtimeState == LieChallengeState.LieRuntimeState.SneakActive;

        if (shouldShowLieStageRoot)
            ShowLieStage(forceSnap);
        else
            HideLieStageVisualOnly();

        if (introChoiceStageRoot != null)
        {
            bool showIntroChoice =
                runtimeState == LieChallengeState.LieRuntimeState.IntroChoice ||
                runtimeState == LieChallengeState.LieRuntimeState.SneakPreparing ||
                runtimeState == LieChallengeState.LieRuntimeState.SneakActive;

            introChoiceStageRoot.SetActive(showIntroChoice);
        }

        if (forcedStageHost != null)
        {
            if (runtimeState == LieChallengeState.LieRuntimeState.TrialRunning &&
                lieRuntime.State.CurrentForcedTrial != LieForcedTrialType.None)
            {
                bool restoringExistingForcedTrial = forceSnap;
                forcedStageHost.ShowForcedTrial(
                    lieRuntime.State.CurrentForcedTrial,
                    lieRuntime,
                    forceRestart: restoringExistingForcedTrial
                );
            }
            else if (!isForcedTrialState)
            {
                forcedStageHost.HideAll();
            }
        }

        if (liePanelUI != null)
        {
            bool shouldShowLiePanel =
                runtimeState == LieChallengeState.LieRuntimeState.IntroChoice ||
                runtimeState == LieChallengeState.LieRuntimeState.SneakActive;

            if (shouldShowLiePanel)
                liePanelUI.ShowForRuntime(lieRuntime);
            else
                liePanelUI.HidePanel();
        }

        bool shouldBlockGameplay =
            runtimeState == LieChallengeState.LieRuntimeState.IntroChoice ||
            runtimeState == LieChallengeState.LieRuntimeState.SneakPreparing ||
            runtimeState == LieChallengeState.LieRuntimeState.TrialPreparing ||
            runtimeState == LieChallengeState.LieRuntimeState.TrialRunning;

        UIInputBlocker.BlockGameplayInput = shouldBlockGameplay;

        // Palm magic is now controlled directly by the fail routine.
        // If we are NOT in failed sneak-active, make sure it is hidden.
        bool shouldKeepPalmMagicVisible =
            runtimeState == LieChallengeState.LieRuntimeState.SneakActive &&
            !lieRuntime.State.sneakWillSucceed &&
            sneakFailRoutine != null;

        if (!shouldKeepPalmMagicVisible && luciferPalmMagicVisual != null)
            luciferPalmMagicVisual.Hide();

        if (runtimeState == LieChallengeState.LieRuntimeState.SneakActive)
        {
            if (cameraFollow != null)
            {
                cameraFollow.ClearTemporaryZoom();
                cameraFollow.ClearTemporaryFocusTarget();
            }
        }
        else if (shouldShowLieStageRoot)
        {
            ApplyCameraStageView();
        }
    }

    private void ShowLieStage(bool forceSnap)
    {
        if (!stageShown)
        {
            stageShown = true;

            if (stageRoot != null)
                stageRoot.SetActive(true);

            ApplyCameraStageView();
        }

        if (forceSnap)
            SnapPlayerToStandPoint();
    }

    private void HideLieStageVisualOnly()
    {
        if (!stageShown)
            return;

        stageShown = false;

        if (stageRoot != null)
            stageRoot.SetActive(false);

        if (introChoiceStageRoot != null)
            introChoiceStageRoot.SetActive(false);
    }

    public void HideStageImmediate()
    {
        stageShown = false;

        if (stageRoot != null)
            stageRoot.SetActive(false);

        if (introChoiceStageRoot != null)
            introChoiceStageRoot.SetActive(false);

        if (liePanelUI != null)
            liePanelUI.HidePanel();

        if (forcedStageHost != null)
            forcedStageHost.HideAll();

        if (luciferPalmMagicVisual != null)
            luciferPalmMagicVisual.Hide();

        if (cameraFollow != null)
        {
            cameraFollow.ClearTemporaryZoom();
            cameraFollow.ClearTemporaryFocusTarget();
        }

        ZeroPlayerVelocity();
    }

    private void StartSneakFailRoutine()
    {
        StopSneakFailRoutine();

        if (luciferPalmMagicVisual != null)
        {
            Log("Showing Lucifer palm magic explicitly from StartSneakFailRoutine.");
            luciferPalmMagicVisual.Show();
        }
        else
        {
            Log("luciferPalmMagicVisual is NULL in StartSneakFailRoutine.");
        }

        sneakFailRoutine = StartCoroutine(CoSneakFailDelay());
    }

    private void StopSneakFailRoutine()
    {
        if (sneakFailRoutine != null)
        {
            StopCoroutine(sneakFailRoutine);
            sneakFailRoutine = null;
        }

        if (luciferPalmMagicVisual != null)
            luciferPalmMagicVisual.Hide();
    }

    private IEnumerator CoSneakFailDelay()
    {
        yield return new WaitForSecondsRealtime(sneakFailureDelaySeconds);

        if (lieRuntime != null)
            lieRuntime.TriggerSneakFailureNow();

        if (luciferPalmMagicVisual != null)
            luciferPalmMagicVisual.Hide();

        sneakFailRoutine = null;
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

        cameraFollow.SetTemporaryZoom(lieZoomSize);

        if (stageCameraFocusPoint != null)
            cameraFollow.SetTemporaryFocusTarget(stageCameraFocusPoint);
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[LieChallengeStage] {message}", this);
    }
}