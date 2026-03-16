using UnityEngine;

public class EnemyRoomLink : MonoBehaviour
{
    private RoomCombatController room;

    public void Init(RoomCombatController roomController)
    {
        room = roomController;
    }

    public void NotifyEnemyDied(GameObject enemy)
    {
        if (room != null)
            room.NotifyEnemyDead(enemy);
    }
    
    public RoomCombatController GetRoomController()
    {
        return room;
    }
}