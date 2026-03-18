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
            // Exactly one Vermin in level 2 rooms, placed deterministically.
            int verminSlot = PositiveMod(encounterSeed, enemyCount);
            return slotIndex == verminSlot ? EnemyType.Vermin : EnemyType.Hellpuppy;
        }

        // Level 3+ for now:
        // at least one Vermin, sometimes two, but still mostly Hellpuppies.
        int firstVerminSlot = PositiveMod(encounterSeed, enemyCount);
        int secondVerminSlot = PositiveMod(encounterSeed / 7 + 3, enemyCount);

        bool allowSecondVermin = enemyCount >= 4 && (PositiveMod(encounterSeed, 2) == 0);

        if (slotIndex == firstVerminSlot)
            return EnemyType.Vermin;

        if (allowSecondVermin && slotIndex == secondVerminSlot && secondVerminSlot != firstVerminSlot)
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