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
    [SerializeField] private WeaponBase[] weaponUpgrades; // Pistol, SMG, Rifle, Laser
    public WeaponBase currentWeapon { get; private set; }

    // === Dầu thô ===
    public int crudeOil { get; private set; } = 0;

    // === Cấp nâng cấp bằng dầu ===
    public int damageLevel = 0;
    public int magazineLevel = 0;
    public int reloadLevel = 0;

    // === Dash (mặc định, phím Space) ===
    [Header("Dash")]
    [SerializeField] private float dashDistance = 5f;         // Khoảng cách lướt
    [SerializeField] private float dashDuration = 0.2f;       // Thời gian lướt (giây)
    [SerializeField] private float dashCooldown = 2f;         // Thời gian hồi chiêu
    private bool isDashing = false;                           // Đang trong trạng thái lướt
    private float dashTimer = 0f;                             // Bộ đếm thời gian lướt
    private Vector2 dashDirection;                            // Hướng lướt
    private float nextDashTime = 0f;                          // Thời điểm có thể dash tiếp theo

    // === Pet (mở khóa qua phase) ===
    [Header("Pet")]
    [SerializeField] private GameObject petPrefab;
    [SerializeField] private int petCount = 3;
    [SerializeField] private float petLifetime = 10f;
    [SerializeField] private float petCooldown = 5f;
    private bool hasPet = false;
    private float nextPetTime = 0f;
    // == Energy Beam == //
    [Header("Energy Beam")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private Transform mouthPosition;
    [SerializeField] private float beamDamage = 20f;
    [SerializeField] private float beamRange = 5f;
    [SerializeField] private float beamCooldown = 2f;
    [SerializeField] private float mouthOpenTime = 0.3f;   // Thời gian mở miệng (giây)
    [SerializeField] private float beamDuration = 0.5f;     // Thời gian phun beam (giây)
    private bool hasBeam = false;
    private float nextBeamTime = 0f;
    private bool isBeaming = false;   // Đang trong trạng thái bắn beam
    // === Dual Wield (mở khóa qua phase) ===
    [Header("Dual Wield")]
    [SerializeField] private float dualWieldDuration = 5f;
    [SerializeField] private float dualWieldCooldown = 10f;
    [SerializeField] private Vector3 dualGunOffset = new Vector3(-0.5f, -0.2f, 0f);
    [SerializeField] private string dualGunLayer = "Default";
    private bool hasDualWield = false;
    private bool dualWieldActive = false;
    private float dualWieldTimer = 0f;
    private GameObject dualGunInstance;

    // === Trạng thái thăng hoa (Ascended) ===
    [Header("Ascended")]
    [SerializeField] private float ascendedPetBonus = 2f;
    [SerializeField] private float ascendedBeamRangeBonus = 3f;
    [SerializeField] private float ascendedBeamDamageBonus = 10f;
    [SerializeField] private float ascendedDualWieldDurationBonus = 5f;
    private bool isAscended = false;
    public bool IsAscended => isAscended;

    // Thuộc tính public cho UI kiểm tra
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
        MovePlayer();
        HandleSkillsInput();
        UpdateDualWieldTimer();
        UpdateDash(); // Theo dõi trạng thái lướt
        if (Input.GetKeyDown(KeyCode.Escape))
            GameManager.Instance?.PauseGameMenu();
    }

    void MovePlayer()
    {
        // Nếu đang lướt, không can thiệp di chuyển thường (vận tốc do Dash kiểm soát)
        if (isDashing || isBeaming) return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.linearVelocity = input.normalized * movespeed;
        if (input.x < 0) spriteRenderer.flipX = true;
        else if (input.x > 0) spriteRenderer.flipX = false;
        if (animator != null) animator.SetBool("isRun", input != Vector2.zero);
    }

    void HandleSkillsInput()
    {
        // Dash (Space)
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && Time.time >= nextDashTime)
        {
            StartDash();
        }
        // Pet (Q)
        if (Input.GetKeyDown(KeyCode.Q) && hasPet && Time.time >= nextPetTime)
            SummonPets();

        // Energy Beam (E)
        if (Input.GetKeyDown(KeyCode.E) && hasBeam && !isBeaming && Time.time >= nextBeamTime)
        {
            StartCoroutine(FireBeamCoroutine());
        }
        // Dual Wield (R)
        if (Input.GetKeyDown(KeyCode.R) && hasDualWield && !dualWieldActive && Time.time >= dualWieldCooldown)
            ActivateDualWield();
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
    // ===== Dash mới =====
