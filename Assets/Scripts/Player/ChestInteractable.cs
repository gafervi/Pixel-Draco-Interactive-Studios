using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ChestLoot
{
    [Header("Item Settings")]
    [Tooltip("Prefab del item de la carpeta ItemsDown")]
    public GameObject itemPrefab;
    [Range(0f, 100f)]
    [Tooltip("Probability of this item appearing (0-100%)")]
    public float probability = 100f;
}

public class ChestInteractable : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private bool isOpened = false;
    [Tooltip("Tiempo de espera antes de que aparezcan los items (en segundos)")]
    [SerializeField] private float lootDropDelay = 1.5f;

    [Header("Loot Configuration")]
    [SerializeField] private List<ChestLoot> possibleLoot = new List<ChestLoot>();
    [SerializeField] private bool onlyOneItemPerChest = true;

    [Header("Drop Settings")]
    [SerializeField] private Transform[] dropPoints; // Puntos donde pueden aparecer los items (si está vacío, usa posición por defecto)
    [SerializeField] private float dropOffset = 0.5f; // Offset desde el cofre para dropear items
    [SerializeField] private Vector3 dropDirection = Vector3.forward; // Dirección para dropear items

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lootDropSound;

    private Transform player;
    private bool isPlayerInRange = false;
    private bool hasGeneratedLoot = false; // Para evitar generar loot múltiples veces

    void Start()
    {
        player = FindObjectOfType<PlayerController>()?.transform;

        // Si el cofre ya está abierto, ya no se puede interactuar
        if (isOpened)
        {
            hasGeneratedLoot = true;
        }
    }

    void Update()
    {
        if (isOpened && hasGeneratedLoot) return; // Si ya está abierto y ya dio loot, no hacer nada

        CheckPlayerDistance();
        HandleInteraction();
    }

    private void CheckPlayerDistance()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerInRange = distance <= interactionRange;
        }
    }

    private void HandleInteraction()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isOpened)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (isOpened) return;

        isOpened = true;
        hasGeneratedLoot = true;

        // Reproducir sonido de apertura
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }

        // Generar y dropear loot después del delay configurado
        Invoke(nameof(GenerateAndDropLoot), lootDropDelay);
    }

    private void GenerateAndDropLoot()
    {
        if (possibleLoot == null || possibleLoot.Count == 0)
        {
            Debug.LogWarning($"Chest at {transform.position} has no loot configured!");
            return;
        }

        List<ChestLoot> selectedLoot = new List<ChestLoot>();

        if (onlyOneItemPerChest)
        {
            // Seleccionar solo un item basado en probabilidades
            ChestLoot selected = SelectRandomLootByProbability();
            if (selected != null && selected.itemPrefab != null)
            {
                selectedLoot.Add(selected);
            }
        }
        else
        {
            // Seleccionar múltiples items basado en probabilidades
            foreach (ChestLoot loot in possibleLoot)
            {
                if (loot.itemPrefab != null && Random.Range(0f, 100f) <= loot.probability)
                {
                    selectedLoot.Add(loot);
                }
            }
        }

        // Dropear los items seleccionados
        foreach (ChestLoot loot in selectedLoot)
        {
            DropLootItem(loot.itemPrefab);
        }

        // Reproducir sonido de loot si hay items
        if (selectedLoot.Count > 0 && lootDropSound != null)
        {
            AudioSource.PlayClipAtPoint(lootDropSound, transform.position);
        }
    }

    private ChestLoot SelectRandomLootByProbability()
    {
        // Calcular total de probabilidades
        float totalProbability = 0f;
        foreach (ChestLoot loot in possibleLoot)
        {
            if (loot.itemPrefab != null)
            {
                totalProbability += loot.probability;
            }
        }

        if (totalProbability <= 0f) return null;

        // Seleccionar un item aleatorio basado en probabilidades
        float randomValue = Random.Range(0f, totalProbability);
        float currentSum = 0f;

        foreach (ChestLoot loot in possibleLoot)
        {
            if (loot.itemPrefab != null)
            {
                currentSum += loot.probability;
                if (randomValue <= currentSum)
                {
                    return loot;
                }
            }
        }

        // Fallback: devolver el último item válido
        for (int i = possibleLoot.Count - 1; i >= 0; i--)
        {
            if (possibleLoot[i].itemPrefab != null)
            {
                return possibleLoot[i];
            }
        }

        return null;
    }

    private void DropLootItem(GameObject itemPrefab)
    {
        if (itemPrefab == null) return;

        // Calcular posición de drop
        Vector3 dropPosition = GetDropPosition();

        // Instanciar el prefab del item (ya tiene ItemPickup configurado)
        GameObject droppedItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);

        // El prefab ya debería tener ItemPickup, Rigidbody y Collider configurados
        // Pero verificamos y aplicamos fuerza si tiene Rigidbody

        // Aplicar una pequeña fuerza aleatoria para que se vea más natural (tipo Minecraft)
        Rigidbody rbComponent = droppedItem.GetComponent<Rigidbody>();
        if (rbComponent != null)
        {
            Vector3 randomForce = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(1f, 2f),
                Random.Range(-0.5f, 0.5f)
            );
            rbComponent.AddForce(randomForce, ForceMode.Impulse);
        }
    }

    private Vector3 GetDropPosition()
    {
        // Si hay dropPoints configurados, usar uno aleatorio
        if (dropPoints != null && dropPoints.Length > 0)
        {
            Transform randomPoint = dropPoints[Random.Range(0, dropPoints.Length)];
            return randomPoint.position;
        }

        // Si no, calcular posición basada en dropDirection y dropOffset
        Vector3 direction = transform.TransformDirection(dropDirection.normalized);
        return transform.position + direction * dropOffset + Vector3.up * 0.2f; // Un poco arriba para que caiga
    }

    // Método público para verificar si el jugador está en rango (útil para UI)
    public bool IsPlayerInRange()
    {
        return isPlayerInRange && !isOpened;
    }

    // Método público para obtener el nombre del cofre (útil para UI)
    public string GetChestName()
    {
        return "Chest";
    }

    // Gizmos para visualizar el rango de interacción en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Dibujar dirección de drop
        Gizmos.color = Color.green;
        Vector3 dropPos = GetDropPosition();
        Gizmos.DrawLine(transform.position, dropPos);
        Gizmos.DrawWireSphere(dropPos, 0.2f);
    }
}


