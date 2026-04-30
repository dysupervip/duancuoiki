using UnityEngine;
using UnityEngine.UI; // Dùng Image cho thanh máu

public class Player : MonoBehaviour
{
    // === Di chuyển ===
    [SerializeField] private float movespeed = 5f;          // Tốc độ di chuyển cơ bản
    private Rigidbody2D rb;                                // Vật lý 2D
    private SpriteRenderer spriteRenderer;                 // Để lật mặt nhân vật
    private Animator animator;                             // Hoạt ảnh

    // === Máu ===
    [SerializeField] private float maxHp = 700f;           // Máu tối đa
    private float currentHp;                               // Máu hiện tại
    [SerializeField] private Image hpBar;                  // Thanh máu UI (Image fill)

    // === Level & XP ===
    [SerializeField] private int level = 1;                // Cấp độ bắt đầu
    [SerializeField] private int xp = 0;                   // XP hiện có
    [SerializeField] private int[] xpToNextLevel;          // Mảng 9 số: XP cần cho lv2, lv3, ..., lv10

    // === Vũ khí ===
    [SerializeField] private Transform weaponSocket;       // Vị trí gắn súng (con của Player)
    [SerializeField] private WeaponBase[] weaponUpgrades;  // Mảng súng: [0]Pistol, [1]SMG, [2]Rifle, [3]Laser, [4]Ultimate
    public WeaponBase currentWeapon { get; private set; }  // Vũ khí đang cầm

    // === Dầu thô ===
    public int crudeOil { get; private set; } = 0;         // Số dầu hiện có

    // === Cấp nâng cấp bằng dầu ===
    public int damageLevel = 0;      // Cấp sát thương (0-3), mỗi cấp +10 damage
    public int magazineLevel = 0;    // Cấp băng đạn (0-3), mỗi cấp +5 đạn
    public int reloadLevel = 0;      // Cấp nạp đạn (0-3), mỗi cấp giảm 0.3s

    // === Kỹ năng ===
    [Header("Skills")]
    [SerializeField] private float speedBoostMultiplier = 2f;   // Hệ số nhân tốc độ (x2)
    [SerializeField] private float speedBoostDuration = 3f;     // Thời gian tăng tốc
    [SerializeField] private float speedBoostCooldown = 5f;     // Hồi chiêu sau khi hết hiệu lực
    [SerializeField] private float dashDistance = 3f;           // Khoảng cách lướt
    [SerializeField] private float dashCooldown = 3f;           // Hồi chiêu lướt
    [SerializeField] private GameObject petPrefab;              // Prefab pet
    [SerializeField] private int petCount = 3;                  // Số pet mỗi lần gọi
    [SerializeField] private float petLifetime = 10f;           // Thời gian pet tồn tại
    [SerializeField] private float petCooldown = 5f;            // Hồi chiêu sau khi pet biến mất

    // Trạng thái mở khóa skill
    private bool hasSpeedBoost = false;
    private bool hasDash = false;
    private bool hasPet = false;
    public bool HasSpeedBoost => hasSpeedBoost;
    public bool HasDash => hasDash;
    public bool HasPet => hasPet;

    // Trạng thái tăng tốc
    private bool speedBoostActive = false;
    private float speedBoostTimer = 0f;

    // Thời điểm có thể dùng skill tiếp theo
    private float nextSpeedBoostTime = 0f;
    private float nextDashTime = 0f;
    private float nextPetAvailableTime = 0f;

    // Hình ảnh thay đổi khi level 10
    [SerializeField] private Sprite[] playerSprites;    // [0] mặc định, [1] level 10
   
