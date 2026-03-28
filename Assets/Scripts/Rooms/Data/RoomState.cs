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

    public RoomState(bool visited, bool cleared, int remainingEnemies = -1, RoomType roomType = RoomType.Combat)
    {
        this.visited = visited;
        this.cleared = cleared;
        this.remainingEnemies = remainingEnemies;
        this.roomType = roomType;
        
        encounterInitialized = false;
        combatLevel = 0;
        encounterSeed = 0;
    }
}