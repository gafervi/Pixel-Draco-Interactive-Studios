using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Basic Item Info")]
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public int stackMax = 1;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip dropSound;
    
    [Header("Item Type")]
    public ItemType itemType = ItemType.Consumable;
    
    [Header("Weapon Data (Only for Weapons)")]
    public WeaponData weaponData;
    
    [Header("Consumable Data (Only for Consumables)")]
    public ConsumableData consumableData;
}

public enum ItemType
{
    Weapon,
    Consumable,
    Tool,
    Material
}

[System.Serializable]
public class ConsumableData
{
    [Header("Consumable Type")]
    public ConsumableType consumableType = ConsumableType.SmallHeal;
    
    [Header("Consumable Effects")]
    public float healAmount = 0f;
    public float duration = 0f;
    
    [Header("Consumable Visual/Audio Effects")]
    public GameObject consumeEffect;
    public AudioClip consumeSound;
}

public enum ConsumableType
{
    SmallHeal,    // Curación pequeña (fija)
    LargeHeal,    // Curación grande (fija)
    RandomHeal     // Curación aleatoria (5-50% o daño 1-30)
}
