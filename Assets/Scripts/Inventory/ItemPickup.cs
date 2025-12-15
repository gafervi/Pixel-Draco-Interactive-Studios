using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemSO itemData;
    [SerializeField] private int quantity = 1;
    
    private InventoryManager inventoryManager;
    private Transform player;
    private bool isPlayerInRange = false;
    private bool isCollected = false;
    private bool isDroppedItem = false; // Para distinguir items tirados vs encontrados
    
    void Start()
    {
        player = FindObjectOfType<PlayerController>()?.transform;
        inventoryManager = FindObjectOfType<InventoryManager>();
        
        SetupEverything();
    }
    
    void Update()
    {
        if (isCollected) return;
        
        CheckPlayerDistance();
        HandleInteraction();
    }
    
    private void SetupEverything()
    {

        ValidatePrefabSetup();
    }
    
    private void ValidatePrefabSetup()
    {
        // Validar que el prefab tenga los componentes necesarios
        bool hasCollider = GetComponent<Collider>() != null;
        bool hasRigidbody = GetComponent<Rigidbody>() != null;
        bool hasRenderer = GetComponentInChildren<Renderer>() != null;
 
    }
    

    
    private void CheckPlayerDistance()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerInRange = distance <= 1f; // Rango fijo de 2 unidades
        }
    }
    
    private void HandleInteraction()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            CollectItem();
        }
    }
    
    public void CollectItem()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        if (inventoryManager != null && itemData != null)
        {
            bool success = inventoryManager.AddItem(itemData, quantity);
            
            if (success)
            {
                Debug.Log($"Item {itemData.itemName} x{quantity} collected");
                
                // Play pickup sound
                if (itemData.pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(itemData.pickupSound, transform.position);
                }
                
                // Para armas, activar el arma correspondiente en la mano del jugador
                if (itemData.itemType == ItemType.Weapon)
                {
                    inventoryManager.ActivateWeaponInHand(itemData);
                }
            }
            else
            {
                isCollected = false;
                Debug.LogWarning("Could not add item to inventory");
                return;
            }
        }
        
        Destroy(gameObject);
    }
    
    public void SetItemData(ItemSO item, int qty = 1, bool dropped = false)
    {
        itemData = item;
        quantity = qty;
        isDroppedItem = dropped;
        
        if (Application.isPlaying)
        {
            SetupEverything();
        }
    }
    
    public ItemSO GetItemData() => itemData;
    public int GetQuantity() => quantity;
    public bool IsPlayerInRange() => isPlayerInRange;
    public bool IsDroppedItem() => isDroppedItem;
}