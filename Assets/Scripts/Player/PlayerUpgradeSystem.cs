using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PlayerUpgrade
{
    public string upgradeName;
    public string description;
    public int cost;
    public UpgradeType type;
    public float value;
    public int maxLevel;
    public int currentLevel;
}

public enum UpgradeType
{
    Health,
    Damage,
    Speed,
    RandomWeapon
}

public class PlayerUpgradeSystem : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] private PlayerUpgrade[] availableUpgrades;

    [Header("Random Weapon Settings")]
    [SerializeField] private WeaponData[] randomWeapons;
    [SerializeField] private int randomWeaponCost = 100;

    [Header("Events")]
    public UnityEvent<PlayerUpgrade> OnUpgradePurchased;
    public UnityEvent<WeaponData> OnRandomWeaponPurchased;

    private PlayerController playerController;
    private PlayerStats playerStats;
    private CoinSystem coinSystem;
    private InventoryManager inventoryManager;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        coinSystem = GetComponent<CoinSystem>();
        inventoryManager = FindObjectOfType<InventoryManager>();

        InitializeUpgrades();
    }

    void InitializeUpgrades()
    {
        // Initialize upgrade levels
        foreach (var upgrade in availableUpgrades)
        {
            upgrade.currentLevel = 0;
        }
    }

    public bool PurchaseUpgrade(int upgradeIndex)
    {
        if (upgradeIndex < 0 || upgradeIndex >= availableUpgrades.Length)
            return false;

        PlayerUpgrade upgrade = availableUpgrades[upgradeIndex];

        // Check if already at max level
        if (upgrade.currentLevel >= upgrade.maxLevel)
            return false;

        // Check if player has enough coins
        if (!coinSystem.SpendCoins(upgrade.cost))
            return false;

        // Apply upgrade
        ApplyUpgrade(upgrade);
        upgrade.currentLevel++;

        OnUpgradePurchased?.Invoke(upgrade);
        Debug.Log($"Upgrade purchased: {upgrade.upgradeName} (Level {upgrade.currentLevel})");

        return true;
    }

    public bool PurchaseRandomWeapon()
    {
        if (randomWeapons == null || randomWeapons.Length == 0)
            return false;

        // Check if player has enough coins
        if (!coinSystem.SpendCoins(randomWeaponCost))
            return false;

        // Get random weapon
        int randomIndex = Random.Range(0, randomWeapons.Length);
        WeaponData randomWeapon = randomWeapons[randomIndex];

        // Add weapon to inventory (you'll need to create ItemSO for this)
        if (inventoryManager != null)
        {
            OnRandomWeaponPurchased?.Invoke(randomWeapon);
            Debug.Log($"Random weapon purchased: {randomWeapon.weaponName} for {randomWeaponCost} coins");
        }

        return true;
    }

    void ApplyUpgrade(PlayerUpgrade upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.Health:
                // Increase max health
                if (playerStats != null)
                {
                    playerStats.SetMaxHealth(playerStats.MaxHealth + upgrade.value);
                }
                break;

            case UpgradeType.Damage:
                // Increase attack damage - now handled by WeaponCombat
                // This upgrade could modify weapon damage or add damage bonuses
                Debug.Log($"Damage upgrade applied: +{upgrade.value}");
                break;

            case UpgradeType.Speed:
                // Increase movement speed
                if (playerController != null)
                {
                    playerController.moveSpeed += upgrade.value;
                    playerController.sprintSpeed += upgrade.value;
                }
                break;

            case UpgradeType.RandomWeapon:
                // Purchase random weapon
                PurchaseRandomWeapon();
                break;
        }
    }

    public PlayerUpgrade[] GetAvailableUpgrades()
    {
        return availableUpgrades;
    }

    public bool CanAffordUpgrade(int upgradeIndex)
    {
        if (upgradeIndex < 0 || upgradeIndex >= availableUpgrades.Length)
            return false;

        return coinSystem.CurrentCoins >= availableUpgrades[upgradeIndex].cost;
    }

    public bool CanAffordRandomWeapon()
    {
        return coinSystem.CurrentCoins >= randomWeaponCost;
    }
}

