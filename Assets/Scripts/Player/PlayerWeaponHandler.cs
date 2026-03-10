using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerWeaponHandler : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private List<WeaponData> startingWeapons;

    private List<WeaponInstance> weapons = new List<WeaponInstance>();
    private int currentWeaponIndex;
    private float attackCooldownTimer;
    private float reloadTimer;
    private bool isReloading;

    private PlayerController playerController;
    private Camera mainCamera;

    public WeaponInstance CurrentWeapon => weapons.Count > 0 ? weapons[currentWeaponIndex] : null;
    public bool IsReloading => isReloading;

    public System.Action<WeaponInstance> OnWeaponChanged;
    public System.Action<int, int> OnAmmoChanged;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        mainCamera = Camera.main;

        if (weaponPivot == null)
        {
            var pivot = new GameObject("WeaponPivot");
            pivot.transform.SetParent(transform);
            pivot.transform.localPosition = Vector3.zero;
            weaponPivot = pivot.transform;
        }
    }

    private void Start()
    {
        foreach (var data in startingWeapons)
        {
            AddWeapon(data);
        }

        if (weapons.Count > 0)
            OnWeaponChanged?.Invoke(weapons[0]);
    }

    private void Update()
    {
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                FinishReload();
            }
        }

        UpdateWeaponRotation();
    }

    private void UpdateWeaponRotation()
    {
        if (mainCamera == null) return;

        bool usingGamepad = Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.1f;

        Vector2 aimDirection;
        if (usingGamepad)
        {
            aimDirection = Gamepad.current.rightStick.ReadValue().normalized;
        }
        else
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            aimDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        }

        if (aimDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (CurrentWeapon == null || isReloading) return;
        if (attackCooldownTimer > 0f) return;

        var weapon = CurrentWeapon;
        attackCooldownTimer = 1f / weapon.Data.FireRate;

        if (weapon.Data.WeaponType == WeaponType.Ranged)
        {
            if (weapon.CurrentAmmo <= 0)
            {
                StartReload();
                return;
            }

            weapon.CurrentAmmo--;
            OnAmmoChanged?.Invoke(weapon.CurrentAmmo, weapon.Data.MaxAmmo);
            FireProjectile(weapon);
        }
        else
        {
            PerformMeleeAttack(weapon);
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (CurrentWeapon == null) return;
        if (CurrentWeapon.Data.WeaponType != WeaponType.Ranged) return;
        if (isReloading || CurrentWeapon.CurrentAmmo >= CurrentWeapon.Data.MaxAmmo) return;

        StartReload();
    }

    public void OnNextWeapon(InputAction.CallbackContext context)
    {
        if (!context.performed || weapons.Count <= 1) return;
        CancelReload();
        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Count;
        OnWeaponChanged?.Invoke(CurrentWeapon);
        NotifyAmmoChange();
    }

    public void OnPreviousWeapon(InputAction.CallbackContext context)
    {
        if (!context.performed || weapons.Count <= 1) return;
        CancelReload();
        currentWeaponIndex = (currentWeaponIndex - 1 + weapons.Count) % weapons.Count;
        OnWeaponChanged?.Invoke(CurrentWeapon);
        NotifyAmmoChange();
    }

    private void FireProjectile(WeaponInstance weapon)
    {
        Vector2 direction = weaponPivot.right;
        Vector3 spawnPos = weaponPivot.position + (Vector3)(direction * 0.5f);

        var projectile = ProjectilePool.Instance.GetProjectile();
        projectile.transform.position = spawnPos;
        projectile.Init(direction, weapon.Data.Damage, weapon.Data.ProjectileSpeed, weapon.Data.ProjectileRange, true);
    }

    private void PerformMeleeAttack(WeaponInstance weapon)
    {
        Vector2 direction = weaponPivot.right;
        Vector2 attackPos = (Vector2)transform.position + direction * weapon.Data.MeleeRange * 0.5f;
        var hits = Physics2D.OverlapCircleAll(attackPos, weapon.Data.MeleeRange, LayerMask.GetMask("Enemy"));

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(weapon.Data.Damage);
            }
        }
    }

    private void StartReload()
    {
        isReloading = true;
        reloadTimer = CurrentWeapon.Data.ReloadTime;
    }

    private void FinishReload()
    {
        isReloading = false;
        if (CurrentWeapon != null)
        {
            CurrentWeapon.CurrentAmmo = CurrentWeapon.Data.MaxAmmo;
            NotifyAmmoChange();
        }
    }

    private void CancelReload()
    {
        isReloading = false;
        reloadTimer = 0f;
    }

    private void NotifyAmmoChange()
    {
        if (CurrentWeapon != null && CurrentWeapon.Data.WeaponType == WeaponType.Ranged)
            OnAmmoChanged?.Invoke(CurrentWeapon.CurrentAmmo, CurrentWeapon.Data.MaxAmmo);
        else
            OnAmmoChanged?.Invoke(-1, -1);
    }

    public void AddWeapon(WeaponData data)
    {
        var instance = new WeaponInstance(data);
        weapons.Add(instance);
    }

    public void ClearWeapons()
    {
        weapons.Clear();
        currentWeaponIndex = 0;
    }
}

[System.Serializable]
public class WeaponInstance
{
    public WeaponData Data;
    public int CurrentAmmo;

    public WeaponInstance(WeaponData data)
    {
        Data = data;
        CurrentAmmo = data.MaxAmmo;
    }
}
