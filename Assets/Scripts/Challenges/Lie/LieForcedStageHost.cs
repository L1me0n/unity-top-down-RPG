using UnityEngine;

public class LieForcedStageHost : MonoBehaviour
{
    [Header("Betting")]
    [SerializeField] private GameObject bettingStageRoot;
    [SerializeField] private GameObject bettingPanelRoot;
    [SerializeField] private MonoBehaviour bettingDriverBehaviour;

    [Header("Gluttony")]
    [SerializeField] private GameObject gluttonyStageRoot;
    [SerializeField] private GameObject gluttonyPanelRoot;
    [SerializeField] private MonoBehaviour gluttonyDriverBehaviour;

    [Header("Sloth")]
    [SerializeField] private GameObject slothStageRoot;
    [SerializeField] private GameObject slothPanelRoot;
    [SerializeField] private MonoBehaviour slothDriverBehaviour;

    [Header("Debug")]
    [SerializeField] private bool log = false;

    private ILieForcedTrialDriver bettingDriver;
    private ILieForcedTrialDriver gluttonyDriver;
    private ILieForcedTrialDriver slothDriver;

    private ILieForcedTrialDriver activeDriver;
    private LieForcedTrialType activeTrialType = LieForcedTrialType.None;

    public System.Action<LieForcedStageHost, ChallengeResult> OnForcedTrialResolved;

    public LieForcedTrialType ActiveTrialType => activeTrialType;
    public bool HasActiveDriver => activeDriver != null;

    private void Awake()
    {
        bettingDriver = bettingDriverBehaviour as ILieForcedTrialDriver;
        gluttonyDriver = gluttonyDriverBehaviour as ILieForcedTrialDriver;
        slothDriver = slothDriverBehaviour as ILieForcedTrialDriver;

        HideAll();
    }

    public void ShowForcedTrial(LieForcedTrialType forcedTrialType, LieChallengeRuntime lieRuntime, bool forceRestart = false)
    {
        if (forcedTrialType == LieForcedTrialType.None)
            return;

        if (!forceRestart && activeTrialType == forcedTrialType && activeDriver != null)
        {
            Log($"Forced stage host already showing {forcedTrialType}, skipping restart.");
            return;
        }

        HideAll();

        activeTrialType = forcedTrialType;

        switch (forcedTrialType)
        {
            case LieForcedTrialType.Betting:
                SetPairActive(bettingStageRoot, bettingPanelRoot, true);
                BeginDriver(bettingDriver, lieRuntime, forcedTrialType);
                break;

            case LieForcedTrialType.Gluttony:
                SetPairActive(gluttonyStageRoot, gluttonyPanelRoot, true);
                BeginDriver(gluttonyDriver, lieRuntime, forcedTrialType);
                break;

            case LieForcedTrialType.Sloth:
                SetPairActive(slothStageRoot, slothPanelRoot, true);
                BeginDriver(slothDriver, lieRuntime, forcedTrialType);
                break;
        }

        Log($"Forced stage host switched to {forcedTrialType} | forceRestart={forceRestart}");
    }

    public void HideAll()
    {
        if (activeDriver != null)
        {
            activeDriver.OnForcedTrialResolved -= HandleForcedTrialResolved;
            activeDriver.CancelForcedTrial();
            activeDriver = null;
        }

        activeTrialType = LieForcedTrialType.None;

        SetPairActive(bettingStageRoot, bettingPanelRoot, false);
        SetPairActive(gluttonyStageRoot, gluttonyPanelRoot, false);
        SetPairActive(slothStageRoot, slothPanelRoot, false);
    }

    private void BeginDriver(ILieForcedTrialDriver driver, LieChallengeRuntime lieRuntime, LieForcedTrialType forcedTrialType)
    {
        if (driver == null)
        {
            Log($"No driver assigned for {forcedTrialType}. Roots switched only.");
            return;
        }

        activeDriver = driver;
        activeDriver.OnForcedTrialResolved += HandleForcedTrialResolved;
        activeDriver.BeginForcedTrial(lieRuntime);

        Log($"Forced stage host began driver for {forcedTrialType}.");
    }

    private void HandleForcedTrialResolved(ILieForcedTrialDriver driver, ChallengeResult result)
    {
        if (driver != activeDriver)
            return;

        if (activeDriver != null)
        {
            activeDriver.OnForcedTrialResolved -= HandleForcedTrialResolved;
            activeDriver = null;
        }

        activeTrialType = LieForcedTrialType.None;
        OnForcedTrialResolved?.Invoke(this, result);
    }

    private void SetPairActive(GameObject stageRoot, GameObject panelRoot, bool isActive)
    {
        if (stageRoot != null)
            stageRoot.SetActive(isActive);

        if (panelRoot != null)
            panelRoot.SetActive(isActive);
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[LieForcedStageHost] {message}", this);
    }
}