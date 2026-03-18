using System.Collections.Generic;
using UnityEngine;

public static class EncounterGenerator
{
    public static List<RoomEnemyStateEntry> Generate(
        Vector2Int roomCoord,
        int combatLevel,
        int encounterSeed,
        int spawnPointCount)
    {
        var result = new List<RoomEnemyStateEntry>();

        if (spawnPointCount <= 0)
            return result;

        int enemyCount = GetEnemyCountForCombatLevel(combatLevel);
        enemyCount = Mathf.Clamp(enemyCount, 1, spawnPointCount);

        List<int> shuffledSpawnIndices = BuildShuffledIndices(spawnPointCount, encounterSeed);

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyType enemyType = GetEnemyTypeForSlot(
                roomCoord,
                combatLevel,
                encounterSeed,
                i,
                enemyCount
            );

            result.Add(new RoomEnemyStateEntry(
                enemyType,
                shuffledSpawnIndices[i],
                true
            ));
        }

        return result;
    }

    private static EnemyType GetEnemyTypeForSlot(
        Vector2Int roomCoord,
        int combatLevel,
        int encounterSeed,
        int slotIndex,
        int enemyCount)
    {
        if (combatLevel <= 1)
            return EnemyType.Hellpuppy;

        if (combatLevel == 2)
        {
            int verminSlot = PositiveMod(encounterSeed, enemyCount);
            return slotIndex == verminSlot ? EnemyType.Vermin : EnemyType.Hellpuppy;
        }

        if (combatLevel == 3)
        {
            int verminSlot = PositiveMod(encounterSeed, enemyCount);
            int infernoSlot = PositiveMod(encounterSeed / 7 + 3, enemyCount);

            if (slotIndex == verminSlot)
                return EnemyType.Vermin;

            if (infernoSlot == verminSlot)
                infernoSlot = (infernoSlot + 1) % enemyCount;

            if (slotIndex == infernoSlot)
                return EnemyType.Inferno;

            return EnemyType.Hellpuppy;
        }

        // Level 4+:
        // Always at least 1 Vermin and 1 Inferno.
        // Remaining slots are mostly Hellpuppies, with occasional extra Vermin.
        int firstVerminSlot = PositiveMod(encounterSeed, enemyCount);
        int infernoSlotLevel4 = PositiveMod(encounterSeed / 7 + 3, enemyCount);

        if (infernoSlotLevel4 == firstVerminSlot)
            infernoSlotLevel4 = (infernoSlotLevel4 + 1) % enemyCount;

        if (slotIndex == firstVerminSlot)
            return EnemyType.Vermin;

        if (slotIndex == infernoSlotLevel4)
            return EnemyType.Inferno;

        bool allowSecondVermin = enemyCount >= 5 && (PositiveMod(encounterSeed, 2) == 0);
        int secondVerminSlot = PositiveMod(encounterSeed / 11 + 5, enemyCount);

        if (secondVerminSlot == firstVerminSlot || secondVerminSlot == infernoSlotLevel4)
            secondVerminSlot = (secondVerminSlot + 1) % enemyCount;

        if (allowSecondVermin && slotIndex == secondVerminSlot)
            return EnemyType.Vermin;

        return EnemyType.Hellpuppy;
    }

    private static int GetEnemyCountForCombatLevel(int combatLevel)
    {
        switch (combatLevel)
        {
            case 1: return 2;
            case 2: return 3;
            case 3: return 4;
            case 4: return 4;
            case 5: return 5;
            case 6: return 6;
            case 7: return 6;
            case 8: return 6;
            default: return 2;
        }
    }

    private static List<int> BuildShuffledIndices(int count, int seed)
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < count; i++)
            indices.Add(i);

        System.Random rng = new System.Random(seed);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int temp = indices[i];
            indices[i] = indices[j];
            indices[j] = temp;
        }

        return indices;
    }

    private static int PositiveMod(int value, int mod)
    {
        if (mod <= 0)
            return 0;

        int result = value % mod;
        return result < 0 ? result + mod : result;
    }

    public static int BuildEncounterSeed(Vector2Int coord, int combatLevel)
    {
        unchecked
        {
            int seed = 17;
            seed = seed * 31 + coord.x;
            seed = seed * 31 + coord.y;
            seed = seed * 31 + combatLevel;
            return seed;
        }
    }
}