using UnityEngine;
using System.Collections.Generic;
using System;

public class Room : MonoBehaviour
{
    public enum RoomType
    {
        Normal,
        Boss,
        Start,
        Treasure
    }

    [Header("Room Settings")]
    [SerializeField] private RoomType roomType = RoomType.Normal;
    [SerializeField] private Vector2 roomSize = new Vector2(16f, 12f);

    private List<EnemyBase> enemies = new List<EnemyBase>();
    private List<Door> doors = new List<Door>();
    private bool isCleared;
    private bool isActive;

    public RoomType Type => roomType;
    public Vector2 RoomSize => roomSize;
    public bool IsCleared => isCleared;
    public bool IsActive => isActive;

    public event Action<Room> OnRoomCleared;

    public void Initialize(RoomType type, Vector2 size)
    {
        roomType = type;
        roomSize = size;
        isCleared = type == RoomType.Start;
    }

    public void ActivateRoom(int floorNumber)
    {
        isActive = true;

        if (roomType == RoomType.Normal)
        {
            if (EnemySpawner.Instance != null)
            {
                enemies = EnemySpawner.Instance.SpawnEnemiesInRoom(this, floorNumber);
                foreach (var enemy in enemies)
                {
                    enemy.OnEnemyDeath += OnEnemyDied;
                }
            }
            LockDoors();
        }
        else if (roomType == RoomType.Boss)
        {
            if (EnemySpawner.Instance != null)
            {
                var boss = EnemySpawner.Instance.SpawnBoss(this, floorNumber);
                if (boss != null)
                {
                    enemies.Add(boss);
                    boss.OnEnemyDeath += OnEnemyDied;
                }
            }
            LockDoors();
        }
        else if (roomType == RoomType.Start || roomType == RoomType.Treasure)
        {
            isCleared = true;
        }
    }

    private void OnEnemyDied(EnemyBase enemy)
    {
        enemies.Remove(enemy);
        if (enemies.Count == 0)
        {
            isCleared = true;
            UnlockDoors();
            OnRoomCleared?.Invoke(this);
        }
    }

    public void RegisterDoor(Door door)
    {
        if (!doors.Contains(door))
            doors.Add(door);
    }

    private void LockDoors()
    {
        foreach (var door in doors)
            door.Lock();
    }

    private void UnlockDoors()
    {
        foreach (var door in doors)
            door.Unlock();
    }

    public Bounds GetSpawnBounds()
    {
        Vector3 center = transform.position;
        Vector3 size = new Vector3(roomSize.x - 2f, roomSize.y - 2f, 0f);
        return new Bounds(center, size);
    }

    public void BuildWalls()
    {
        float halfW = roomSize.x / 2f;
        float halfH = roomSize.y / 2f;
        float wallThickness = 0.5f;

        CreateWall("WallTop", new Vector2(0, halfH), new Vector2(roomSize.x, wallThickness));
        CreateWall("WallBottom", new Vector2(0, -halfH), new Vector2(roomSize.x, wallThickness));
        CreateWall("WallLeft", new Vector2(-halfW, 0), new Vector2(wallThickness, roomSize.y));
        CreateWall("WallRight", new Vector2(halfW, 0), new Vector2(wallThickness, roomSize.y));
    }

    private void CreateWall(string wallName, Vector2 localPos, Vector2 size)
    {
        var wall = new GameObject(wallName);
        wall.transform.SetParent(transform);
        wall.transform.localPosition = localPos;
        wall.layer = LayerMask.NameToLayer("Wall");

        var col = wall.AddComponent<BoxCollider2D>();
        col.size = size;

        var sr = wall.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
        sr.sortingOrder = 1;
    }
}
