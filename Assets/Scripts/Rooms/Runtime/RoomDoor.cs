using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [SerializeField] private RoomDirection direction;
    [SerializeField] private bool requiresTag = true;
    [SerializeField] private string playerTag = "Player";

    private RoomManager roomManager;

    public RoomDirection Direction => direction;

    public void SetRoomManager(RoomManager manager)
    {
        roomManager = manager;
    }

    private void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (roomManager == null) return;

        if (requiresTag)
        {
            if (!other.CompareTag(playerTag)) return;
        }

        roomManager.RequestTransition(direction);
    }
}