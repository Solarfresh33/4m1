using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invincibilityDuration = 0.5f;

    private int currentHealth;
    private float invincibilityTimer;
    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public bool IsInvincible => invincibilityTimer > 0f;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (invincibilityTimer > 0f)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || IsInvincible) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        invincibilityTimer = invincibilityDuration;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetInvincible(float duration)
    {
        invincibilityTimer = Mathf.Max(invincibilityTimer, duration);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        invincibilityTimer = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
