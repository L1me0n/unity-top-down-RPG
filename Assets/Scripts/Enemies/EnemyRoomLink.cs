using UnityEngine;

public class EnemyRoomLink : MonoBehaviour
{
    private RoomCombatController room;
    private Health health;
    private bool notifiedDeath = false;

    public void Init(RoomCombatController roomController)
    {
        room = roomController;
        health = GetComponent<Health>();

        if (health != null)
        {
            health.OnDied += HandleDied;
        }
    }

    private void HandleDied()
    {
        if (notifiedDeath) return;
        notifiedDeath = true;

        if (room != null)
        {
            room.NotifyEnemyDead(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDied -= HandleDied;
        }
    }
}