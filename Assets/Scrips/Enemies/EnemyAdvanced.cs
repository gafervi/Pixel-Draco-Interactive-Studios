using UnityEngine;
using UnityEngine.Events;

public class EnemyAdvanced : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 6f;

    [Header("Orientation Fix")]
    [SerializeField] private bool invertForward = false; 

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    [Header("Reward Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoinReward = 2;
    [SerializeField] private int maxCoinReward = 10;
    [SerializeField] private bool isBoss = false;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent<int> OnTakeDamage;
    public UnityEvent OnPlayerAttack;

    private Transform player;
    private PlayerStats playerStats;
    private Animator animator;

    private float lastAttackTime;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (isDead || player == null || playerStats == null) return;

        HandleEnemyBehavior();
    }

    private void HandleEnemyBehavior()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            if (distance > attackRange)
            {
                MoveTowardsPlayer();
                animator.SetFloat("Speed", 1f); // WALK
            }
            else
            {
                animator.SetFloat("Speed", 0f); // IDLE
                AttackPlayer();
            }
        }
        else
        {
            animator.SetFloat("Speed", 0f); // IDLE
        }
    }

    private void MoveTowardsPlayer()
    {
        
        if (isDead) return;

        
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return;

        Vector3 direction = (player.position - transform.position);
        direction.y = 0;
        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Vector3 lookDir = invertForward ? -direction : direction;
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetTrigger("Attack"); // ATAQUE
            playerStats.TakeDamage(damage);
            lastAttackTime = Time.time;

            OnPlayerAttack?.Invoke();
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        OnTakeDamage?.Invoke(amount);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool("Dead", true); // MUERTE
        OnDeath?.Invoke();

        DropCoins();

        Destroy(gameObject, 2f); // tiempo para ver animación
    }

    private void DropCoins()
    {
        if (coinPrefab == null) return;

        int coinAmount = isBoss
            ? 100
            : UnityEngine.Random.Range(minCoinReward, maxCoinReward + 1);

        for (int i = 0; i < coinAmount; i++)
        {
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                1.5f,
                UnityEngine.Random.Range(-1f, 1f)
            );

            GameObject coin = Instantiate(
                coinPrefab,
                transform.position + offset,
                Quaternion.identity
            );

            CoinPickup pickup = coin.GetComponent<CoinPickup>();
            if (pickup != null)
            {
                pickup.SetCoinValue(1);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
