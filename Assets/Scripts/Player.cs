using UnityEngine;
using UnityEngine.UI; // Để dùng Image (thanh máu)

public class Player : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [Header("Vũ khí mặc định")]
    [SerializeField] private WeaponBase defaultWeaponPrefab; // Kéo prefab Pistol vào đây
    // --- Di chuyển ---
    [SerializeField] private float movespeed = 5f;   // Tốc độ di chuyển cơ bản
    private Rigidbody2D rb;                          // Vật lý 2D
    private SpriteRenderer spriteRenderer;           // Để lật mặt nhân vật
    private Animator animator;                       // Để chạy animation

    // --- Máu và Giáp ---
    [SerializeField] private float maxHp = 700f;     // Máu tối đa
    private float currentHp;                         // Máu hiện tại
    [SerializeField] private Image hpBar;            // Thanh máu UI (Image fill)
    [Header("Vị trí đặt súng")]
    [SerializeField] private Transform weaponSocket; // Kéo WeaponSocket vào đây

    // --- Chỉ số nâng cấp ---
    [SerializeField] private float armor = 0f;       // Giáp (0.0 = 0%, 0.5 = 50% giảm sát thương)

    // --- Dầu thô ---
    public int crudeOil { get; private set; } = 0;   // Số dầu đang có (chỉ đọc từ ngoài)

    // --- Vũ khí ---
    public WeaponBase currentWeapon { get; private set; }  // Vũ khí hiện tại (script WeaponBase)

    // --- Bonus từ dầu cho vũ khí ---
    public float damageBonus = 0f;        // Tỉ lệ tăng sát thương (0.2 = +20%)
    public float fireRateBonus = 0f;      // Tỉ lệ giảm thời gian hồi bắn
    public float reloadSpeedBonus = 0f;   // Tỉ lệ giảm thời gian nạp đạn
    public int magazineBonus = 0;         // Đạn cộng thêm vào băng

    // Cấp nâng cấp (max 3)
    public int damageUpgradeLevel = 0;
    public int fireRateUpgradeLevel = 0;
    public int reloadUpgradeLevel = 0;
    public int magazineUpgradeLevel = 0;

    void Awake()
    {
        // Lấy các component gắn trên cùng GameObject
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHp = maxHp;  // Bắt đầu với máu đầy
        UpdateHpBar();      // Cập nhật thanh máu UI
        // Trang bị súng mặc định (súng ngắn) ngay khi bắt đầu
        if (defaultWeaponPrefab != null)
        EquipWeapon(defaultWeaponPrefab);
    }

    void Update()
    {
        MovePlayer(); // Mỗi khung hình kiểm tra di chuyển
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.PauseGameMenu();
        }    
    }

    void MovePlayer()
    {
        // Lấy input từ bàn phím (WASD hoặc mũi tên)
        Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // Gán vận tốc cho Rigidbody2D (di chuyển)
        rb.linearVelocity = playerInput.normalized * movespeed;

        // Lật mặt nhân vật dựa vào hướng di chuyển
        if (playerInput.x < 0) spriteRenderer.flipX = true;   // Sang trái
        else if (playerInput.x > 0) spriteRenderer.flipX = false; // Sang phải

        // Bật/tắt animation chạy
        animator.SetBool("isRun", playerInput != Vector2.zero);
    }

    public void TakeDamage(float damage)
    {
        // Giáp giảm sát thương: damage * (1 - armor)
        float effectiveDamage = damage * (1f - Mathf.Clamp01(armor));
        currentHp -= effectiveDamage;
        currentHp = Mathf.Max(currentHp, 0); // Không để máu âm
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(float healValue)
    {
        currentHp = Mathf.Min(currentHp + healValue, maxHp);
        UpdateHpBar();
    }

    // Nâng cấp từ bảng chọn sau phase
    public void UpgradeHP(float amount)
    {
        maxHp += amount;
        currentHp = maxHp; // Hồi đầy máu sau khi nâng cấp
        UpdateHpBar();
    }

    public void UpgradeMoveSpeed(float amount)
    {
        movespeed += amount; // Tăng tốc độ vĩnh viễn
    }

    public void UpgradeArmor(float amount)
    {
        armor = Mathf.Clamp01(armor + amount); // Tăng giáp nhưng không vượt quá 100%
    }

    // Nhặt dầu thô
    public void AddCrudeOil(int amount)
    {
        crudeOil += amount;
        // Ở đây có thể gọi UI cập nhật hiển thị dầu (sẽ làm sau)
    }

    // Thay vũ khí mới (tự động sau phase)
    public void EquipWeapon(WeaponBase newWeaponPrefab)
{
    if (newWeaponPrefab == null)
    {
        Debug.LogError("EquipWeapon: prefab súng rỗng!");
        return;
    }

    if (currentWeapon != null)
        Destroy(currentWeapon.gameObject);

    WeaponBase newWeapon = Instantiate(newWeaponPrefab, transform);

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
}

    // Các hàm nâng cấp vũ khí bằng dầu (gọi từ UI)
    public bool UpgradeDamage()
    {
        int cost = damageUpgradeLevel + 1; // Lần 1 tốn 1 dầu, lần 2 tốn 2...
        if (crudeOil >= cost && damageUpgradeLevel < 3)
        {
            crudeOil -= cost;
            damageUpgradeLevel++;
            damageBonus = damageUpgradeLevel * 0.2f; // Mỗi cấp +20% sát thương
            currentWeapon?.UpdateStatsFromPlayer(); // Cập nhật chỉ số cho súng hiện tại
            return true;
        }
        return false;
    }

    // Tương tự cho các chỉ số khác...
    public bool UpgradeFireRate()
    {
        int cost = fireRateUpgradeLevel + 1;
        if (crudeOil >= cost && fireRateUpgradeLevel < 3)
        {
            crudeOil -= cost;
            fireRateUpgradeLevel++;
            fireRateBonus = fireRateUpgradeLevel * 0.15f; // Mỗi cấp giảm 15% thời gian hồi bắn
            currentWeapon?.UpdateStatsFromPlayer();
            return true;
        }
        return false;
    }

    public bool UpgradeReloadSpeed()
    {
        int cost = reloadUpgradeLevel + 1;
        if (crudeOil >= cost && reloadUpgradeLevel < 3)
        {
            crudeOil -= cost;
            reloadUpgradeLevel++;
            reloadSpeedBonus = reloadUpgradeLevel * 0.2f; // Mỗi cấp giảm 20% thời gian nạp đạn
            currentWeapon?.UpdateStatsFromPlayer();
            return true;
        }
        return false;
    }

    public bool UpgradeMagazine()
    {
        int cost = magazineUpgradeLevel + 1;
        if (crudeOil >= cost && magazineUpgradeLevel < 3)
        {
            crudeOil -= cost;
            magazineUpgradeLevel++;
            magazineBonus = magazineUpgradeLevel * 3; // Mỗi cấp +3 đạn
            currentWeapon?.UpdateStatsFromPlayer();
            return true;
        }
        return false;
    }

    void Die()
    {
        Debug.Log("Player chết. Game Over!");

        GameManager.Instance.GameOver();

        Destroy(gameObject);
    }

    void UpdateHpBar()
    {
        if (hpBar != null)
            hpBar.fillAmount = currentHp / maxHp; // Fill Image theo tỉ lệ máu
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Usb"))
        {
            Debug.Log("Nhặt USB → Thắng game");

            Destroy(other.gameObject); 

            GameManager.Instance.WinGame(); 
        }
    }
}