    // Tự thiết lập khi trò chơi bắt đầu
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();               // Lấy Rigidbody2D
        spriteRenderer = GetComponent<SpriteRenderer>(); // Lấy SpriteRenderer
        animator = GetComponent<Animator>();            // Lấy Animator
    }

    void Start()
    {
        currentHp = maxHp;                              // Đặt máu đầy
        UpdateHpBar();                                  // Cập nhật thanh máu
        if (weaponUpgrades.Length > 0)                  // Nếu có súng trong mảng
            EquipWeapon(weaponUpgrades[0]);             // Trang bị súng đầu tiên (Pistol)
    }

    void Update()
    {
        MovePlayer();                                   // Di chuyển
        UpdateSpeedBoost();                             // Cập nhật trạng thái tăng tốc
        HandleSkillsInput();                            // Xử lý phím kỹ năng
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Nếu đang mở upgrade thì KHÔNG pause
            if (FindAnyObjectByType<WeaponUpgradeUI>()?.IsOpen() == true)
                return;

            GameManager.Instance?.PauseGameMenu();
        }
    }

    void MovePlayer()
    {
        // Lấy hướng di chuyển
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // Nếu đang tăng tốc thì nhân tốc độ, ngược lại dùng tốc độ thường
        float currentSpeed = speedBoostActive ? movespeed * speedBoostMultiplier : movespeed;
        rb.linearVelocity = input.normalized * currentSpeed; // Gán vận tốc

        // Lật mặt nhân vật
        if (input.x < 0) spriteRenderer.flipX = true;
        else if (input.x > 0) spriteRenderer.flipX = false;

        // Bật/tắt animation chạy
        if (animator != null) animator.SetBool("isRun", input != Vector2.zero);
    }

    void UpdateSpeedBoost()
    {
        if (speedBoostActive)                           // Nếu đang tăng tốc
        {
            speedBoostTimer -= Time.deltaTime;          // Giảm thời gian
            if (speedBoostTimer <= 0)                   // Hết giờ
            {
                speedBoostActive = false;               // Tắt tăng tốc
            }
        }
    }

    void HandleSkillsInput()
    {
        // Q - Tăng tốc: cần mở khóa và hết cooldown
        if (Input.GetKeyDown(KeyCode.Q) && hasSpeedBoost && Time.time >= nextSpeedBoostTime)
            ActivateSpeedBoost();
        // E - Pet: cần mở khóa và hết cooldown
        if (Input.GetKeyDown(KeyCode.E) && hasPet && Time.time >= nextPetAvailableTime)
            SummonPets();
        // R - Lướt: cần mở khóa và hết cooldown
        if (Input.GetKeyDown(KeyCode.R) && hasDash && Time.time >= nextDashTime)
            Dash();
    }

    // === XP & LEVEL ===
    public void AddXP(int amount)
    {
        xp += amount;           // Cộng XP
        CheckLevelUp();         // Kiểm tra lên cấp
    }

    void CheckLevelUp()
    {
        // Khi còn level < 10 và XP đủ ngưỡng
        while (level < 10 && xpToNextLevel.Length >= level && xp >= xpToNextLevel[level - 1])
        {
            xp -= xpToNextLevel[level - 1];  // Trừ XP đã dùng
            level++;                          // Tăng level
            OnLevelUp();                      // Xử lý lên cấp
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
        else if (level == 10)
        {
            currentHp = maxHp;                // Hồi máu đầy
            UpdateHpBar();
            movespeed += 2f;                  // Tăng vĩnh viễn 2 tốc độ
            if (weaponUpgrades.Length > 4)
                EquipWeapon(weaponUpgrades[4]); // Súng tối thượng
            if (playerSprites.Length >= 2)
                spriteRenderer.sprite = playerSprites[1]; // Đổi hình
        }
    }

    // === VŨ KHÍ ===
    public void EquipWeapon(WeaponBase newWeaponPrefab)
    {
        if (newWeaponPrefab == null) return;    // Không làm gì nếu rỗng
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject); // Hủy súng cũ

        // Tạo súng mới làm con của Player
        WeaponBase newWeapon = Instantiate(newWeaponPrefab, transform);

        // Đặt vị trí súng theo socket
        if (weaponSocket != null)
        {
            newWeapon.transform.localPosition = weaponSocket.localPosition;
            newWeapon.transform.localRotation = weaponSocket.localRotation;
        }
        else
        {
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localRotation = Quaternion.identity;
        }

        currentWeapon = newWeapon;
        newWeapon.UpdateStatsFromPlayer(); // Cập nhật chỉ số từ Player

        // Báo cho CursorManager biết vũ khí mới
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetCurrentWeapon(newWeapon);
    }

    // === KỸ NĂNG ===
    public void UnlockSpeedBoost() { hasSpeedBoost = true; }
    public void UnlockDash() { hasDash = true; }
    public void UnlockPet() { hasPet = true; }

    void ActivateSpeedBoost()
    {
        speedBoostActive = true;
        speedBoostTimer = speedBoostDuration;
        nextSpeedBoostTime = Time.time + speedBoostDuration + speedBoostCooldown; // Thời điểm được dùng lại
    }

    void SummonPets()
    {
        for (int i = 0; i < petCount; i++)
        {
            // Vị trí ngẫu nhiên quanh player
            Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 1.5f;
            Instantiate(petPrefab, spawnPos, Quaternion.identity);
        }
        nextPetAvailableTime = Time.time + petLifetime + petCooldown;
    }

    void Dash()
    {
        // Lấy hướng di chuyển từ input, nếu không nhấn gì thì lăn sang phải
        Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (dir == Vector2.zero) dir = Vector2.right;
        transform.position += (Vector3)(dir * dashDistance); // Dịch chuyển
        nextDashTime = Time.time + dashCooldown;
    }

    // === MÁU & DẦU ===
    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();
        if (currentHp <= 0)
        {
            GameManager.Instance?.GameOver(); // Báo game over
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
        if (hpBar != null)
            hpBar.fillAmount = currentHp / maxHp;

        if (PlayerHP.Instance != null)
        {
            PlayerHP.Instance.UpdateHP(currentHp, maxHp);
        }
    }

    // === NÂNG CẤP BẰNG DẦU ===
    public bool UpgradeDamage()
    {
        int cost = damageLevel + 1;             // Chi phí = cấp hiện tại + 1
        if (crudeOil >= cost && damageLevel < 3) // Đủ dầu và chưa đạt cấp tối đa
        {
            crudeOil -= cost;
            damageLevel++;
            currentWeapon?.UpdateStatsFromPlayer(); // Cập nhật lại chỉ số súng
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