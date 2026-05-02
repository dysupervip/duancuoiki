using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Base stats")]
    public float baseDamage = 10f;          // Sát thương gốc
    public float baseFireRate = 0.5f;       // Thời gian giữa hai phát bắn
    public float baseReloadTime = 1.5f;     // Thời gian nạp đạn gốc
    public int baseMagazine = 12;           // Cỡ băng đạn gốc
    public int baseTotalAmmo = 60;          // Tổng đạn dự trữ

    [Header("References")]
    [SerializeField] protected Transform firePos;        // Vị trí đầu nòng
    [SerializeField] protected GameObject bulletPrefab;  // Prefab viên đạn

    // Chỉ số thực tế (đã cộng bonus từ Player)
    protected float damage;
    protected float fireRate;
    protected float reloadTime;
    protected int magazineSize;
    protected int totalAmmo;

    protected int currentAmmo;          // Đạn hiện tại trong băng
    protected float nextShotTime;       // Thời điểm được bắn tiếp
    protected bool isReloading;         // Đang nạp đạn?
    public bool IsReloading => isReloading;
    protected float reloadTimer;        // Đếm ngược nạp đạn

    // Tiến trình nạp đạn (0..1) để hiển thị UI
    public float ReloadProgress => isReloading ? (1f - reloadTimer / reloadTime) : 0f;

    // Sự kiện để CursorManager lắng nghe
    public System.Action OnReloadStarted;
    public System.Action OnReloadFinished;

    // Biến để đánh dấu súng clone (dùng cho Dual Wield)
    [HideInInspector] public bool isClone = false;

    protected virtual void Start()
    {
        UpdateStatsFromPlayer();    // Áp dụng chỉ số từ Player (level dầu, etc.)
        currentAmmo = magazineSize; // Nạp đầy băng ban đầu
        totalAmmo = baseTotalAmmo;
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetCurrentWeapon(this); // Đăng ký với CursorManager
    }

    public void UpdateStatsFromPlayer()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            // Sát thương = base + (cấp sát thương * 10)
            damage = baseDamage + player.damageLevel * 10f;
            // Tốc độ bắn giữ nguyên (không nâng cấp)
            fireRate = baseFireRate;
            // Nạp đạn = base - (cấp nạp * 0.3f), tối thiểu 0.1s
            reloadTime = Mathf.Max(0.1f, baseReloadTime - player.reloadLevel * 0.3f);
            // Băng đạn = base + (cấp băng * 5)
            magazineSize = baseMagazine + player.magazineLevel * 5;
        }
        else
        {
            // Mặc định nếu không tìm thấy Player
            damage = baseDamage;
            fireRate = baseFireRate;
            reloadTime = baseReloadTime;
            magazineSize = baseMagazine;
        }
    }

    protected virtual void Update()
    {
        // Nếu là súng clone (Dual Wield), bỏ qua toàn bộ input để tránh xung đột
        if (isClone)
            return;

        RotateGun();        // Hàm ảo – lớp con tự định nghĩa
        HandleReload();     // Xử lý nạp đạn
        HandleShooting();   // Xử lý bắn
    }

    protected abstract void RotateGun();

    void HandleShooting()
    {
        // Điều kiện bắn: giữ chuột trái, còn đạn, hết cooldown, không đang nạp
        if (Input.GetMouseButton(0) && currentAmmo > 0 && Time.time >= nextShotTime && !isReloading)
        {
            nextShotTime = Time.time + fireRate;
            Instantiate(bulletPrefab, firePos.position, firePos.rotation);

            // === Xử lý tiêu hao đạn ===
            // Tìm Player trong scene
            Player player = FindAnyObjectByType<Player>();
            // Nếu Player không tồn tại hoặc Player chưa thăng hoa => trừ đạn bình thường
            if (player == null || !player.IsAscended)
            {
                currentAmmo--;
            }
            // Nếu Player đã thăng hoa => không trừ đạn (vô hạn đạn)

            // Tự động nạp đạn khi hết đạn trong băng
            if (currentAmmo <= 0 && totalAmmo > 0 && !isReloading)
            {
                StartReload();
            }
        }
    }

    void HandleReload()
    {
        // Nạp thủ công: chuột phải, chưa đầy băng, không đang nạp, còn đạn dự trữ
        if (Input.GetMouseButtonDown(1) && currentAmmo < magazineSize && !isReloading && totalAmmo > 0)
        {
            StartReload();
        }

        // Nếu đang nạp
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;    // Giảm đồng hồ
            if (reloadTimer <= 0)
            {
                // Nạp xong: tính số đạn cần nạp
                int ammoNeeded = magazineSize - currentAmmo;
                int ammoToReload = Mathf.Min(ammoNeeded, totalAmmo);

                currentAmmo += ammoToReload;
                totalAmmo -= ammoToReload;

                isReloading = false;
                OnReloadFinished?.Invoke();   // Báo sự kiện kết thúc nạp
            }
        }
    }

    void StartReload()
    {
        if (totalAmmo <= 0) return; // Không còn đạn dự trữ
        isReloading = true;
        reloadTimer = reloadTime;    // Đặt lại thời gian nạp
        OnReloadStarted?.Invoke();   // Báo sự kiện bắt đầu nạp
    }
}