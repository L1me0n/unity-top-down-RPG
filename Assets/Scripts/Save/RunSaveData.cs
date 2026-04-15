using System;
using System.Collections.Generic;

[Serializable]
public class RunSaveData
{
    public int souls;
    public int xp;

    public int level;
    public int unspentPoints;

    public int demon;
    public int monster;
    public int fallenGod;
    public int hellhound;

    public int maxHPLoss;
    public int maxAPLoss;
    public int actionRateLoss;
    public int dpLoss;

    public int playerRoomX;
    public int playerRoomY;

    public bool hasActivatedCampfireCheckpoint;
    public int checkpointRoomX;
    public int checkpointRoomY;

    public int runStepCount;

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

    public int challengeType;
    public bool challengeCompleted;
    public int lastChallengeCompletedStep;

    public bool encounterInitialized;
    public int combatLevel;
    public int encounterSeed;

    public int lastVisitedStep;
    public int lastClearedStep;
    public int repopulationBlockedUntilStep;
    public int timesRepopulated;

    public bool hasHostageGhosts;
    public int hostageGhostCount;
    public bool hostageGhostsRescued;

    public int storedHostageGhostCount;

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