using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Door : MonoBehaviour
{
    public enum DoorDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField] private DoorDirection direction;
    [SerializeField] private Room connectedRoom;

    private BoxCollider2D col;
    private SpriteRenderer sr;
    private bool isLocked;

    public DoorDirection Direction => direction;
    public Room ConnectedRoom => connectedRoom;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 1f);

        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;

        UpdateVisual();
    }

    public void Setup(DoorDirection dir, Room connected)
    {
        direction = dir;
        connectedRoom = connected;
    }

    public void Lock()
    {
        isLocked = true;
        UpdateVisual();
    }

    public void Unlock()
    {
        isLocked = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (sr != null)
            sr.color = isLocked ? new Color(0.6f, 0.2f, 0.2f, 1f) : new Color(0.2f, 0.6f, 0.2f, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLocked) return;

        if (other.CompareTag("Player"))
        {
            FloorManager.Instance?.TransitionToRoom(connectedRoom, direction);
        }
    }
}
