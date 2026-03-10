using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text healthText;
    [SerializeField] private Text ammoText;
    [SerializeField] private Text boltsText;
    [SerializeField] private Text floorText;
    [SerializeField] private Text weaponNameText;
    [SerializeField] private Text reloadText;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

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
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (reloadText != null) reloadText.gameObject.SetActive(false);

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.OnHealthChanged += UpdateHealthUI;
                UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
            }

            var weaponHandler = player.GetComponent<PlayerWeaponHandler>();
            if (weaponHandler != null)
            {
                weaponHandler.OnAmmoChanged += UpdateAmmoUI;
                weaponHandler.OnWeaponChanged += UpdateWeaponUI;
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBoltsChanged += UpdateBoltsUI;
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        if (FloorManager.Instance != null)
        {
            FloorManager.Instance.OnFloorChanged += UpdateFloorUI;
        }
    }

    private void Update()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var weaponHandler = player.GetComponent<PlayerWeaponHandler>();
            if (weaponHandler != null && reloadText != null)
            {
                reloadText.gameObject.SetActive(weaponHandler.IsReloading);
            }
        }
    }

    public void UpdateHealthUI(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
        if (healthText != null)
            healthText.text = $"{current}/{max}";
    }

    public void UpdateAmmoUI(int current, int max)
    {
        if (ammoText == null) return;
        if (current < 0)
            ammoText.text = "MELEE";
        else
            ammoText.text = $"{current}/{max}";
    }

    public void UpdateBoltsUI(int bolts)
    {
        if (boltsText != null)
            boltsText.text = bolts.ToString();
    }

    public void UpdateFloorUI(int floor)
    {
        if (floorText != null)
            floorText.text = $"Floor {floor}";
    }

    public void UpdateWeaponUI(WeaponInstance weapon)
    {
        if (weaponNameText != null && weapon != null)
            weaponNameText.text = weapon.Data.WeaponName;
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(state == GameManager.GameState.GameOver);
        if (pausePanel != null)
            pausePanel.SetActive(state == GameManager.GameState.Paused);
    }

    public void OnRestartButtonClicked()
    {
        GameManager.Instance?.RestartRun();
    }

    public void OnResumeButtonClicked()
    {
        GameManager.Instance?.ResumeGame();
    }
}
