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
            result.Add(new RoomEnemyStateEntry(
                EnemyType.Hellpuppy,
                shuffledSpawnIndices[i],
                true
            ));
        }

        return result;
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