using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float lifetime = 5f;
    
    [Header("Explosion")]
    public GameObject explosionPrefab;
    
    // Valores que se configuran desde WeaponData
    private float speed;
    private int damage;
    private float explosionRadius;
    
    // Variables para configurar desde WeaponData
    private AudioClip fireballSound;
    private AudioClip explosionSound;
    private float fireballVolume;
    private float explosionVolume;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasExploded = false;
    private bool soundPlayed = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 50f;
        
        PlayFireballSound();
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Mantener velocidad constante en la dirección inicial
        if (rb != null && rb.velocity.magnitude < speed)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        
        if (collision.gameObject.CompareTag("Player"))
            return;
        
        Explode();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;
        
        if (other.CompareTag("Player"))
            return;
        
        if (other.CompareTag("Enemy") || other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Explode();
        }
    }
    
    private void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;
        
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }
        
        ApplyAreaDamage();
        PlayExplosionSound();
        Destroy(gameObject);
    }
    
    private void ApplyAreaDamage()
    {
        Collider[] hitObjects = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hitCollider in hitObjects)
        {
            EnemyAdvanced enemy = hitCollider.GetComponent<EnemyAdvanced>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
    
    private void PlayFireballSound()
    {
        if (audioSource != null && fireballSound != null && !soundPlayed)
        {
            audioSource.PlayOneShot(fireballSound, fireballVolume);
            soundPlayed = true;
        }
    }
    
    private void PlayExplosionSound()
    {
        if (audioSource != null && explosionSound != null)
        {
            GameObject tempAudioObject = new GameObject("TempExplosionAudio");
            tempAudioObject.transform.position = transform.position;
            AudioSource tempAudioSource = tempAudioObject.AddComponent<AudioSource>();
            
            tempAudioSource.spatialBlend = 1f;
            tempAudioSource.maxDistance = 50f;
            tempAudioSource.volume = explosionVolume;
            
            tempAudioSource.PlayOneShot(explosionSound);
            Destroy(tempAudioObject, explosionSound.length + 0.5f);
        }
    }
    
    // Métodos para configurar desde el WeaponData
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetExplosionRadius(float newRadius)
    {
        explosionRadius = newRadius;
    }
    
    public void SetFireballSound(AudioClip sound, float volume)
    {
        fireballSound = sound;
        fireballVolume = volume;
    }
    
    public void SetExplosionSound(AudioClip sound, float volume)
    {
        explosionSound = sound;
        explosionVolume = volume;
    }
    
  
}
