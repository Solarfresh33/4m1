using UnityEngine;

public enum WeaponType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "4M1/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    public string WeaponName = "Weapon";
    public WeaponType WeaponType = WeaponType.Melee;
    public int Damage = 10;
    public float FireRate = 2f;
    public Sprite Icon;

    [Header("Ranged")]
    public int MaxAmmo = 12;
    public float ReloadTime = 1.5f;
    public float ProjectileSpeed = 10f;
    public float ProjectileRange = 8f;

    [Header("Melee")]
    public float MeleeRange = 1.2f;
}
