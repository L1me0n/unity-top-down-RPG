using UnityEngine;

public class LieSneakController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DisappearController disappearController;
    [SerializeField] private PlayerCombatController combatController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ChallengeEffectManager challengeEffectManager;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool lieSneakActive;
    private bool lieSneakShouldFail;
    private bool lieSneakFailureTriggered;

    public bool IsLieSneakActive => lieSneakActive;
    public bool LieSneakShouldFail => lieSneakShouldFail;
    public bool LieSneakFailureTriggered => lieSneakFailureTriggered;

    public System.Action OnLieSneakStarted;
    public System.Action OnLieSneakEnded;
    public System.Action OnLieSneakFailureTriggered;

    private void Awake()
    {
        if (disappearController == null)
            disappearController = FindFirstObjectByType<DisappearController>();

        if (combatController == null)
            combatController = FindFirstObjectByType<PlayerCombatController>();

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (challengeEffectManager == null)
            challengeEffectManager = FindFirstObjectByType<ChallengeEffectManager>();
    }

    public void BeginLieSneak(bool shouldFail)
    {
        lieSneakActive = true;
        lieSneakShouldFail = shouldFail;
        lieSneakFailureTriggered = false;

        if (combatController != null)
            combatController.SetMode(CombatMode.Defense);

        if (disappearController != null)
            disappearController.BeginLieSneakOverride();

        Log($"Lie sneak started | shouldFail={shouldFail}");
        OnLieSneakStarted?.Invoke();
    }

    public void EndLieSneak()
    {
        if (!lieSneakActive)
            return;

        lieSneakActive = false;
        lieSneakShouldFail = false;

        if (disappearController != null)
            disappearController.EndLieSneakOverride();

        if (combatController != null)
            combatController.SetMode(CombatMode.Attack);

        Log("Lie sneak ended.");
        OnLieSneakEnded?.Invoke();
    }

    public bool TryTriggerLieSneakFailure()
    {
        if (!lieSneakActive)
            return false;

        if (!lieSneakShouldFail)
            return false;

        if (lieSneakFailureTriggered)
            return false;

        lieSneakFailureTriggered = true;

        if (disappearController != null)
            disappearController.BreakLieSneakOverride();

        Log("Lie sneak failure triggered.");
        OnLieSneakFailureTriggered?.Invoke();
        return true;
    }

    private void Log(string message)
    {
        if (!log)
            return;

        Debug.Log($"[LieSneakController] {message}", this);
    }
}