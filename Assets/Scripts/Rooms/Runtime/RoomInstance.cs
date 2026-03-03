using UnityEngine;

public class RoomInstance : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private BoxCollider2D roomBounds;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private Transform spawnFromNorth;
    [SerializeField] private Transform spawnFromEast;
    [SerializeField] private Transform spawnFromSouth;
    [SerializeField] private Transform spawnFromWest;

    [Header("Doors")]
    public RoomDoor[] Doors { get; private set; }
    public RoomDoorLockable[] DoorLocks { get; private set; }

    private void Awake()
    {
        Doors = GetComponentsInChildren<RoomDoor>(true);
        DoorLocks = GetComponentsInChildren<RoomDoorLockable>(true);
    }

    public BoxCollider2D RoomBounds => roomBounds;

    public Vector3 GetSpawnPosition(RoomDirection? enteredFrom)
    {
        // enteredFrom = the side we ENTERED this room from.
        if (enteredFrom == null)
        {
            return spawnCenter != null ? spawnCenter.position : transform.position;
        }

        switch (enteredFrom.Value)
        {
            case RoomDirection.North: return spawnFromNorth != null ? spawnFromNorth.position : transform.position;
            case RoomDirection.East:  return spawnFromEast  != null ? spawnFromEast.position  : transform.position;
            case RoomDirection.South: return spawnFromSouth != null ? spawnFromSouth.position : transform.position;
            case RoomDirection.West:  return spawnFromWest  != null ? spawnFromWest.position  : transform.position;
        }

        return transform.position;
    }

    private void OnValidate()
    {
        if (roomBounds == null)
        {
            // Try to auto-find a bounds collider named RoomBounds
            var found = GetComponentInChildren<BoxCollider2D>();
            if (found != null) roomBounds = found;
        }
    }
}