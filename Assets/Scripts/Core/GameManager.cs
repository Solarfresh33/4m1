using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Victory
    }

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;

    private GameState currentState = GameState.Playing;
    private int totalBolts;
    private GameObject playerInstance;

    public GameState CurrentState => currentState;
    public int TotalBolts => totalBolts;

    public System.Action<GameState> OnGameStateChanged;
    public System.Action<int> OnBoltsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartNewRun();
    }

    public void StartNewRun()
    {
        currentState = GameState.Playing;
        totalBolts = 0;
        OnBoltsChanged?.Invoke(totalBolts);
        OnGameStateChanged?.Invoke(currentState);

        if (FloorManager.Instance != null)
            FloorManager.Instance.ResetToFloorOne();

        // Reset player if exists
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.ResetHealth();
                health.OnDeath += OnPlayerDeath;
            }

            var weaponHandler = player.GetComponent<PlayerWeaponHandler>();
            if (weaponHandler != null)
                weaponHandler.ClearWeapons();

            if (FloorManager.Instance != null && FloorManager.Instance.CurrentRoom != null)
                player.transform.position = FloorManager.Instance.CurrentRoom.transform.position;
        }
    }

    private void OnPlayerDeath()
    {
        currentState = GameState.GameOver;
        OnGameStateChanged?.Invoke(currentState);
    }

    public void AddBolts(int amount)
    {
        totalBolts += amount;
        OnBoltsChanged?.Invoke(totalBolts);
    }

    public bool SpendBolts(int amount)
    {
        if (totalBolts < amount) return false;
        totalBolts -= amount;
        OnBoltsChanged?.Invoke(totalBolts);
        return true;
    }

    public void RestartRun()
    {
        StartNewRun();
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        OnGameStateChanged?.Invoke(currentState);
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        OnGameStateChanged?.Invoke(currentState);
    }
}
