using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossProgressionState
{
    public int gluttonyClueCount;
    public bool gluttonyBossUnlocked;
    public bool gluttonyBossDefeated;
    public bool hungerHorsemanClueUnlocked;
    public bool mvpEndingReached;

    public List<string> gluttonyClueAwardedRoomKeys = new List<string>();

    public bool HasRoomAwardedGluttonyClue(Vector2Int coord)
    {
        string key = ToRoomKey(coord);
        return gluttonyClueAwardedRoomKeys != null &&
               gluttonyClueAwardedRoomKeys.Contains(key);
    }

    public void MarkRoomAwardedGluttonyClue(Vector2Int coord)
    {
        if (gluttonyClueAwardedRoomKeys == null)
            gluttonyClueAwardedRoomKeys = new List<string>();

        string key = ToRoomKey(coord);

        if (!gluttonyClueAwardedRoomKeys.Contains(key))
            gluttonyClueAwardedRoomKeys.Add(key);
    }

    public void ClampAndRepair()
    {
        if (gluttonyClueAwardedRoomKeys == null)
            gluttonyClueAwardedRoomKeys = new List<string>();

        gluttonyClueCount = Mathf.Clamp(gluttonyClueCount, 0, 4);

        if (gluttonyClueCount >= 4)
            gluttonyBossUnlocked = true;

        if (gluttonyBossDefeated)
        {
            gluttonyBossUnlocked = true;
            hungerHorsemanClueUnlocked = true;
            mvpEndingReached = true;
        }
    }

    public BossProgressionState Clone()
    {
        BossProgressionState copy = new BossProgressionState();

        copy.gluttonyClueCount = gluttonyClueCount;
        copy.gluttonyBossUnlocked = gluttonyBossUnlocked;
        copy.gluttonyBossDefeated = gluttonyBossDefeated;
        copy.hungerHorsemanClueUnlocked = hungerHorsemanClueUnlocked;
        copy.mvpEndingReached = mvpEndingReached;

        copy.gluttonyClueAwardedRoomKeys =
            gluttonyClueAwardedRoomKeys != null
                ? new List<string>(gluttonyClueAwardedRoomKeys)
                : new List<string>();

        copy.ClampAndRepair();
        return copy;
    }

    public static string ToRoomKey(Vector2Int coord)
    {
        return coord.x + "," + coord.y;
    }
}