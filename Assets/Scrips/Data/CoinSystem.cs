using UnityEngine;
using UnityEngine.Events;

public class CoinSystem : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private AudioClip coinCollectSound;
    
    [Header("Events")]
    public UnityEvent<int> OnCoinsChanged;
    public UnityEvent<int> OnCoinsCollected;
    
    // Properties
    public int CurrentCoins => currentCoins;
    
    void Start()
    {
        OnCoinsChanged?.Invoke(currentCoins);
    }
    
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        OnCoinsChanged?.Invoke(currentCoins);
        OnCoinsCollected?.Invoke(amount);
        
        Debug.Log($"Coins added: {amount}. Total: {currentCoins}");
    }
    
    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            OnCoinsChanged?.Invoke(currentCoins);
            return true;
        }
        return false;
    }
    
    public void SetCoins(int amount)
    {
        currentCoins = amount;
        OnCoinsChanged?.Invoke(currentCoins);
    }
    
    public void SpawnCoins(Vector3 position, int minAmount, int maxAmount)
    {
        int coinAmount = Random.Range(minAmount, maxAmount + 1);
        
        for (int i = 0; i < coinAmount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                0.5f,
                Random.Range(-1f, 1f)
            );
            
            GameObject coin = Instantiate(coinPrefab, position + offset, Quaternion.identity);
            coin.GetComponent<CoinPickup>().SetCoinSystem(this);
        }
    }
    
    public void PlayCoinSound()
    {
        if (coinCollectSound != null)
        {
            AudioSource.PlayClipAtPoint(coinCollectSound, Camera.main.transform.position);
        }
    }
}
