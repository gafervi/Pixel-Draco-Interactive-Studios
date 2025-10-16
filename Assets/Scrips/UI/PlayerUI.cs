using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Health UI")]
    [SerializeField] private Image healthBarBackground; 
    [SerializeField] private Image healthBarFill; // Rectángulo rojo que actúa como barra
    [SerializeField] private Text healthText;
    
    [Header("Coins UI")]
    [SerializeField] private Text coinsText;
    
    private PlayerStats playerStats;
    
    // Variables para optimizar rendimiento
    private float lastHealthPercentage = -1f;
    private int lastCoins = -1;
    
    void Start()
    {
        // Find player components
        playerStats = FindObjectOfType<PlayerStats>();
        
        if (playerStats != null)
        {
            // Subscribe to events
            playerStats.OnHealthChanged.AddListener(UpdateHealthUI);
            playerStats.OnCoinsChanged.AddListener(UpdateCoinsUI);
            
            // Initialize UI
            UpdateHealthUI(playerStats.HealthPercentage);
            UpdateCoinsUI(playerStats.Coins);
        }
        else
        {
            Debug.LogError("PlayerUI: No se encontró PlayerStats. La UI no funcionará correctamente.");
        }
    }
    
    void UpdateHealthUI(float healthPercentage)
    {
        // Solo actualizar si el valor realmente cambió (optimización de rendimiento)
        if (Mathf.Approximately(lastHealthPercentage, healthPercentage))
            return;
            
        lastHealthPercentage = healthPercentage;
        
        if (healthBarFill != null)
        {
            // Cambiar el ancho de la barra usando rectTransform para mejor control
            RectTransform fillRect = healthBarFill.rectTransform;
            RectTransform backgroundRect = healthBarBackground.rectTransform;
            
            // Calcular el nuevo ancho basado en el porcentaje de vida
            float newWidth = backgroundRect.rect.width * healthPercentage;
            fillRect.sizeDelta = new Vector2(newWidth, fillRect.sizeDelta.y);
        }
        
        if (healthText != null && playerStats != null)
        {
            healthText.text = $"{Mathf.RoundToInt(playerStats.Health)}/{Mathf.RoundToInt(playerStats.MaxHealth)}";
        }
    }
    
    void UpdateCoinsUI(int currentCoins)
    {
        // Solo actualizar si el valor realmente cambió (optimización de rendimiento)
        if (lastCoins == currentCoins)
            return;
            
        lastCoins = currentCoins;
        
        if (coinsText != null)
        {
            coinsText.text = $"{currentCoins}";
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerStats != null)
        {
            playerStats.OnHealthChanged.RemoveListener(UpdateHealthUI);
            playerStats.OnCoinsChanged.RemoveListener(UpdateCoinsUI);
        }
    }
}
