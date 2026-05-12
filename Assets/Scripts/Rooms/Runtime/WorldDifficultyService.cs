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
        if (ring <= 5) return 1;
        if (ring <= 10) return 2;
        if (ring <= 15) return 3;
        if (ring <= 20) return 4;
        if (ring <= 25) return 5;
        if (ring <= 30) return 6;
        if (ring <= 35) return 7;
        if (ring <= 40) return 8;
        if (ring <= 45) return 9;
        return 10;
    }
}