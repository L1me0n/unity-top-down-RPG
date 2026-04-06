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

    public bool hasActivatedCampfireCheckpoint;
    public int checkpointRoomX;
    public int checkpointRoomY;

    // 7.5: deterministic progress clock for future repopulation logic.
    public int runStepCount;

    // Tips seen
    public bool hasSeenGeneralChallengeTips;
    public bool hasSeenBettingTips;
    public bool hasSeenGluttonyTips;
    public bool hasSeenSlothTips;
    public bool hasSeenLieTips;

    public RoomStateSaveEntry[] rooms;

    public ChallengeEffectSaveEntry[] activeChallengeEffects;
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

    // Challenge rooms
    public int challengeType;
    public bool challengeCompleted;
    public int lastChallengeCompletedStep;

    public bool encounterInitialized;
    public int combatLevel;
    public int encounterSeed;

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

    public List<RoomEnemyStateSaveEntry> enemyStates = new List<RoomEnemyStateSaveEntry>();
}

[Serializable]
public class RoomEnemyStateSaveEntry
{
    public int enemyType;
    public int spawnPointIndex;
    public bool alive;
}

[Serializable]
public class ChallengeEffectSaveEntry
{
    public int sourceChallenge;
    public int effectType;
    public float value;
    public bool clearsOnNextChallengeEntry;
    public string debugLabel;
}