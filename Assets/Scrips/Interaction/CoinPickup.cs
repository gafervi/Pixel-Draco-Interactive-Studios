using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private float lifetime = 15f; // Tiempo antes de desaparecer
    
    private CoinSystem coinSystem;
    private PlayerStats playerStats;
    private Transform player;
    private bool isMovingToPlayer = false;
    private bool isCollected = false;
    
    void Start()
    {
        player = FindObjectOfType<PlayerController>()?.transform;
        
        // Buscar PlayerStats (sistema principal de monedas)
        playerStats = FindObjectOfType<PlayerStats>();
        
        // Add slight random rotation
        transform.Rotate(0, Random.Range(0, 360), 0);
        
        // Auto-destruir después del tiempo de vida
        // esto es para que no se quede en el suelo para siempre y porque algunas monedas estan dando error y se caen al vacio
        Destroy(gameObject, lifetime);
        
        // Debug para verificar conexión
        if (playerStats == null)
        {
            Debug.LogError("CoinPickup: No se encontró PlayerStats. La moneda no funcionará correctamente.");
        }
    }
    
    void Update()
    {
        if (isCollected) return;
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            
            if (distance <= pickupRange)
            {
                if (!isMovingToPlayer)
                {
                    isMovingToPlayer = true;
                }
                
                // Move towards player
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
                
                if (distance <= 0.5f)
                {
                    CollectCoin();
                }
            }
        }
    }
    
    void CollectCoin()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Usar PlayerStats como sistema principal
        if (playerStats != null)
        {
            playerStats.AddCoins(coinValue);
            Debug.Log($"Moneda recolectada: +{coinValue} monedas");
        }
        else
        {
            Debug.LogError("CoinPickup: No se pudo agregar monedas - PlayerStats no encontrado");
        }
        
        // Efecto visual
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Destruir la moneda
        Destroy(gameObject);
    }
    
    public void SetCoinSystem(CoinSystem system)
    {
        coinSystem = system;
    }
    
    public void SetCoinValue(int value)
    {
        coinValue = value;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}