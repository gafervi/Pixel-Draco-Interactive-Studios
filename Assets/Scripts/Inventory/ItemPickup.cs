using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemSO itemData;
    [SerializeField] private int quantity = 1;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private InventoryManager inventoryManager;
    private Transform player;
    private PlayerKeys playerKeys;

    private bool isPlayerInRange = false;
    private bool isCollected = false;
    private bool isDroppedItem = false; // Para distinguir items tirados vs encontrados

    void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();

        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            player = playerGO.transform;

            playerKeys = playerGO.GetComponent<PlayerKeys>();
            if (playerKeys == null) playerKeys = playerGO.GetComponentInChildren<PlayerKeys>();
            if (playerKeys == null) playerKeys = playerGO.GetComponentInParent<PlayerKeys>();
        }
    }


    void Update()
    {
        if (isCollected) return;

        CheckPlayerDistance();
        HandleInteraction();
    }

    private void CheckPlayerDistance()
    {
        if (player == null)
        {
            // Reintenta por si el Player spawnea después
            player = FindObjectOfType<PlayerController>()?.transform;
            if (player != null)
                playerKeys = player.GetComponent<PlayerKeys>();
        }

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerInRange = distance <= interactionRange;
        }
        else
        {
            isPlayerInRange = false;
        }
    }

    private void HandleInteraction()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            CollectItem();
        }
    }

    public void CollectItem()
    {
        if (isCollected) return;

        if (itemData == null)
        {
            Debug.LogWarning("[ItemPickup] itemData es NULL. No se puede recoger este objeto.");
            return;
        }

        isCollected = true;

        // --- 1) LLAVES: flujo separado, NO depende del inventario ---
        if (itemData.itemType == ItemType.Key)
        {
            if (playerKeys == null)
            {
                GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null)
                {
                    playerKeys = playerGO.GetComponent<PlayerKeys>();
                    if (playerKeys == null) playerKeys = playerGO.GetComponentInChildren<PlayerKeys>();
                    if (playerKeys == null) playerKeys = playerGO.GetComponentInParent<PlayerKeys>();
                }
            }

            if (playerKeys != null)
            {
                playerKeys.AddKey(quantity);

                if (itemData.pickupSound != null)
                    AudioSource.PlayClipAtPoint(itemData.pickupSound, transform.position);

                Destroy(gameObject);
            }
            else
            {
                isCollected = false;
                Debug.LogWarning("[ItemPickup] No se encontró PlayerKeys en el Player (Tag: Player). No se sumaron llaves.");
            }

            return;

        }

        // --- 2) RESTO DE ITEMS: requieren inventario ---
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();

        if (inventoryManager == null)
        {
            isCollected = false;
            Debug.LogWarning("[ItemPickup] InventoryManager no encontrado. No se recogió el item.");
            return;
        }

        bool success = inventoryManager.AddItem(itemData, quantity);
        if (!success)
        {
            isCollected = false;
            Debug.LogWarning("[ItemPickup] No se pudo agregar el item al inventario.");
            return;
        }

        // Sonido pickup
        if (itemData.pickupSound != null)
            AudioSource.PlayClipAtPoint(itemData.pickupSound, transform.position);

        // Si es arma, activar en mano
        if (itemData.itemType == ItemType.Weapon)
            inventoryManager.ActivateWeaponInHand(itemData);

        Destroy(gameObject);
    }


    public void SetItemData(ItemSO item, int qty = 1, bool dropped = false)
    {
        itemData = item;
        quantity = qty;
        isDroppedItem = dropped;
    }

    public ItemSO GetItemData() => itemData;
    public int GetQuantity() => quantity;
    public bool IsPlayerInRange() => isPlayerInRange;
    public bool IsDroppedItem() => isDroppedItem;
}
