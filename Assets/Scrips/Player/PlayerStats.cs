using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    private float lastDamageTime;

    [Header("Coins")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private AudioClip coinCollectSound;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnPlayerDeath;
    public UnityEvent<int> OnCoinsChanged;

    // Properties
    public float Health => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => currentHealth / maxHealth;
    public int Coins => currentCoins;

    public bool IsAlive => currentHealth > 0;

    void Start()
    {
        // Initialize stats
        currentHealth = maxHealth;
        
        // Invoke initial events
        OnHealthChanged?.Invoke(HealthPercentage);
        OnCoinsChanged?.Invoke(currentCoins);
    }

    void Update()
    {
    }

    // Health Methods
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        lastDamageTime = Time.time;
        
        OnHealthChanged?.Invoke(HealthPercentage);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(HealthPercentage);
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(HealthPercentage);
    }

    

    // Death
    private void Die()
    {
        OnPlayerDeath?.Invoke();
    }

    // Coin Methods
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        OnCoinsChanged?.Invoke(currentCoins);
        
        // Reproducir sonido de moneda
        PlayCoinSound();
        
        Debug.Log($"Coins added: {amount}. Total: {currentCoins}");
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            OnCoinsChanged?.Invoke(currentCoins);
            Debug.Log($"Coins spent: {amount}. Remaining: {currentCoins}");
            return true;
        }
        return false;
    }

    public void SetCoins(int amount)
    {
        currentCoins = amount;
        OnCoinsChanged?.Invoke(currentCoins);
    }

    private void PlayCoinSound()
    {
        if (coinCollectSound != null)
        {
            AudioSource.PlayClipAtPoint(coinCollectSound, Camera.main.transform.position);
        }
    }

    // Reset all stats
    public void ResetStats()
    {
        currentHealth = maxHealth;
        lastDamageTime = 0;
        
        OnHealthChanged?.Invoke(HealthPercentage);
        OnCoinsChanged?.Invoke(currentCoins);
    }
}