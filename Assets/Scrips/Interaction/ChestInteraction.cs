using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private bool isOpened = false;
    [SerializeField] private int minCoins = 5;
    [SerializeField] private int maxCoins = 25;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private GameObject openEffect;
    
    [Header("Visual Settings")]
    [SerializeField] private GameObject closedChest;
    [SerializeField] private GameObject openedChest;
    
    private CoinSystem coinSystem;
    private Transform player;
    private bool isPlayerInRange = false;
    
    void Start()
    {
        coinSystem = FindObjectOfType<CoinSystem>();
        player = FindObjectOfType<PlayerController>()?.transform;
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        UpdateChestVisuals();
    }
    
    void Update()
    {
        if (player != null && !isOpened)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = isPlayerInRange;
            isPlayerInRange = distance <= interactionRange;
            
            // Show/hide interaction prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(isPlayerInRange);
            }
            
            // Handle interaction
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                OpenChest();
            }
            
            // Play sound when player enters range
            if (isPlayerInRange && !wasInRange && openSound != null)
            {
                AudioSource.PlayClipAtPoint(openSound, transform.position);
            }
        }
    }
    
    void OpenChest()
    {
        if (isOpened) return;
        
        isOpened = true;
        
        // Spawn coins
        if (coinSystem != null)
        {
            int coinAmount = Random.Range(minCoins, maxCoins + 1);
            coinSystem.SpawnCoins(transform.position, coinAmount, coinAmount);
        }
        
        // Play sound
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
        
        // Play effect
        if (openEffect != null)
        {
            Instantiate(openEffect, transform.position, Quaternion.identity);
        }
        
        // Update visuals
        UpdateChestVisuals();
        
        // Hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        Debug.Log($"Chest opened! Dropped {Random.Range(minCoins, maxCoins + 1)} coins");
    }
    
    void UpdateChestVisuals()
    {
        if (closedChest != null)
            closedChest.SetActive(!isOpened);
            
        if (openedChest != null)
            openedChest.SetActive(isOpened);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
