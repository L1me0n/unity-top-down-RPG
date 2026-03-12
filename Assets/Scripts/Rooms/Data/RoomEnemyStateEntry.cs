using System;

[Serializable]
public class RoomEnemyStateEntry
{
    public EnemyType enemyType;
    public int spawnPointIndex;
    public bool alive = true;
}