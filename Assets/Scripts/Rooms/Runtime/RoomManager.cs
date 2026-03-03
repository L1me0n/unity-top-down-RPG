using System.Collections;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject roomPrefab; // RoomBase

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Collider2D playerCollider;

    [Header("Transition Tuning")]
    [SerializeField] private float transitionLockSeconds = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool logTransitions = true;

    [Header("Camera Clamping")]
    [SerializeField] private CameraFollow cameraClampBehaviour;

    public Vector2Int CurrentCoord => currentCoord;
    public RoomInstance CurrentRoom => currentRoom;

    private Vector2Int currentCoord;
    private RoomInstance currentRoom;
    private bool isTransitioning;

    private void Awake()
    {
        if (playerRb == null && player != null) playerRb = player.GetComponent<Rigidbody2D>();
        if (playerCollider == null && player != null) playerCollider = player.GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Start at (0,0)
        LoadRoom(Vector2Int.zero, enteredFrom: null);
    }

    public void RequestTransition(RoomDirection viaDoorDirection)
    {
        if (isTransitioning) return;
        if (roomPrefab == null || player == null) return;

        Vector2Int next = currentCoord + DirToDelta(viaDoorDirection);

        // When we go NORTH through a door, the next room is entered FROM SOUTH.
        RoomDirection enteredFromSideInNewRoom = Opposite(viaDoorDirection);

        if (logTransitions)
            Debug.Log($"[RoomManager] Transition {currentCoord} -> {next} via {viaDoorDirection} (enter new room from {enteredFromSideInNewRoom})");

        StartCoroutine(DoTransition(next, enteredFromSideInNewRoom));
    }

    private IEnumerator DoTransition(Vector2Int nextCoord, RoomDirection enteredFrom)
    {
        isTransitioning = true;

        // Stop motion so we don't “skate” into the next trigger.
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        // Disable collider briefly to avoid instant retrigger.
        if (playerCollider != null) playerCollider.enabled = false;

        LoadRoom(nextCoord, enteredFrom);

        // Wait a tiny bit to clear overlaps, then re-enable.
        yield return new WaitForSeconds(transitionLockSeconds);

        if (playerCollider != null) playerCollider.enabled = true;

        isTransitioning = false;
    }

    private void LoadRoom(Vector2Int coord, RoomDirection? enteredFrom)
    {
        // Destroy old
        if (currentRoom != null)
        {
            Destroy(currentRoom.gameObject);
            currentRoom = null;
        }

        // Spawn new
        GameObject roomGo = Instantiate(roomPrefab);
        roomGo.name = $"Room_{coord.x}_{coord.y}";

        currentRoom = roomGo.GetComponent<RoomInstance>();
        if (currentRoom == null)
        {
            Debug.LogError("[RoomManager] Spawned room prefab has no RoomInstance component.");
            return;
        }

        cameraClampBehaviour.SetRoomBounds(currentRoom.RoomBounds);

        // Update coord
        currentCoord = coord;

        // Hook doors to this manager
        var doors = roomGo.GetComponentsInChildren<RoomDoor>(true);
        for (int i = 0; i < doors.Length; i++)
            doors[i].SetRoomManager(this);

        // Place player at the correct spawn
        Vector3 spawnPos = currentRoom.GetSpawnPosition(enteredFrom);
        player.position = spawnPos;

    }

    private Vector2Int DirToDelta(RoomDirection d)
    {
        switch (d)
        {
            case RoomDirection.North: return new Vector2Int(0, 1);
            case RoomDirection.East:  return new Vector2Int(1, 0);
            case RoomDirection.South: return new Vector2Int(0, -1);
            case RoomDirection.West:  return new Vector2Int(-1, 0);
        }
        return Vector2Int.zero;
    }

    private RoomDirection Opposite(RoomDirection d)
    {
        switch (d)
        {
            case RoomDirection.North: return RoomDirection.South;
            case RoomDirection.East:  return RoomDirection.West;
            case RoomDirection.South: return RoomDirection.North;
            case RoomDirection.West:  return RoomDirection.East;
        }
        return d;
    }
}