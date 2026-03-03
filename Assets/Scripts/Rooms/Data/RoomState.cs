[System.Serializable]
public class RoomState
{
    public bool visited;
    public bool cleared;

    public RoomState(bool visited, bool cleared)
    {
        this.visited = visited;
        this.cleared = cleared;
    }
}