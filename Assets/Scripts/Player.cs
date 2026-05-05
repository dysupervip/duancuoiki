using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
    // === Di chuyển ===
    [SerializeField] private float movespeed = 5f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // === Máu ===
    [SerializeField] private float maxHp = 700f;
    private float currentHp;
    [SerializeField] private Image hpBar;

    // === Level & XP ===
    [SerializeField] private int level = 1;
    [SerializeField] private int xp = 0;
    [SerializeField] private int[] xpToNextLevel; // XP cần cho lv2,3,...,10

    // === Vũ khí ===
    [SerializeField] private Transform weaponSocket;
    [SerializeField] private WeaponBase[] weaponUpgrades; // Pistol, SMG, Rifle, Laser, Ultimate
    public WeaponBase currentWeapon { get; private set; }

    // === Dầu thô ===
    public int crudeOil { get; private set; } = 0;

    // === Cấp nâng cấp bằng dầu ===
    public int damageLevel = 0;
    public int magazineLevel = 0;
    public int reloadLevel = 0;

    // === Dash (mặc định, phím Space) ===
    [Header("Dash")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 2f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector2 dashDirection;
    private float nextDashTime = 0f;

    // === Pet (mở khóa qua phase) ===
    [Header("Pet")]
    [SerializeField] private GameObject petPrefab;
    [SerializeField] private int petCount = 3;
    [SerializeField] private float petLifetime = 10f;
    [SerializeField] private float petCooldown = 5f;
    private bool hasPet = false;
    private float nextPetTime = 0f;

    // === Energy Beam (mở khóa qua phase) ===
    [Header("Energy Beam")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private Transform bellyPosition;     // Vị trí bụng
    [SerializeField] private Transform backPosition;      // Vị trí lưng
    [SerializeField] private float beamDamage = 20f;
    [SerializeField] private float beamRange = 5f;
    [SerializeField] private float beamCooldown = 2f;
    [SerializeField] private float beamDuration = 0.5f;
    private bool hasBeam = false;
    private float nextBeamTime = 0f;
    private bool isBeaming = false;

    // === Dual Wield (mở khóa qua phase) ===
    [Header("Dual Wield")]
    [SerializeField] private string dualGunSortingLayer = "Default";
    [SerializeField] private int dualGunOrderInLayer = 0;
    [SerializeField] private float dualWieldDuration = 5f;
    [SerializeField] private float dualWieldCooldown = 10f;
    [SerializeField] private Vector3 dualGunOffset = new Vector3(-0.5f, -0.2f, 0f);
    [SerializeField] private string dualGunLayer = "Default";
    private bool hasDualWield = false;
    private bool dualWieldActive = false;
    private float dualWieldTimer = 0f;
    private GameObject dualGunInstance;
    private float nextDualWieldTime = 0f;

    // === Trạng thái thăng hoa (Ascended) ===
    [Header("Ascended")]
    [SerializeField] private float ascendedPetBonus = 2f;
    [SerializeField] private float ascendedBeamRangeBonus = 3f;
    [SerializeField] private float ascendedBeamDamageBonus = 10f;
    [SerializeField] private float ascendedDualWieldDurationBonus = 5f;
    private bool isAscended = false;
    public bool IsAscended => isAscended;

    // Thuộc tính public cho UI
    public bool HasPet => hasPet;
    public bool HasBeam => hasBeam;
    public bool HasDualWield => hasDualWield;

    // Hình ảnh khi thăng hoa
    [SerializeField] private Sprite ascendedSprite;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHp = maxHp;
        UpdateHpBar();
        if (weaponUpgrades.Length > 0)
            EquipWeapon(weaponUpgrades[0]); // Bắt đầu với Pistol
    }

    void Update()
    {
        WeaponUpgradeUI upgradeUI = FindAnyObjectByType<WeaponUpgradeUI>();

        // ESC ưu tiên đóng bảng upgrade
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (upgradeUI != null && upgradeUI.IsOpen())
            {
                upgradeUI.Close();
                return;
            }

            GameManager.Instance?.PauseGameMenu();
            return;
        }

        // Nếu đang mở bảng upgrade thì chặn gameplay
        if (upgradeUI != null && upgradeUI.IsOpen())
            return;

        MovePlayer();
        HandleSkillsInput();
        UpdateDualWieldTimer();
        UpdateDash();
    }

    void MovePlayer()
    {
        if (isDashing || isBeaming) return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.linearVelocity = input.normalized * movespeed;
        if (input.x < 0) spriteRenderer.flipX = true;
        else if (input.x > 0) spriteRenderer.flipX = false;
        if (animator != null) animator.SetBool("isRun", input != Vector2.zero);
    }

    void HandleSkillsInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            UseDashSkill();

        if (Input.GetKeyDown(KeyCode.Q))
            UsePetSkill();

        if (Input.GetKeyDown(KeyCode.E))
            UseBeamSkill();

        if (Input.GetKeyDown(KeyCode.R))
            UseDualWieldSkill();
    }

    void UpdateDualWieldTimer()
    {
        if (dualWieldActive)
        {
            dualWieldTimer -= Time.deltaTime;
            if (dualWieldTimer <= 0) DeactivateDualWield();
        }
    }

    // ===== KỸ NĂNG =====
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        nextDashTime = Time.time + dashCooldown;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.magnitude > 0)
            dashDirection = input.normalized;
        else
            dashDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        float dashSpeed = dashDistance / dashDuration;
        rb.linearVelocity = dashDirection * dashSpeed;

        if (animator != null)
            animator.SetTrigger("Dash");
    }

    void UpdateDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void SummonPets()
    {
        int totalPets = petCount + (isAscended ? (int)ascendedPetBonus : 0);
        for (int i = 0; i < totalPets; i++)
        {
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 1.5f;
            Instantiate(petPrefab, spawnPos, Quaternion.identity);
        }
        nextPetTime = Time.time + petLifetime + petCooldown;
    }

    IEnumerator FireBeamCoroutine()
    {
        isBeaming = true;
        animator.SetTrigger("Beam");

        // Xác định hướng và vị trí chuột
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Sát thương và tầm (tăng khi thăng hoa)
        float dmg = beamDamage + (isAscended ? ascendedBeamDamageBonus : 0);
        float range = beamRange + (isAscended ? ascendedBeamRangeBonus : 0);

        // --- Quyết định vị trí bắn chính (cơ chế cũ) ---
        bool facingLeft = spriteRenderer.flipX;
        bool mouseOnLeft = mousePos.x < transform.position.x;
        bool sameSide = (facingLeft == mouseOnLeft);
        Transform primarySpawn = sameSide ? bellyPosition : backPosition;

        // --- Spawn tia chính ---
        SpawnBeam(primarySpawn.position, angle, dmg, range, direction);

        // --- Nếu thăng hoa, spawn thêm tia từ vị trí còn lại ---
        if (isAscended)
        {
            Transform secondarySpawn = sameSide ? backPosition : bellyPosition;
            if (secondarySpawn != null)
            {
                SpawnBeam(secondarySpawn.position, angle, dmg, range, direction);
            }
        }

        yield return new WaitForSeconds(beamDuration);

        isBeaming = false;
        nextBeamTime = Time.time + beamCooldown;
    }

    /// <summary>
    /// Tạo một tia beam tại vị trí spawnPoint, với góc xoay, sát thương, tầm và hướng.
    /// </summary>
    void SpawnBeam(Vector3 spawnPoint, float angle, float dmg, float range, Vector2 direction)
    {
        if (beamPrefab == null)
        {
            Debug.LogError("[Player] SpawnBeam: beamPrefab is null!");
            return;
        }

        GameObject beam = Instantiate(beamPrefab, spawnPoint, Quaternion.Euler(0, 0, angle));
        EnergyBeam beamScript = beam.GetComponent<EnergyBeam>();
        if (beamScript != null)
        {
            beamScript.Initialize(dmg, range, direction);
        }
    }

    void ActivateDualWield()
    {
        nextDualWieldTime = Time.time + dualWieldCooldown;
        dualWieldActive = true;
        float duration = dualWieldDuration + (isAscended ? ascendedDualWieldDurationBonus : 0);
        dualWieldTimer = duration;

        if (currentWeapon != null && weaponSocket != null)
        {
            GameObject dualGun = Instantiate(currentWeapon.gameObject,
                weaponSocket.position + dualGunOffset,
                Quaternion.identity, transform);
            dualGunInstance = dualGun;

            // --- Layer vật lý ---
            dualGun.layer = LayerMask.NameToLayer(dualGunLayer);
            foreach (Transform child in dualGun.GetComponentsInChildren<Transform>())
                child.gameObject.layer = dualGun.layer;

            // --- Sorting Layer & Order ---
            SpriteRenderer[] renderers = dualGun.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in renderers)
            {
                sr.sortingLayerName = dualGunSortingLayer;
                sr.sortingOrder = dualGunOrderInLayer;
            }

            DualWieldHandler handler = dualGun.AddComponent<DualWieldHandler>();
            handler.Initialize(currentWeapon, duration);
        }
    }

    void DeactivateDualWield()
    {
        dualWieldActive = false;
        if (dualGunInstance != null)
            Destroy(dualGunInstance);
    }

    // ===== XP & LEVEL =====
    public void AddXP(int amount)
    {
        xp += amount;
        CheckLevelUp();
    }

    void CheckLevelUp()
    {
        while (level < 10 && level - 1 < xpToNextLevel.Length && xp >= xpToNextLevel[level - 1])
        {
            xp -= xpToNextLevel[level - 1];
            level++;
            OnLevelUp();
        }
    }

    void OnLevelUp()
    {
        Debug.Log($"Level up! Now level {level}");
        if (level == 3 && weaponUpgrades.Length > 1)
            EquipWeapon(weaponUpgrades[1]);   // SMG
        else if (level == 6 && weaponUpgrades.Length > 2)
            EquipWeapon(weaponUpgrades[2]);   // Rifle
        else if (level == 9 && weaponUpgrades.Length > 3)
            EquipWeapon(weaponUpgrades[3]);   // Laser
    }

    // ===== VŨ KHÍ =====
    public void EquipWeapon(WeaponBase newWeaponPrefab)
    {
        if (newWeaponPrefab == null) return;
        if (currentWeapon != null) Destroy(currentWeapon.gameObject);

        WeaponBase newWeapon = Instantiate(newWeaponPrefab, transform);
        if (weaponSocket != null)
        {
            newWeapon.transform.localPosition = weaponSocket.localPosition;
            newWeapon.transform.localRotation = weaponSocket.localRotation;
        }
        currentWeapon = newWeapon;
        newWeapon.UpdateStatsFromPlayer();
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetCurrentWeapon(newWeapon);
    }

    // ===== MỞ KHÓA KỸ NĂNG =====
    public void UnlockPet() { hasPet = true; }
    public void UnlockBeam() { hasBeam = true; }
    public void UnlockDualWield() { hasDualWield = true; }

    // ===== THĂNG HOA =====
    public void EnterAscendedState()
    {
        isAscended = true;
        if (ascendedSprite != null) spriteRenderer.sprite = ascendedSprite;

        // Trang bị súng tối thượng (index 4) nếu có
        if (weaponUpgrades.Length > 4 && weaponUpgrades[4] != null)
        {
            EquipWeapon(weaponUpgrades[4]);
            Debug.Log("[Player] Entered Ascended State! Trang bị súng tối thượng.");
        }
        else
        {
            Debug.Log("[Player] Entered Ascended State nhưng không có súng tối thượng.");
        }
    }

    // ===== MÁU & DẦU =====
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();
        if (currentHp <= 0)
        {
            GameManager.Instance?.GameOver();
            Destroy(gameObject);
        }
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        UpdateHpBar();
    }

    public void AddCrudeOil(int amount)
    {
        crudeOil += amount;
    }

    void UpdateHpBar()
    {
        if (hpBar != null) hpBar.fillAmount = currentHp / maxHp;
    }

    // ===== NÂNG CẤP BẰNG DẦU =====
    public bool UpgradeDamage()
    {
        int cost = damageLevel + 1;
        if (crudeOil >= cost && damageLevel < 3)
        {
            crudeOil -= cost;
            damageLevel++;
            currentWeapon?.UpdateStatsFromPlayer();
            return true;
        }
        return false;
    }

    public bool UpgradeMagazine()
    {
        int cost = magazineLevel + 1;
        if (crudeOil >= cost && magazineLevel < 3)
        {
            crudeOil -= cost;
            magazineLevel++;
            currentWeapon?.UpdateStatsFromPlayer();
            return true;
        }
        return false;
    }

    public bool UpgradeReloadSpeed()
    {
        int cost = reloadLevel + 1;
        if (crudeOil >= cost && reloadLevel < 3)
        {
            crudeOil -= cost;
            reloadLevel++;
            currentWeapon?.UpdateStatsFromPlayer();
            return true;
        }
        return false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Usb"))
        {
            GameManager.Instance?.WinGame();

            Destroy(collision.gameObject);
        }
    }

    public void UsePetSkill()
    {
        if (hasPet && Time.time >= nextPetTime)
            SummonPets();
    }

    public void UseBeamSkill()
    {
        if (hasBeam && !isBeaming && Time.time >= nextBeamTime)
            StartCoroutine(FireBeamCoroutine());
    }

    public void UseDualWieldSkill()
    {
        if (hasDualWield && !dualWieldActive && Time.time >= nextDualWieldTime)
            ActivateDualWield();
    }

    public void UseDashSkill()
    {
        if (!isDashing && Time.time >= nextDashTime)
            StartDash();
    }

    public float GetPetCooldownRemaining()
    {
        return Mathf.Max(0, nextPetTime - Time.time);
    }

    public float GetPetCooldown()
    {
        return petLifetime + petCooldown;
    }

    public float GetBeamCooldownRemaining()
    {
        return Mathf.Max(0, nextBeamTime - Time.time);
    }

    public float GetBeamCooldown()
    {
        return beamCooldown;
    }

    public float GetDualCooldownRemaining()
    {
        return Mathf.Max(0, nextDualWieldTime - Time.time);
    }

    public float GetDualCooldown()
    {
        return dualWieldCooldown;
    }

    public float GetDashCooldownRemaining()
    {
        return Mathf.Max(0, nextDashTime - Time.time);
    }

    public float GetDashCooldown()
    {
        return dashCooldown;
    }
}