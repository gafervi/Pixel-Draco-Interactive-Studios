using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject[] hotbarSlots;
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private Text interactionText;
    
    [Header("Inventory Full UI")]
    [SerializeField] private GameObject inventoryFullUI;
    [SerializeField] private Text inventoryFullText;
    
    [Header("Slot Content References")]
    [SerializeField] private Image[] slotIcons;        // Referencias directas a los iconos
    [SerializeField] private Text[] slotQuantities;   // Referencias directas a los textos de cantidad
    
    [Header("Selection Settings")]
    [SerializeField] private string selectionChildName = "Selection"; // Nombre del objeto hijo que muestra selección
    
    private InventoryManager inventoryManager;
    private ItemPickup currentPickup;
    
    void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        
        if (inventoryManager != null)
        {
            inventoryManager.OnItemAdded.AddListener(OnItemAdded);
            inventoryManager.OnItemRemoved.AddListener(OnItemRemoved);
            inventoryManager.OnItemEquipped.AddListener(OnItemEquipped);
            inventoryManager.OnItemUnequipped.AddListener(OnItemUnequipped);
            
            // Only update slots if inventory manager is found
            UpdateAllSlots();
        }
        else
        {
            Debug.LogWarning("InventoryManager not found! InventoryUI will not work properly.");
        }
        
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
        
        if (inventoryFullUI != null)
        {
            inventoryFullUI.SetActive(false);
        }
    }
    
    void Update()
    {
        UpdateHotbarUI();
        UpdateInteractionUI();
        UpdateInventoryFullUI();
    }
    
    private void UpdateHotbarUI()
    {
        if (inventoryManager == null) return;
        
        for (int i = 0; i < inventoryManager.InventorySize; i++)
        {
            UpdateSlotVisual(i);
        }
    }
    
    private void UpdateSlotVisual(int slotIndex)
    {
        if (slotIndex >= hotbarSlots.Length || inventoryManager == null) return;
        
        GameObject slot = hotbarSlots[slotIndex];
        InventoryItem item = inventoryManager.GetItem(slotIndex);
        
        if (item == null) return; // Additional safety check
        
        // Usar referencias directas si están configuradas, sino buscar automáticamente
        Image iconImage = null;
        Text quantityText = null;
        
        if (slotIcons != null && slotIndex < slotIcons.Length)
            iconImage = slotIcons[slotIndex];
        else
            iconImage = slot.GetComponentInChildren<Image>();
            
        if (slotQuantities != null && slotIndex < slotQuantities.Length)
            quantityText = slotQuantities[slotIndex];
        else
            quantityText = slot.GetComponentInChildren<Text>();
        
        if (!item.isEmpty)
        {
            if (iconImage != null)
            {
                iconImage.sprite = item.itemData.icon;
                iconImage.gameObject.SetActive(true);
            }
            
            if (quantityText != null)
            {
                if (item.itemData.stackMax > 1)
                {
                    quantityText.text = item.quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // Slot vacío - ocultar contenido pero mantener el slot visible
            if (iconImage != null)
                iconImage.gameObject.SetActive(false);
            if (quantityText != null)
                quantityText.gameObject.SetActive(false);
        }
        
        // Update selection highlight using child objects (design-based)
        bool isSelected = slotIndex == inventoryManager.GetSelectedSlot();
        
        // Buscar objeto hijo de selección
        Transform selectionChild = slot.transform.Find(selectionChildName);
        if (selectionChild != null)
        {
            selectionChild.gameObject.SetActive(isSelected);
        }
        else
        {
            // Fallback: buscar cualquier objeto hijo que contenga "Selection" en el nombre
            foreach (Transform child in slot.transform)
            {
                if (child.name.ToLower().Contains("selection") || child.name.ToLower().Contains("selected"))
                {
                    child.gameObject.SetActive(isSelected);
                    break;
                }
            }
        }
    }
    
    private void UpdateInteractionUI()
    {
        if (interactionUI == null) return;
        
        // Find nearest pickup item
        ItemPickup nearestPickup = FindNearestPickup();
        
        if (nearestPickup != null && nearestPickup.IsPlayerInRange())
        {
            currentPickup = nearestPickup;
            interactionUI.SetActive(true);
            
            if (interactionText != null)
            {
                ItemSO itemData = nearestPickup.GetItemData();
                int quantity = nearestPickup.GetQuantity();
                
                if (quantity > 1)
                {
                    interactionText.text = $"'E' {itemData.itemName} x{quantity}";
                }
                else
                {
                    interactionText.text = $"'E' {itemData.itemName}";
                }
            }
        }
        else
        {
            currentPickup = null;
            interactionUI.SetActive(false);
        }
    }
    
    private ItemPickup FindNearestPickup()
    {
        ItemPickup[] pickups = FindObjectsOfType<ItemPickup>();
        ItemPickup nearest = null;
        float nearestDistance = float.MaxValue;
        
        Transform player = FindObjectOfType<PlayerController>()?.transform;
        if (player == null) return null;
        
        foreach (ItemPickup pickup in pickups)
        {
            float distance = Vector3.Distance(player.position, pickup.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = pickup;
            }
        }
        
        return nearest;
    }
    
    private void UpdateInventoryFullUI()
    {
        if (inventoryFullUI == null || inventoryManager == null) return;
        
        // Find nearest pickup item
        ItemPickup nearestPickup = FindNearestPickup();
        
        if (nearestPickup != null && nearestPickup.IsPlayerInRange())
        {
            // Check if inventory is full
            if (IsInventoryFull())
            {
                inventoryFullUI.SetActive(true);
                
                if (inventoryFullText != null)
                {
                    inventoryFullText.text = "Inventario lleno!!!";
                }
            }
            else
            {
                inventoryFullUI.SetActive(false);
            }
        }
        else
        {
            inventoryFullUI.SetActive(false);
        }
    }
    
    private bool IsInventoryFull()
    {
        if (inventoryManager == null) return false;
        
        for (int i = 0; i < inventoryManager.InventorySize; i++)
        {
            InventoryItem item = inventoryManager.GetItem(i);
            if (item == null || item.isEmpty)
            {
                return false; // Found empty slot
            }
        }
        
        return true; // All slots are full
    }
    
    private void UpdateAllSlots()
    {
        if (inventoryManager == null) return;
        
        for (int i = 0; i < inventoryManager.InventorySize; i++)
        {
            UpdateSlotVisual(i);
        }
    }
    
    // Event handlers
    private void OnItemAdded(ItemSO item, int quantity)
    {
        UpdateAllSlots();
    }
    
    private void OnItemRemoved(ItemSO item, int quantity)
    {
        UpdateAllSlots();
    }
    
    private void OnItemEquipped(ItemSO item)
    {
        UpdateAllSlots();
    }
    
    private void OnItemUnequipped()
    {
        UpdateAllSlots();
    }
}
