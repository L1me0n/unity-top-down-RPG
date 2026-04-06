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
    // -1 means "this event has never happened yet"
    public int lastVisitedStep;
    public int lastClearedStep;

    // If current run step is below this value, repopulation is blocked
    public int repopulationBlockedUntilStep;

    // Tracks how many times this room has been repopulated in the future system
    public int timesRepopulated;

    // Hostages
    public bool hasHostageGhosts;
    public int hostageGhostCount;
    public bool hostageGhostsRescued;

    // Campfire storage
    public int storedHostageGhostCount;

    //Challenge rooms
    public ChallengeType challengeType;
    public bool challengeCompleted;
    public int lastChallengeCompletedStep; 

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
        int lastChallengeCompletedStep = -1)
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
    }
}