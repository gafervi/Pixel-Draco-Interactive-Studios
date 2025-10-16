using UnityEngine;

[System.Serializable]
public class WeaponData
{
    [Header("General Info")]
    public string weaponName;
    public GameObject weaponPrefab;

    [Header("Weapon Stats")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;

    [Header("Weapon Type")]
    public WeaponType weaponType = WeaponType.Melee;
    
    [Header("Animation Settings")]
    [Tooltip("Si es true, usa animaciones Attack 1/2. Si es false, no usa animaciones")]
    public bool useAttackAnimations = true;

    [Header("Weapon Effects")]
    public GameObject hitEffect;
    public AudioClip weaponSwing;
    public AudioClip hitSound;
    
    [Header("Magic Projectile (Solo para armas Magic)")]
    public GameObject magicProjectilePrefab;
    public float projectileSpeed = 50f;
    public float explosionRadius = 3f;
    
    [Header("Magic Projectile Audio")]
    public AudioClip fireballSound;      // Sonido al disparar la bola
    public AudioClip magicExplosionSound; // Sonido al explotar
    public float fireballVolume = 0.7f;
    public float explosionVolume = 0.8f;

}

public enum WeaponType
{
    Melee,      // Espadas, hachas, etc. - Usa Attack 1/2
    Magic       // Libros de magia, varitas - Sin animaciones
}
