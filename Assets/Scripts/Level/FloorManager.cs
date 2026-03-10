using UnityEngine;
using System.Collections.Generic;
using System;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

    [Header("Floor Settings")]
    [SerializeField] private int roomsPerFloor = 5;
    [SerializeField] private Vector2 roomSize = new Vector2(16f, 12f);
    [SerializeField] private float roomSpacing = 20f;

    private int currentFloor = 1;
    private List<Room> currentRooms = new List<Room>();
    private Room currentRoom;
    private Room bossRoom;

    public int CurrentFloor => currentFloor;
    public Room CurrentRoom => currentRoom;

    public event Action<int> OnFloorChanged;
    public event Action<Room> OnRoomChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void GenerateFloor()
    {
        ClearFloor();

        // Generate rooms in a linear path (tower ascending)
        for (int i = 0; i < roomsPerFloor; i++)
        {
            var roomGo = new GameObject($"Room_{i}");
            roomGo.transform.SetParent(transform);
            roomGo.transform.position = new Vector3(0, i * roomSpacing, 0);

            var room = roomGo.AddComponent<Room>();
            Room.RoomType type;

            if (i == 0)
                type = Room.RoomType.Start;
            else if (i == roomsPerFloor - 1)
                type = Room.RoomType.Boss;
            else
                type = Room.RoomType.Normal;

            room.Initialize(type, roomSize);
            room.BuildWalls();
            room.OnRoomCleared += OnRoomClearedHandler;

            currentRooms.Add(room);
        }

        // Create doors between adjacent rooms
        for (int i = 0; i < currentRooms.Count - 1; i++)
        {
            var currentRm = currentRooms[i];
            var nextRm = currentRooms[i + 1];

            // Door going up in current room
            var doorUpGo = new GameObject("Door_Up");
            doorUpGo.transform.SetParent(currentRm.transform);
            doorUpGo.transform.localPosition = new Vector3(0, roomSize.y / 2f, 0);
            var doorUp = doorUpGo.AddComponent<Door>();
            doorUp.Setup(Door.DoorDirection.Up, nextRm);
            currentRm.RegisterDoor(doorUp);

            // Door going down in next room
            var doorDownGo = new GameObject("Door_Down");
            doorDownGo.transform.SetParent(nextRm.transform);
            doorDownGo.transform.localPosition = new Vector3(0, -roomSize.y / 2f, 0);
            var doorDown = doorDownGo.AddComponent<Door>();
            doorDown.Setup(Door.DoorDirection.Down, currentRm);
            nextRm.RegisterDoor(doorDown);
        }

        bossRoom = currentRooms[currentRooms.Count - 1];
        currentRoom = currentRooms[0];
        OnFloorChanged?.Invoke(currentFloor);
    }

    public void TransitionToRoom(Room targetRoom, Door.DoorDirection fromDirection)
    {
        if (targetRoom == null) return;

        currentRoom = targetRoom;

        // Move player to appropriate position in new room
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 entryOffset = fromDirection switch
            {
                Door.DoorDirection.Up => new Vector3(0, -roomSize.y / 2f + 2f, 0),
                Door.DoorDirection.Down => new Vector3(0, roomSize.y / 2f - 2f, 0),
                Door.DoorDirection.Left => new Vector3(roomSize.x / 2f - 2f, 0, 0),
                Door.DoorDirection.Right => new Vector3(-roomSize.x / 2f + 2f, 0, 0),
                _ => Vector3.zero
            };
            player.transform.position = targetRoom.transform.position + entryOffset;
        }

        if (!targetRoom.IsCleared)
            targetRoom.ActivateRoom(currentFloor);

        OnRoomChanged?.Invoke(targetRoom);
    }

    private void OnRoomClearedHandler(Room room)
    {
        if (room == bossRoom)
        {
            AdvanceFloor();
        }
    }

    public void AdvanceFloor()
    {
        currentFloor++;
        GenerateFloor();

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && currentRooms.Count > 0)
        {
            player.transform.position = currentRooms[0].transform.position;
        }
    }

    public void ResetToFloorOne()
    {
        currentFloor = 1;
        GenerateFloor();
    }

    private void ClearFloor()
    {
        foreach (var room in currentRooms)
        {
            if (room != null)
                Destroy(room.gameObject);
        }
        currentRooms.Clear();
    }
}
