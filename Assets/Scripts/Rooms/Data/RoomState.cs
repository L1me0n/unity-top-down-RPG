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

    public RoomState(
        bool visited,
        bool cleared,
        int remainingEnemies = -1,
        RoomType roomType = RoomType.Combat,
        int lastVisitedStep = -1,
        int lastClearedStep = -1,
        int repopulationBlockedUntilStep = 0,
        int timesRepopulated = 0)
    {
        this.visited = visited;
        this.cleared = cleared;
        this.remainingEnemies = remainingEnemies;
        this.roomType = roomType;

        this.lastVisitedStep = lastVisitedStep;
        this.lastClearedStep = lastClearedStep;
        this.repopulationBlockedUntilStep = repopulationBlockedUntilStep;
        this.timesRepopulated = timesRepopulated;

        encounterInitialized = false;
        combatLevel = 0;
        encounterSeed = 0;
    }
}