void StartDash()
{
    isDashing = true;
    dashTimer = dashDuration;
    nextDashTime = Time.time + dashCooldown;

    // Xác định hướng lướt: nếu không có input thì lướt theo hướng mặt
    Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    if (input.magnitude > 0)
        dashDirection = input.normalized;
    else
        dashDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;

    // Gán vận tốc ban đầu
    float dashSpeed = dashDistance / dashDuration;
    rb.linearVelocity = dashDirection * dashSpeed;

    // Kích hoạt animation nếu có
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
            rb.linearVelocity = Vector2.zero; // Dừng lại sau khi lướt xong
        }
        else
        {
            // Giữ vận tốc không đổi trong lúc lướt (có thể thêm hiệu ứng giảm dần nếu muốn)
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

    void FireBeam()
    {
        GameObject beam = Instantiate(beamPrefab, mouthPosition.position, Quaternion.identity);
        EnergyBeam beamScript = beam.GetComponent<EnergyBeam>();
        if (beamScript != null)
        {
            float dmg = beamDamage + (isAscended ? ascendedBeamDamageBonus : 0);
            float range = beamRange + (isAscended ? ascendedBeamRangeBonus : 0);
            Vector2 dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            beamScript.Initialize(dmg, range, dir);
        }
        nextBeamTime = Time.time + beamCooldown;
    }
    IEnumerator FireBeamCoroutine()
    {
        isBeaming = true;
        // Kích hoạt animation "Beam" (mở miệng)
        animator.SetTrigger("Beam");

        // Đợi cho miệng mở xong
        yield return new WaitForSeconds(mouthOpenTime);

        // Tạo beam tại miệng
        GameObject beam = Instantiate(beamPrefab, mouthPosition.position, Quaternion.identity);
        EnergyBeam beamScript = beam.GetComponent<EnergyBeam>();
        if (beamScript != null)
        {
            float dmg = beamDamage + (isAscended ? ascendedBeamDamageBonus : 0);
            float range = beamRange + (isAscended ? ascendedBeamRangeBonus : 0);
            Vector2 dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            beamScript.Initialize(dmg, range, dir);
        }

        // Đợi cho animation phun beam kết thúc
        yield return new WaitForSeconds(beamDuration);

        // Kết thúc beam, cho phép di chuyển
        isBeaming = false;
        nextBeamTime = Time.time + beamCooldown;
    }

    void ActivateDualWield()
    {
        dualWieldActive = true;
        float duration = dualWieldDuration + (isAscended ? ascendedDualWieldDurationBonus : 0);
        dualWieldTimer = duration;

        if (currentWeapon != null && weaponSocket != null)
        {
            GameObject dualGun = Instantiate(currentWeapon.gameObject,
                weaponSocket.position + dualGunOffset,
                Quaternion.identity, transform);
            dualGunInstance = dualGun;

            // Set layer cho súng clone và tất cả con
            dualGun.layer = LayerMask.NameToLayer(dualGunLayer);
            foreach (Transform child in dualGun.GetComponentsInChildren<Transform>())
                child.gameObject.layer = dualGun.layer;

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
    public void UnlockPet()        { hasPet = true; }
    public void UnlockBeam()       { hasBeam = true; }
    public void UnlockDualWield()  { hasDualWield = true; }

    // ===== THĂNG HOA =====
    public void EnterAscendedState()
    {
        isAscended = true;
        if (ascendedSprite != null) spriteRenderer.sprite = ascendedSprite;
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
}