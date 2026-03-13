using UnityEngine;

public static class WorldDifficultyService
{
    public static int GetRing(Vector2Int coord)
    {
        return Mathf.Max(Mathf.Abs(coord.x), Mathf.Abs(coord.y));
    }

    public static int GetCombatLevel(Vector2Int coord)
    {
        int ring = GetRing(coord);

        // temporary compressed mapping for your small bounded test map
        if (ring == 0) return 1;
        if (ring == 1) return 2;
        if (ring == 2) return 3;
        if (ring == 3) return 4;
        return 5;
    }
}