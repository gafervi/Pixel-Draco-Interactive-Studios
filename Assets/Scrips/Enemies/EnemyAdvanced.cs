using UnityEngine;
using UnityEngine.Events;

public class EnemyAdvanced : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 5f;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    
    [Header("Reward Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoinReward = 2;
    [SerializeField] private int maxCoinReward = 10;
    [SerializeField] private bool isBoss = false;
    
    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent<int> OnTakeDamage;
    public UnityEvent OnPlayerAttack;
    
    private float lastAttackTime;
    private Transform player;
    private PlayerStats playerStats;
    private bool isDead = false;
    
    // Properties
    public int Health => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)currentHealth / maxHealth;
    public bool IsAlive => currentHealth > 0 && !isDead;

    void Awake()
    {
        // Initialize health
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Buscar al jugador en la escena
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (isDead) return;
        
        if (player != null && playerStats != null)
        {
            HandleEnemyBehavior();
        }
    }

    private void HandleEnemyBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Si el jugador está en rango de detección
        if (distanceToPlayer <= detectionRange)
        {
            // Mover hacia el jugador
            MoveTowardsPlayer();
            
            // Atacar si está en rango
            if (distanceToPlayer <= attackRange)
            {
                AttackPlayer();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Mantener en el suelo
        
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Rotar hacia el jugador
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void AttackPlayer()
    {
        // Si puede atacar (cooldown terminado)
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // Quitar vida al jugador
            playerStats.TakeDamage(damage);
            lastAttackTime = Time.time;
            
            OnPlayerAttack?.Invoke();
            Debug.Log($"Enemigo atacó al jugador por {damage} de daño");
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        
        currentHealth -= amount;
        OnTakeDamage?.Invoke(amount);
        
        Debug.Log($"Enemigo recibió {amount} de daño. Vida restante: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        if (isDead) return;
        
        isDead = true;
        OnDeath?.Invoke();
        
        // Dropear monedas
        DropCoins();
        
       
        
        Debug.Log("Enemigo murió");
        
        // Destruir el objeto
        Destroy(gameObject);
    }

    private void DropCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("EnemyAdvanced: coinPrefab no está configurado. No se dropearán monedas.");
            return;
        }
        
        // Calcular cantidad de monedas
        int coinAmount = isBoss ? 100 : Random.Range(minCoinReward, maxCoinReward + 1);
        
        for (int i = 0; i < coinAmount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                1.5f, // Aumentar altura para evitar que se hundan en el suelo
                Random.Range(-1f, 1f)
            );
            
            GameObject coin = Instantiate(coinPrefab, transform.position + offset, Quaternion.identity);
            
            // Configurar el valor de la moneda si tiene el componente CoinPickup
            CoinPickup coinPickup = coin.GetComponent<CoinPickup>();
            if (coinPickup != null)
            {
                coinPickup.SetCoinValue(1); // Cada moneda vale 1
            }
        }
        
        Debug.Log($"Enemigo dropeó {coinAmount} monedas");
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"Enemigo se curó {amount} puntos. Vida actual: {currentHealth}");
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    // Método para dibujar rangos en el editor
    void OnDrawGizmosSelected()
    {
        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
