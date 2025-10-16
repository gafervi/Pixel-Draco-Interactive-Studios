using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerInput.MainActions input;

    CharacterController controller;
    Animator animator;
    AudioSource audioSource;
    AudioSource torchLoopAudioSource;  // loop de sonido de la antorcha 
    AudioSource footstepsAudioSource; // AudioSource para los pasos 
    PlayerStats playerStats; // se llama al code de las stats
    InventoryManager inventoryManager; 

    // controller es el componente de movimiento
    [Header("Controller")]
    public float moveSpeed = 5;
    public float sprintSpeed = 8;
    public float gravity = -9.8f;
    public float jumpHeight = 1.2f;
    
    // el code para correr
    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public KeyCode runningKey = KeyCode.LeftShift;

    Vector3 _PlayerVelocity;

    // el code para el suelo
    bool isGrounded;
    bool torchActive = false;
    bool isMoving = false;

    [Header("Camera")]
    public Camera cam;
    public float sensitivity;

    float xRotation = 0f;

     [Header("Torch")]
    public GameObject torchObject;
    public AudioClip torchLoopSound;  // Sonido en loop mientras esta activo
    [Range(0f, 1f)]
    public float torchVolume = 0.5f;  // Volumendel sonido de antorcha


    [Header("Footsteps")]
    public AudioClip footstepsLoopSound;  // Sonido de pasos en loop igual 
    [Range(0f, 1f)]
    public float footstepsVolume = 0.3f;  // Volumen de los pasos

    // luz basica porque no se podian ver las armas en la oscuridad
    [Header("Player Ambient Light")]
    public Light playerAmbientLight;
    public float normalLightIntensity = 0.5f;
    public float torchLightIntensity = 2f;
    
    // code para el combate
    [Header("Combat Settings")]
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private Camera combatCam;
    
    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount = 0;

    void Awake()
    { 
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerStats = GetComponent<PlayerStats>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        
        // Setup torch loop audio 
        torchLoopAudioSource = gameObject.AddComponent<AudioSource>();
        torchLoopAudioSource.loop = true;
        torchLoopAudioSource.playOnAwake = false;
        torchLoopAudioSource.clip = torchLoopSound;
        torchLoopAudioSource.volume = torchVolume;

        // Setup footsteps audio 
        footstepsAudioSource = gameObject.AddComponent<AudioSource>();
        footstepsAudioSource.loop = true;
        footstepsAudioSource.playOnAwake = false;
        footstepsAudioSource.volume = footstepsVolume;

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // empieza la antorcha
        if (torchObject != null)
            torchObject.SetActive(false);
            
        // Initialize player ambient light
        if (playerAmbientLight != null)
        {
            playerAmbientLight.intensity = normalLightIntensity;
        }
        
        // Setup combat camera
        if (combatCam == null)
            combatCam = cam;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        HandleFootsteps();
        HandleJump();
        HandleCombat();
        SetAnimations();
    }
    
    private void HandleFootsteps()
    {
        // revisa si el jugador está moviendose
        Vector2 movementInput = input.Movement.ReadValue<Vector2>();
        bool wasMoving = isMoving;
        isMoving = isGrounded && (Mathf.Abs(movementInput.x) > 0.1f || Mathf.Abs(movementInput.y) > 0.1f);

        // aqui se controlar el loop de pasos
        if (isMoving && footstepsLoopSound != null)
        {
            if (!footstepsAudioSource.isPlaying)
            {
                footstepsAudioSource.clip = footstepsLoopSound;
                footstepsAudioSource.Play();
            }
        }
        else
        {
            if (footstepsAudioSource.isPlaying)
            {
                footstepsAudioSource.Stop();
            }
        }
    }
    
    private void HandleJump()
    {
        // para ver si se presiona la tecla de salto
        if (input.Jump.WasPressedThisFrame() && isGrounded)
        {
            _PlayerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

    // code para el combate
    private void HandleCombat()
    {
        if (inventoryManager != null && inventoryManager.HasCurrentItem)
        {
            ItemSO currentItem = inventoryManager.CurrentItem;
            
            if (currentItem != null && currentItem.itemType == ItemType.Weapon && currentItem.weaponData != null)
            {
                if (Input.GetMouseButtonDown(0) && readyToAttack && !attacking)
                {
                    AttackWithWeapon(currentItem.weaponData);
                }
            }
        }
    }

    // code para atacar con el arma porque me dio problemas de crear las animaciones por armas porque son de los movimientos de las manos
    private void AttackWithWeapon(WeaponData weaponData)
    {
        readyToAttack = false;
        attacking = true;

        // Solo usar animaciones si el arma lo permite esto tambien para que los objetos no hagan como si atacaran
        if (weaponData.useAttackAnimations)
        {
            // Alternar entre Attack 1 y Attack 2
            if (attackCount == 0)
            {
                ChangeAnimationState(ATTACK_1);
                attackCount++;
            }
            else
            {
                ChangeAnimationState(ATTACK_2);
                attackCount = 0;
            }
        }
        // Para no hacehr animaciones 
        Invoke(nameof(ResetAttack), weaponData.attackSpeed);
        
        // para el arma que dispara protectiles 
        if (weaponData.weaponType == WeaponType.Magic && weaponData.magicProjectilePrefab != null)
        {
            ShootMagicProjectile(weaponData);
        }
        else
        {
            Invoke(nameof(AttackRaycast), weaponData.attackDelay);
        }
        
        if (audioSource != null && weaponData.weaponSwing != null)
        {
            audioSource.PlayOneShot(weaponData.weaponSwing);
        }
    }
    
    private void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }
    
    private void ShootMagicProjectile(WeaponData weaponData)
    {
        if (weaponData.magicProjectilePrefab != null)
        {
            // Crear proyectil desde la posicion de la camara
            Vector3 spawnPosition = combatCam.transform.position + combatCam.transform.forward * 1.2f + Vector3.down * 0.2f;

            // Calcular direccion donde esta mirando el jugador
            Vector3 fireDirection = GetFireDirection();
            
            GameObject projectile = Instantiate(weaponData.magicProjectilePrefab, spawnPosition, Quaternion.LookRotation(fireDirection));
            
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            if (projectileRb != null)
            {
                projectileRb.velocity = fireDirection * weaponData.projectileSpeed;
            }
            
            // Conf de la bola de fuego
            FireballProjectile fireballScript = projectile.GetComponent<FireballProjectile>();
            if (fireballScript != null)
            {
                fireballScript.SetDamage(weaponData.attackDamage);
                fireballScript.SetSpeed(weaponData.projectileSpeed);
                fireballScript.SetExplosionRadius(weaponData.explosionRadius);
                fireballScript.SetFireballSound(weaponData.fireballSound, weaponData.fireballVolume);
                fireballScript.SetExplosionSound(weaponData.magicExplosionSound, weaponData.explosionVolume);
            }
        }
    }
    
    private Vector3 GetFireDirection()
    {
        // Raycast desde la cámara hacia el centro de la pantalla (crosshair)
        Ray ray = combatCam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        RaycastHit hit;
        
        Vector3 fireDirection;

        if (Physics.Raycast(ray, out hit, 1000f))
        {
            // problema que tenia el codigo viejo que estaba haciendo como una variacion en el disparo que
            // este si el objeto o cosa estaba muy cerca del jugador disparaba mas alto o mas bajo pero era tan facil como normalizar el vector en  la camara
            fireDirection = (hit.point - combatCam.transform.position).normalized;

        }
        else
        {
            fireDirection = combatCam.transform.forward;
        }
        
        if (fireDirection.magnitude < 0.1f)
        {
            fireDirection = combatCam.transform.forward;
        }
        
        return fireDirection;
    }
    
    private void AttackRaycast()
    {
        if (inventoryManager != null && inventoryManager.HasCurrentItem)
        {
            ItemSO currentItem = inventoryManager.CurrentItem;
            
            if (currentItem != null && currentItem.itemType == ItemType.Weapon && currentItem.weaponData != null)
            {
                WeaponData weaponData = currentItem.weaponData;
                
                // Raycast desde la camara hacia adelante
                Ray ray = combatCam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, weaponData.attackDistance, attackLayer))
                {
                    // Aplicar daño al enemigo
                    EnemyAdvanced enemy = hit.collider.GetComponent<EnemyAdvanced>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(weaponData.attackDamage);
                        
                        // Efecto visual de golpe
                        if (weaponData.hitEffect != null)
                        {
                            Instantiate(weaponData.hitEffect, hit.point, Quaternion.identity);
                        }
                        
                        // Sonido de golpe
                        if (audioSource != null && weaponData.hitSound != null)
                        {
                            audioSource.PlayOneShot(weaponData.hitSound);
                        }
                    }
                }
            }
        }
    }

    void FixedUpdate() 
    { MoveInput(input.Movement.ReadValue<Vector2>()); }

    void LateUpdate() 
    { LookInput(input.Look.ReadValue<Vector2>()); }

    void MoveInput(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        // Update IsRunning from input (simple method)
        IsRunning = canRun && Input.GetKey(runningKey);
        
        // Determine current speed based on running
        float currentSpeed = IsRunning ? sprintSpeed : moveSpeed;
        

        controller.Move(transform.TransformDirection(moveDirection) * currentSpeed * Time.deltaTime);
        _PlayerVelocity.y += gravity * Time.deltaTime;
        if(isGrounded && _PlayerVelocity.y < 0)
            _PlayerVelocity.y = -2f;
        controller.Move(_PlayerVelocity * Time.deltaTime);
    }

    void LookInput(Vector3 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime * sensitivity);
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * sensitivity));
    }

    void OnEnable() 
    { input.Enable(); }

    void OnDisable()
    { 
        input.Disable();
        
        // Stop torch loop sound when object is disabled
        if (torchLoopAudioSource != null)
        {
            torchLoopAudioSource.Stop();
        }
        
        // Stop footsteps sound when object is disabled
        if (footstepsAudioSource != null)
        {
            footstepsAudioSource.Stop();
        }
    }

    void AssignInputs()
    {
        input.Torch.performed += ctx => ToggleTorch();
    }

    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string SPRINT = "Sprint";
    public const string ATTACK_1 = "Attack 1";
    public const string ATTACK_2 = "Attack 2";

    string currentAnimationState;

    public void ChangeAnimationState(string newState) 
    {
        if (currentAnimationState == newState) return;

        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        if (!attacking || (inventoryManager != null && inventoryManager.HasCurrentItem && 
            inventoryManager.CurrentItem.weaponData != null && 
            !inventoryManager.CurrentItem.weaponData.useAttackAnimations))
        {
            if(_PlayerVelocity.x == 0 &&_PlayerVelocity.z == 0)
            { ChangeAnimationState(IDLE); }
            else
            { ChangeAnimationState(WALK); }
        }
    }

    void ToggleTorch()
    {
        torchActive = !torchActive;
        
        if (torchObject != null)
            torchObject.SetActive(torchActive);
            
        // Update player ambient light intensity
        if (playerAmbientLight != null)
        {
            playerAmbientLight.intensity = torchActive ? torchLightIntensity : normalLightIntensity;
        }

        // Control torch loop sound
        if (torchLoopAudioSource != null && torchLoopSound != null)
        {
            if (torchActive)
            {
                torchLoopAudioSource.Play();
            }
            else
            {
                torchLoopAudioSource.Stop();
            }
        }
    }

    void OnDestroy()
    {
        // Stop torch loop sound when object is destroyed
        if (torchLoopAudioSource != null)
        {
            torchLoopAudioSource.Stop();
        }
        
        // Stop footsteps sound when object is destroyed
        if (footstepsAudioSource != null)
        {
            footstepsAudioSource.Stop();
        }
    }

    // Public methods for UI
    public bool IsTorchActive()
    {
        return torchActive;
    }
    
    // Public method to check if running
    public bool IsPlayerRunning()
    {
        return IsRunning;
    }
    
    // Method called when player dies
    public void OnPlayerDeath()
    {
        // Desactivar movimientos
        enabled = false;
        
        // Liberar y mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}