using UnityEngine;

/// <summary>
/// Script helper para configurar el sistema de armas pre-posicionadas
/// Coloca este script en el jugador para facilitar la configuración
/// </summary>
public class WeaponSetupHelper : MonoBehaviour
{
    [Header("Configuración de Armas")]
    [SerializeField] private Transform[] weaponSlots;
    [SerializeField] private ItemSO[] weaponTypes;
    
    [Header("Referencias")]
    [SerializeField] private InventoryManager inventoryManager;
    
    void Start()
    {
        if (inventoryManager == null)
        {
            inventoryManager = GetComponent<InventoryManager>();
        }
        
        SetupWeaponSystem();
    }
    
    private void SetupWeaponSystem()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager no encontrado!");
            return;
        }
        
        // Configurar el sistema de armas en el InventoryManager
        // Esto se hace automáticamente, pero puedes usar este script para debug
        
        Debug.Log("Sistema de armas configurado:");
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null && weaponTypes[i] != null)
            {
                Debug.Log($"Slot {i}: {weaponTypes[i].itemName} -> {weaponSlots[i].name}");
            }
        }
    }
    
    [ContextMenu("Test Activate All Weapons")]
    public void TestActivateAllWeapons()
    {
        if (inventoryManager == null) return;
        
        for (int i = 0; i < weaponTypes.Length; i++)
        {
            if (weaponTypes[i] != null)
            {
                inventoryManager.ActivateWeaponInHand(weaponTypes[i]);
            }
        }
    }
    
    [ContextMenu("Test Deactivate All Weapons")]
    public void TestDeactivateAllWeapons()
    {
        if (inventoryManager == null) return;
        
        for (int i = 0; i < weaponTypes.Length; i++)
        {
            if (weaponTypes[i] != null)
            {
                inventoryManager.DeactivateWeaponInHand(weaponTypes[i]);
            }
        }
    }
    
    [ContextMenu("Add All Weapons to Inventory")]
    public void AddAllWeaponsToInventory()
    {
        if (inventoryManager == null) return;
        
        for (int i = 0; i < weaponTypes.Length; i++)
        {
            if (weaponTypes[i] != null)
            {
                inventoryManager.AddItem(weaponTypes[i], 1);
            }
        }
    }
}
