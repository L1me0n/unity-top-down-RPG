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
        if (ring <= 1) return 1;
        if (ring == 2) return 2;
        if (ring == 3) return 3;
        if (ring == 4) return 4;
        if (ring == 5) return 5;
        if (ring == 6) return 6;
        if (ring == 7) return 7;
        if (ring == 8) return 8;
        return 9;
    }
}