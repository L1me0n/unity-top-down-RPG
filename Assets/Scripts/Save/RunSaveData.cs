using System;
using System.Collections.Generic;

[Serializable]
public class RunSaveData
{
    // Currency
    public int souls;
    public int xp;

    // Level system
    public int level;
    public int unspentPoints;

    // Branches
    public int demon;
    public int monster;
    public int fallenGod;
    public int hellhound;

    // Penalties
    public int maxHPLoss;
    public int maxAPLoss;
    public int actionRateLoss;
    public int dpLoss;

    public int playerRoomX;
    public int playerRoomY;

    public RoomStateSaveEntry[] rooms;
}

[Serializable]
public class RoomStateSaveEntry
{
    public int x;
    public int y;
    public bool visited;
    public bool cleared;
    public int remainingEnemies;

    public int roomType;

    public bool encounterInitialized;
    public int combatLevel;
    public int encounterSeed;

    public List<RoomEnemyStateSaveEntry> enemyStates = new List<RoomEnemyStateSaveEntry>();
}

[Serializable]
public class RoomEnemyStateSaveEntry
{
    public int enemyType;
    public int spawnPointIndex;
    public bool alive;
}