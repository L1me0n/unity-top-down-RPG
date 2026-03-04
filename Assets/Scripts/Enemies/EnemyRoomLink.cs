using UnityEngine;

public class EnemyRoomLink : MonoBehaviour
{
    private RoomCombatController room;
    private Health health;

    public void Init(RoomCombatController roomController)
    {
        room = roomController;
    }

    private void Awake()
    {
        health = GetComponent<Health>();
    }


    private void OnDestroy()
    {
        if (room != null)
        {
            room.NotifyEnemyDead(gameObject);
        }
    }
}