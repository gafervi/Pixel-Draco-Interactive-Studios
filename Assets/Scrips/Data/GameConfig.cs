using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Configuration")]
public class GameConfig : ScriptableObject
{
    [Header("Level Settings")]
    public int maxLevels = 10;
    public float difficultyMultiplier = 1.2f;
    
    [Header("Coin Settings")]
    public int bossCoinReward = 100;
    public int minEnemyCoinReward = 2;
    public int maxEnemyCoinReward = 10;
    public int minChestCoinReward = 5;
    public int maxChestCoinReward = 25;
    
    [Header("Difficulty Scaling")]
    public float healthScaling = 0.2f;
    public float damageScaling = 0.15f;
    public float speedScaling = 0.1f;
    public float countScaling = 0.1f;
    
    [Header("Transition Settings")]
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    
    [Header("Audio Settings")]
    public AudioClip coinCollectSound;
    public AudioClip chestOpenSound;
    public AudioClip portalSound;
    public AudioClip upgradeSound;
    
    [Header("Visual Effects")]
    public GameObject coinCollectEffect;
    public GameObject chestOpenEffect;
    public GameObject upgradeEffect;
    
    [Header("Prefabs")]
    public GameObject coinPrefab;
    public GameObject portalPrefab;
    public GameObject chestPrefab;
}
