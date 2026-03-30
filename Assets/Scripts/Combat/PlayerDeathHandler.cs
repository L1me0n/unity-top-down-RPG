using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private RoomManager roomManager;

    private PlayerDamageReceiver receiver;

    private void Awake()
    {
        if (roomManager == null)
            roomManager = FindFirstObjectByType<RoomManager>();

        receiver = GetComponent<PlayerDamageReceiver>();

        if (receiver != null && roomManager != null)
            receiver.OnDied += roomManager.RespawnAtCheckpoint;
    }

    private void OnDestroy()
    {
        if (receiver != null && roomManager != null)
            receiver.OnDied -= roomManager.RespawnAtCheckpoint;
    }
}