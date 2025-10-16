using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventoryItem
{
    public ItemSO itemData;
    public int quantity;
    public bool isEmpty => itemData == null || quantity <= 0;
}

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 7;
    [SerializeField] private Transform handParent;
    
    [Header("Weapon System")]
    [SerializeField] private Transform[] weaponSlots; // Slots donde están las armas invisibles
    [SerializeField] private ItemSO[] weaponTypes; // Tipos de armas correspondientes a cada slot
    
    [Header("Events")]
    public UnityEvent<ItemSO> OnItemEquipped;
    public UnityEvent OnItemUnequipped;
    public UnityEvent<ItemSO, int> OnItemAdded;
    public UnityEvent<ItemSO, int> OnItemRemoved;
    
    private InventoryItem[] inventory;
    private int selectedSlot = 0;
    private GameObject currentHandItem;
    private bool[] weaponInInventory; // Track qué armas están en inventario
    
    // Properties
    public ItemSO CurrentItem => inventory != null && inventory[selectedSlot] != null ? inventory[selectedSlot].itemData : null;
    public int CurrentQuantity => inventory != null && inventory[selectedSlot] != null ? inventory[selectedSlot].quantity : 0;
    public bool HasCurrentItem => inventory != null && inventory[selectedSlot] != null && !inventory[selectedSlot].isEmpty;
    public int InventorySize => inventorySize;
    public int GetSelectedSlot() => selectedSlot;
    
    
    void Start()
    {
        InitializeInventory();
        InitializeWeaponSystem();
        EquipItem();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    private void InitializeInventory()
    {
        inventory = new InventoryItem[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            inventory[i] = new InventoryItem();
        }
    }
    
    private void InitializeWeaponSystem()
    {
        // Inicializar array de tracking de armas
        if (weaponSlots != null && weaponTypes != null)
        {
            weaponInInventory = new bool[weaponSlots.Length];
            
            // Hacer todas las armas invisibles al inicio
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null)
                {
                    weaponSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
    
    private void HandleInput()
    {
        // Hotbar selection
        for (int i = 0; i < inventorySize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
        
        // Mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SelectSlot((selectedSlot + 1) % inventorySize);
        }
        else if (scroll < 0f)
        {
            SelectSlot((selectedSlot - 1 + inventorySize) % inventorySize);
        }
        
        // Drop item (solo si esta en el suelo porque si esta en el aire este no tiene el rigidbody ni el box)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IsPlayerGrounded())
            {
                DropCurrentItem();
            }
        }
        
        // Use consumable
        if (Input.GetMouseButtonDown(1))
        {
            UseConsumable();
        }
    }
    
    private void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize) return;
        
        selectedSlot = slotIndex;
        EquipItem();
    }
    
    private void EquipItem()
    {
        // Destroy current hand item (para items no-armas)
        if (currentHandItem != null)
        {
            Destroy(currentHandItem);
            currentHandItem = null;
        }
        
        // Primero desactivar TODAS las armas
        DeactivateAllWeapons();
        
        InventoryItem currentItem = inventory[selectedSlot];
        if (!currentItem.isEmpty)
        {
            // Si es un arma, activar SOLO el arma correspondiente
            if (currentItem.itemData.itemType == ItemType.Weapon)
            {
                ActivateWeaponInHand(currentItem.itemData);
            }
            else
            {
                // Para otros items, usar el sistema normal
                if (currentItem.itemData.prefab != null)
                {
                    currentHandItem = Instantiate(currentItem.itemData.prefab, handParent);
                }
            }
            
            OnItemEquipped?.Invoke(currentItem.itemData);
        }
        else
        {
            // Slot vacío - todas las armas ya están desactivadas
            OnItemUnequipped?.Invoke();
        }
    }
    
    public bool AddItem(ItemSO itemData, int quantity = 1)
    {
        if (itemData == null) return false;
        
        // Try to stack with existing items
        for (int i = 0; i < inventorySize; i++)
        {
            if (!inventory[i].isEmpty && 
                inventory[i].itemData == itemData && 
                inventory[i].quantity < itemData.stackMax)
            {
                int canAdd = itemData.stackMax - inventory[i].quantity;
                int addAmount = Mathf.Min(quantity, canAdd);
                
                inventory[i].quantity += addAmount;
                quantity -= addAmount;
                
                OnItemAdded?.Invoke(itemData, addAmount);
                
                if (quantity <= 0) return true;
            }
        }
        
        // Add to empty slots
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventory[i].isEmpty)
            {
                inventory[i].itemData = itemData;
                inventory[i].quantity = Mathf.Min(quantity, itemData.stackMax);
                quantity -= inventory[i].quantity;
                
                OnItemAdded?.Invoke(itemData, inventory[i].quantity);
                
                if (quantity <= 0) return true;
            }
        }
        
        return quantity <= 0;
    }
    
    public bool RemoveItem(ItemSO itemData, int quantity = 1)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (!inventory[i].isEmpty && inventory[i].itemData == itemData)
            {
                int removeAmount = Mathf.Min(quantity, inventory[i].quantity);
                inventory[i].quantity -= removeAmount;
                quantity -= removeAmount;
                
                OnItemRemoved?.Invoke(itemData, removeAmount);
                
                if (inventory[i].quantity <= 0)
                {
                    inventory[i].itemData = null;
                    inventory[i].quantity = 0;
                }
                
                // Update equipment if removed from current slot
                if (i == selectedSlot)
                {
                    EquipItem();
                }
                
                if (quantity <= 0) return true;
            }
        }
        
        return quantity <= 0;
    }
    
    private void UseConsumable()
    {
        if (HasCurrentItem && CurrentItem.itemType == ItemType.Consumable)
        {
            ItemSO currentItem = CurrentItem;
            ConsumableData consumableData = currentItem.consumableData;
            
            if (consumableData != null)
            {
                PlayerStats playerStats = GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    float healAmount = CalculateHealAmount(consumableData, playerStats);
                    
                    if (healAmount > 0)
                    {
                        playerStats.Heal((int)healAmount);
                    }
                    else if (healAmount < 0)
                    {
                        playerStats.TakeDamage((int)Mathf.Abs(healAmount));
                    }
                }
                
                // Play effects
                if (consumableData.consumeEffect != null)
                {
                    Instantiate(consumableData.consumeEffect, transform.position, Quaternion.identity);
                }
                
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null && consumableData.consumeSound != null)
                {
                    audioSource.PlayOneShot(consumableData.consumeSound);
                }
                
                // Remove item
                RemoveItem(currentItem, 1);
            }
        }
    }
    
    private float CalculateHealAmount(ConsumableData consumableData, PlayerStats playerStats)
    {
        switch (consumableData.consumableType)
        {
            case ConsumableType.SmallHeal:
                return consumableData.healAmount; // Curación fija pequeña
                
            case ConsumableType.LargeHeal:
                return consumableData.healAmount; // Curación fija grande
                
            case ConsumableType.RandomHeal:
                // 50% chance de curación (5-50% de vida máxima) o daño (1-30)
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    // Curación: 5-50% de vida máxima
                    float healPercent = Random.Range(5f, 50f) / 100f;
                    return playerStats.MaxHealth * healPercent;
                }
                else
                {
                    // Daño: 1-30 puntos
                    return -Random.Range(1, 31);
                }
                
            default:
                return consumableData.healAmount;
        }
    }
    
    private void DropCurrentItem()
    {
        if (HasCurrentItem)
        {
            ItemSO currentItem = CurrentItem;
            
            // Drop only 1 item at a time y cerca del jugador
            Vector3 dropPosition = transform.position + transform.forward * 1f;
            
            GameObject droppedItem;
            
            // Si el item tiene prefab, instanciarlo
            if (currentItem.prefab != null)
            {
                droppedItem = Instantiate(currentItem.prefab, dropPosition, Quaternion.identity);
                droppedItem.name = $"Dropped_{currentItem.itemName}";
            }
            else
            {
                // Si no tiene prefab, crear GameObject vacío (para items sin prefab)
                droppedItem = new GameObject($"Dropped_{currentItem.itemName}");
                droppedItem.transform.position = dropPosition;
            }
            
            // Add pickup component
            ItemPickup pickup = droppedItem.AddComponent<ItemPickup>();
            pickup.SetItemData(currentItem, 1, true); // Marcar como item tirado
            
            // Play drop sound
            if (currentItem.dropSound != null)
            {
                AudioSource.PlayClipAtPoint(currentItem.dropSound, dropPosition);
            }
            
            // Si es un arma, desactivarla en la mano
            if (currentItem.itemType == ItemType.Weapon)
            {
                DeactivateWeaponInHand(currentItem);
            }
            
            // Remove 1 item from inventory
            RemoveItem(currentItem, 1);
            
        }
    }
    
    // Public methods for external access
    public InventoryItem GetItem(int slotIndex)
    {
        if (inventory == null) return null;
        if (slotIndex >= 0 && slotIndex < inventorySize)
            return inventory[slotIndex];
        return null;
    }
    
    private bool IsPlayerGrounded()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            CharacterController controller = playerController.GetComponent<CharacterController>();
            if (controller != null)
            {
                return controller.isGrounded;
            }
        }
        return false;
    }
    
    // Métodos para manejar armas pre-posicionadas
    public void ActivateWeaponInHand(ItemSO weaponData)
    {
        if (weaponSlots == null || weaponTypes == null) return;
        
        // Buscar el slot correspondiente a esta arma
        for (int i = 0; i < weaponTypes.Length; i++)
        {
            if (weaponTypes[i] == weaponData && weaponSlots[i] != null)
            {
                weaponSlots[i].gameObject.SetActive(true);
                // No cambiar weaponInInventory aquí - solo activar visualmente
                break;
            }
        }
    }
    
    public void DeactivateWeaponInHand(ItemSO weaponData)
    {
        if (weaponSlots == null || weaponTypes == null) return;
        
        // Buscar el slot correspondiente a esta arma
        for (int i = 0; i < weaponTypes.Length; i++)
        {
            if (weaponTypes[i] == weaponData && weaponSlots[i] != null)
            {
                weaponSlots[i].gameObject.SetActive(false);
                // No cambiar weaponInInventory aquí - solo desactivar visualmente
                break;
            }
        }
    }
    
    private void DeactivateAllWeapons()
    {
        if (weaponSlots == null) return;
        
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
            {
                weaponSlots[i].gameObject.SetActive(false);
            }
        }
    }
    
    public bool IsWeaponInInventory(ItemSO weaponData)
    {
        if (weaponTypes == null || weaponInInventory == null) return false;
        
        for (int i = 0; i < weaponTypes.Length; i++)
        {
            if (weaponTypes[i] == weaponData)
            {
                return weaponInInventory[i];
            }
        }
        return false;
    }
}
