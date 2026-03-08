[System.Serializable]
public class RoomState
{
    public bool visited;
    public bool cleared;

    // -1 means "not initialized yet"
    public int remainingEnemies = -1;

    public RoomState(bool visited, bool cleared, int remainingEnemies = -1)
    {
        this.visited = visited;
        this.cleared = cleared;
        this.remainingEnemies = remainingEnemies;
    }
}