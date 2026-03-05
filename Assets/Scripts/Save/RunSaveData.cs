using System;

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
    public int dpLoss;
}