using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;

    private PlayerDamageReceiver receiver;

    private void Awake()
    {
        receiver = GetComponent<PlayerDamageReceiver>();
        receiver.OnDied += roomManager.RespawnInCurrentRoom;

    }
}