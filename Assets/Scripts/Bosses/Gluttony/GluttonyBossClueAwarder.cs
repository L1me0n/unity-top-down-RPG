using UnityEngine;

public class GluttonyBossClueAwarder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChallengeRoomController challengeRoomController;
    [SerializeField] private BossProgressionManager bossProgressionManager;

    [Header("Behavior")]
    [SerializeField] private bool requireSuccessfulOutcome = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private void Awake()
    {
        if (challengeRoomController == null)
            challengeRoomController = GetComponentInParent<ChallengeRoomController>();

        if (bossProgressionManager == null)
            bossProgressionManager = FindFirstObjectByType<BossProgressionManager>();
    }

    private void OnEnable()
    {
        if (challengeRoomController == null)
            challengeRoomController = GetComponentInParent<ChallengeRoomController>();

        if (bossProgressionManager == null)
            bossProgressionManager = FindFirstObjectByType<BossProgressionManager>();

        if (challengeRoomController != null)
            challengeRoomController.OnChallengeResolved += HandleChallengeResolved;
    }

    private void OnDisable()
    {
        if (challengeRoomController != null)
            challengeRoomController.OnChallengeResolved -= HandleChallengeResolved;
    }

    private void HandleChallengeResolved(
        ChallengeRoomController controller,
        ChallengeResult result)
    {
        if (controller == null)
        {
            Log("Ignored challenge result because controller was null.");
            return;
        }

        if (result == null)
        {
            Log("Ignored null challenge result.");
            return;
        }

        if (result.challengeType != ChallengeType.Gluttony)
        {
            Log("Ignored non-Gluttony challenge result: " + result.challengeType);
            return;
        }

        if (requireSuccessfulOutcome && !result.IsSuccess)
        {
            Log("Ignored Gluttony result because it was not successful. Outcome: " + result.outcome);
            return;
        }

        if (bossProgressionManager == null)
            bossProgressionManager = BossProgressionManager.Instance;

        if (bossProgressionManager == null)
        {
            Debug.LogWarning(
                "[GluttonyBossClueAwarder] BossProgressionManager missing. Could not award Gluttony clue.",
                this
            );
            return;
        }

        Vector2Int sourceRoomCoord = controller.RoomCoord;

        bool awarded = bossProgressionManager.TryAwardGluttonyClue(sourceRoomCoord);

        if (awarded)
        {
            Log(
                "Awarded Gluttony boss clue from room " + sourceRoomCoord +
                ". Count: " + bossProgressionManager.GluttonyClueCount + "/4"
            );
        }
        else
        {
            Log(
                "No clue awarded from room " + sourceRoomCoord +
                ". It may have already awarded a clue, or the boss is already unlocked/defeated."
            );
        }
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[GluttonyBossClueAwarder] " + message, this);
    }
}