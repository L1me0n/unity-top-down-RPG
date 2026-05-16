using System;

[Serializable]
public class RunSavePreviewData
{
    public bool hasSave;

    public int roomX;
    public int roomY;

    public int level;
    public int unspentPoints;

    public int souls;
    public int xp;

    public int gluttonyClues;
    public bool gluttonyBossUnlocked;
    public bool gluttonyBossDefeated;
    public bool hungerClueUnlocked;
    public bool mvpEndingReached;
}