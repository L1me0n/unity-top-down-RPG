using System;

[Serializable]
public class RoomEnemyStateEntry
{
    public EnemyType enemyType;
    public int spawnPointIndex;
    public bool alive = true;

    public RoomEnemyStateEntry() { }

    public RoomEnemyStateEntry(EnemyType enemyType, int spawnPointIndex, bool alive = true)
    {
        this.enemyType = enemyType;
        this.spawnPointIndex = spawnPointIndex;
        this.alive = alive;
    }
}