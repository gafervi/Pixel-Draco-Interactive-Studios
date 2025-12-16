
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerInput.MainActions input;

    CharacterController controller;
    Animator animator;
    AudioSource audioSource;
    AudioSource torchLoopAudioSource;
    AudioSource footstepsAudioSource;
    PlayerStats playerStats;
    InventoryManager inventoryManager;

    [Header("Controller")]
    public float moveSpeed = 5;
    public float sprintSpeed = 8;
    public float gravity = -9.8f;
    public float jumpHeight = 1.2f;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public KeyCode runningKey = KeyCode.LeftShift;

    Vector3 _PlayerVelocity;
    bool isGrounded;
    bool torchActive = false;
    bool isMoving = false;

    [Header("Camera")]
    public Camera cam;
    public float sensitivity;
    float xRotation = 0f;

    [Header("Torch")]
    public GameObject torchObject;
    public AudioClip torchLoopSound;
    [Range(0f, 1f)] public float torchVolume = 0.5f;

    [Header("Footsteps")]
    public AudioClip footstepsLoopSound;
    [Range(0f, 1f)] public float footstepsVolume = 0.3f;

    [Header("Player Ambient Light")]
    public Light playerAmbientLight;
    public float normalLightIntensity = 0.5f;
    public float torchLightIntensity = 2f;

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

        torchLoopAudioSource = gameObject.AddComponent<AudioSource>();
        torchLoopAudioSource.loop = true;
        torchLoopAudioSource.playOnAwake = false;
        torchLoopAudioSource.clip = torchLoopSound;
        torchLoopAudioSource.volume = torchVolume;

        footstepsAudioSource = gameObject.AddComponent<AudioSource>();
        footstepsAudioSource.loop = true;
        footstepsAudioSource.playOnAwake = false;
        footstepsAudioSource.volume = footstepsVolume;

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (torchObject != null)
            torchObject.SetActive(false);

        if (playerAmbientLight != null)
            playerAmbientLight.intensity = normalLightIntensity;

        if (combatCam == null)
            combatCam = cam;

        if (playerStats != null)
        {
            playerStats.OnHealthChanged.AddListener(OnHealthChanged);
            playerStats.OnPlayerDeath.AddListener(OnPlayerDeath);
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        HandleFootsteps();
        HandleJump();
        HandleCombat();
        SetAnimations();
    }

    // ================= COMBAT =================

    private void HandleCombat()
    {
        if (!readyToAttack || attacking) return;
        if (!Input.GetMouseButtonDown(0)) return;
        if (inventoryManager == null || !inventoryManager.HasCurrentItem) return;

        ItemSO item = inventoryManager.CurrentItem;
        if (item == null || item.itemType != ItemType.Weapon || item.weaponData == null) return;

        AttackWithWeapon(item.weaponData);
    }

    private void AttackWithWeapon(WeaponData weaponData)
    {
        readyToAttack = false;
        attacking = true;

        if (weaponData.useAttackAnimations)
        {
            ChangeAnimationState(attackCount == 0 ? ATTACK_1 : ATTACK_2);
            attackCount = 1 - attackCount;
        }

        AttackRaycast(weaponData); // 💥 DAÑO INMEDIATO

        Invoke(nameof(ResetAttack), weaponData.attackSpeed);

        if (audioSource && weaponData.weaponSwing)
            audioSource.PlayOneShot(weaponData.weaponSwing);
    }

    private void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    private void AttackRaycast(WeaponData weaponData)
    {
        Ray ray = combatCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.attackDistance, attackLayer))
        {
            EnemyAdvanced enemy = hit.collider.GetComponent<EnemyAdvanced>();
            if (enemy != null)
            {
                enemy.TakeDamage(weaponData.attackDamage);
                Debug.Log("💥 Enemigo eliminado");

                if (weaponData.hitEffect)
                    Instantiate(weaponData.hitEffect, hit.point, Quaternion.identity);

                if (audioSource && weaponData.hitSound)
                    audioSource.PlayOneShot(weaponData.hitSound);
            }
        }
    }

    // ================= MOVEMENT =================

    void FixedUpdate()
    {
        MoveInput(input.Movement.ReadValue<Vector2>());
    }

    void LateUpdate()
    {
        LookInput(input.Look.ReadValue<Vector2>());
    }

    void MoveInput(Vector2 inputVec)
    {
        Vector3 move = new Vector3(inputVec.x, 0, inputVec.y);
        IsRunning = canRun && Input.GetKey(runningKey);
        float speed = IsRunning ? sprintSpeed : moveSpeed;

        controller.Move(transform.TransformDirection(move) * speed * Time.deltaTime);
        _PlayerVelocity.y += gravity * Time.deltaTime;

        if (isGrounded && _PlayerVelocity.y < 0)
            _PlayerVelocity.y = -2f;

        controller.Move(_PlayerVelocity * Time.deltaTime);
    }

    void LookInput(Vector2 inputVec)
    {
        xRotation -= inputVec.y * Time.deltaTime * sensitivity;
        xRotation = Mathf.Clamp(xRotation, -80, 80);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * inputVec.x * Time.deltaTime * sensitivity);
    }

    // ================= ANIMATIONS =================

    public const string IDLE = "Idle";
    public const string WALK = "Walk";
    public const string ATTACK_1 = "Attack 1";
    public const string ATTACK_2 = "Attack 2";

    string currentAnimationState;

    void ChangeAnimationState(string state)
    {
        if (currentAnimationState == state) return;
        currentAnimationState = state;
        animator.CrossFadeInFixedTime(state, 0.15f);
    }

    void SetAnimations()
    {
        if (attacking) return;
        ChangeAnimationState(_PlayerVelocity.magnitude < 0.1f ? IDLE : WALK);
    }

    // ================= EVENTS =================

    void AssignInputs()
    {
        input.Torch.performed += _ => ToggleTorch();
    }

    void ToggleTorch()
    {
        torchActive = !torchActive;
        if (torchObject) torchObject.SetActive(torchActive);
        if (playerAmbientLight)
            playerAmbientLight.intensity = torchActive ? torchLightIntensity : normalLightIntensity;

        if (torchLoopAudioSource && torchLoopSound)
        {
            if (torchActive) torchLoopAudioSource.Play();
            else torchLoopAudioSource.Stop();
        }
    }

    void OnHealthChanged(float hp)
    {
        Debug.Log($"❤️ Vida: {hp * 100}%");
    }

    public void OnPlayerDeath()
    {
        enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnEnable() => playerInput?.Enable();
    void OnDisable() => playerInput?.Disable();

    private void HandleFootsteps()
    {
        if (!isGrounded) return;

        Vector2 movementInput = input.Movement.ReadValue<Vector2>();
        bool moving = Mathf.Abs(movementInput.x) > 0.1f || Mathf.Abs(movementInput.y) > 0.1f;

        if (moving && footstepsLoopSound != null)
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
                footstepsAudioSource.Stop();
        }
    }

    private void HandleJump()
    {
        if (input.Jump.WasPressedThisFrame() && isGrounded)
        {
            _PlayerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

}

