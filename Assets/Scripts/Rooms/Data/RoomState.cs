using System.Collections.Generic;

[System.Serializable]
public class RoomState
{
    public bool visited;
    public bool cleared;

    // -1 means "not initialized yet"
    public int remainingEnemies = -1;

    public RoomType roomType;

    public int combatLevel;
    public int encounterSeed;
    public bool encounterInitialized;
    public List<RoomEnemyStateEntry> enemyStates = new List<RoomEnemyStateEntry>();

    // 7.5: repopulation memory fields
    public int lastVisitedStep;
    public int lastClearedStep;
    public int repopulationBlockedUntilStep;
    public int timesRepopulated;

    // Hostages
    public bool hasHostageGhosts;
    public int hostageGhostCount;
    public bool hostageGhostsRescued;

    // Campfire storage
    public int storedHostageGhostCount;

    // Challenge rooms
    public ChallengeType challengeType;
    public bool challengeCompleted;
    public int lastChallengeCompletedStep;

    // Lie mid-progress persistence
    public bool lieProgressActive;
    public int lieChosenRoute;
    public int lieRuntimeState;
    public bool lieSneakOutcomeRolled;
    public bool lieSneakWillSucceed;
    public bool lieSneakAttemptFinished;
    public bool lieTrialsPrepared;
    public int lieForcedTrialCount;
    public int lieCurrentTrialIndex;
    public int lieForcedTrial0;
    public int lieForcedTrial1;
    public int lieForcedTrial2;

    public RoomState(
        bool visited,
        bool cleared,
        int remainingEnemies = -1,
        RoomType roomType = RoomType.Combat,
        int lastVisitedStep = -1,
        int lastClearedStep = -1,
        int repopulationBlockedUntilStep = 0,
        int timesRepopulated = 0,
        bool hasHostageGhosts = false,
        int hostageGhostCount = 0,
        bool hostageGhostsRescued = false,
        int storedHostageGhostCount = 0,
        ChallengeType challengeType = ChallengeType.None,
        bool challengeCompleted = false,
        int lastChallengeCompletedStep = -1,
        bool lieProgressActive = false,
        int lieChosenRoute = 0,
        int lieRuntimeState = 0,
        bool lieSneakOutcomeRolled = false,
        bool lieSneakWillSucceed = false,
        bool lieSneakAttemptFinished = false,
        bool lieTrialsPrepared = false,
        int lieForcedTrialCount = 0,
        int lieCurrentTrialIndex = -1,
        int lieForcedTrial0 = 0,
        int lieForcedTrial1 = 0,
        int lieForcedTrial2 = 0)
    {
        this.visited = visited;
        this.cleared = cleared;
        this.remainingEnemies = remainingEnemies;
        this.roomType = roomType;

        this.lastVisitedStep = lastVisitedStep;
        this.lastClearedStep = lastClearedStep;
        this.repopulationBlockedUntilStep = repopulationBlockedUntilStep;
        this.timesRepopulated = timesRepopulated;

        this.hasHostageGhosts = hasHostageGhosts;
        this.hostageGhostCount = hostageGhostCount;
        this.hostageGhostsRescued = hostageGhostsRescued;
        this.storedHostageGhostCount = storedHostageGhostCount;

        encounterInitialized = false;
        combatLevel = 0;
        encounterSeed = 0;

        this.challengeType = challengeType;
        this.challengeCompleted = challengeCompleted;
        this.lastChallengeCompletedStep = lastChallengeCompletedStep;

        this.lieProgressActive = lieProgressActive;
        this.lieChosenRoute = lieChosenRoute;
        this.lieRuntimeState = lieRuntimeState;
        this.lieSneakOutcomeRolled = lieSneakOutcomeRolled;
        this.lieSneakWillSucceed = lieSneakWillSucceed;
        this.lieSneakAttemptFinished = lieSneakAttemptFinished;
        this.lieTrialsPrepared = lieTrialsPrepared;
        this.lieForcedTrialCount = lieForcedTrialCount;
        this.lieCurrentTrialIndex = lieCurrentTrialIndex;
        this.lieForcedTrial0 = lieForcedTrial0;
        this.lieForcedTrial1 = lieForcedTrial1;
        this.lieForcedTrial2 = lieForcedTrial2;
    }
}