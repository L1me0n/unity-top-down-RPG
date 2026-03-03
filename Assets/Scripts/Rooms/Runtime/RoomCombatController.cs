using System.Collections.Generic;
using UnityEngine;

public class RoomCombatController : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject hellpuppyPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform enemySpawnPointsRoot; // EnemySpawnPoints container
    [SerializeField] private bool shuffleSpawns = true;

    [Header("Spawn Count (v1)")]
    [SerializeField] private int minEnemies = 2;
    [SerializeField] private int maxEnemies = 5;

    private readonly List<GameObject> alive = new List<GameObject>();

    private RoomManager roomManager;
    private Vector2Int roomCoord;

    public int AliveCount => alive.Count;

    private void Awake()
    {
        if (enemySpawnPointsRoot == null)
        {
            var t = transform.Find("EnemySpawnPoints");
            if (t != null) enemySpawnPointsRoot = t;
        }
    }

    // Called by RoomManager right after room is spawned
    public void OnRoomEntered(RoomManager manager, Vector2Int coord, bool isClearedAlready)
    {
        roomManager = manager;
        roomCoord = coord;

        if (isClearedAlready)
        {
            SetDoorsLocked(false);
            return;
        }

        SpawnWaveV1();
        SetDoorsLocked(true);

        // Edge-case: if spawn config results in 0 enemies
        if (alive.Count == 0)
        {
            HandleRoomCleared();
        }
    }

    private void SpawnWaveV1()
    {
        if (hellpuppyPrefab == null)
        {
            Debug.LogError("[RoomCombatController] Missing hellpuppyPrefab!");
            return;
        }
        if (enemySpawnPointsRoot == null)
        {
            Debug.LogError("[RoomCombatController] Missing EnemySpawnPoints root!");
            return;
        }

        Transform[] points = enemySpawnPointsRoot.GetComponentsInChildren<Transform>(true);
        // points[0] is root; skip it
        List<Transform> usable = new List<Transform>();
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == enemySpawnPointsRoot) continue;
            usable.Add(points[i]);
        }

        if (usable.Count == 0) return;

        if (shuffleSpawns) Shuffle(usable);

        int count = Random.Range(minEnemies, maxEnemies + 1);
        count = Mathf.Clamp(count, 1, usable.Count); // v1: no reuse

        for (int i = 0; i < count; i++)
        {
            Transform sp = usable[i];
            GameObject e = Instantiate(hellpuppyPrefab, sp.position, Quaternion.identity);

            // Link enemy death back to this room
            var link = e.AddComponent<EnemyRoomLink>();
            link.Init(this);

            alive.Add(e);
        }
    }

    // Called by EnemyRoomLink when enemy dies/destroys
    public void NotifyEnemyDead(GameObject enemy)
    {
        alive.Remove(enemy);

        if (alive.Count <= 0)
        {
            HandleRoomCleared();
        }
    }

    private void HandleRoomCleared()
    {
        SetDoorsLocked(false);

        if (roomManager != null)
        {
            roomManager.MarkCurrentRoomCleared(); // we’ll add this in RoomManager below
        }
    }

    private void SetDoorsLocked(bool locked)
    {
        if (roomManager == null || roomManager.CurrentRoom == null) return;
        

        var doors = roomManager.CurrentRoom.Doors; 
        if (doors == null) return;

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i] == null) continue;

            var lockable = doors[i].GetComponent<RoomDoorLockable>();
            if (lockable != null)
            {
                lockable.SetLocked(locked);
            }
            else
            {
                // Fallback: disable collider if no lockable script
                var col = doors[i].GetComponent<Collider2D>();
                if (col != null) col.enabled = !locked;
            }
        }
    }

    private void Shuffle(List<Transform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}