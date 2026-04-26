using UnityEngine;

public abstract class WeaponBase : MonoBehaviour // abstract nghĩa là không dùng trực tiếp, phải kế thừa
{
    [Header("Base stats")] // Các chỉ số cơ bản của súng (có thể chỉnh trong Inspector)
    public float baseDamage = 10f;          // Sát thương gốc
    public float baseFireRate = 0.3f;       // Thời gian giữa hai phát bắn (giây)
    public float baseReloadTime = 1.5f;     // Thời gian nạp đạn
    public int baseMagazine = 12;           // Số đạn tối đa trong băng

    [Header("References")]
    [SerializeField] protected Transform firePos;      // Vị trí đầu nòng (nơi viên đạn xuất hiện)
    [SerializeField] protected GameObject bulletPrefab; // Prefab viên đạn

    // Chỉ số thực tế sau khi cộng bonus từ Player
    protected float damage;
    protected float fireRate;
    protected float reloadTime;
    protected int magazineSize;

    protected int currentAmmo;          // Đạn hiện tại trong băng
    protected float nextShotTime;       // Thời điểm có thể bắn tiếp
    protected bool isReloading;         // Đang nạp đạn?
    protected float reloadTimer;        // Đếm ngược thời gian nạp

    protected virtual void Start()
    {
        UpdateStatsFromPlayer();     // Lấy bonus từ Player (nếu có)
        currentAmmo = magazineSize;  // Nạp đầy đạn
    }

    // Gọi khi cần cập nhật chỉ số (ví dụ sau khi nâng cấp bằng dầu)
    public void UpdateStatsFromPlayer()
    {
        Player player = FindAnyObjectByType<Player>(); // Tìm Player trong scene
        if (player != null)
        {
            // Áp dụng bonus
            damage = baseDamage * (1f + player.damageBonus);
            fireRate = baseFireRate * (1f - player.fireRateBonus);
            reloadTime = baseReloadTime * (1f - player.reloadSpeedBonus);
            magazineSize = baseMagazine + player.magazineBonus;
        }
        else
        {
            // Nếu không tìm thấy Player (ví dụ lúc test) thì dùng chỉ số gốc
            damage = baseDamage;
            fireRate = baseFireRate;
            reloadTime = baseReloadTime;
            magazineSize = baseMagazine;
        }
    }

    protected virtual void Update()
    {
        RotateGun();         // Xoay súng theo chuột (hàm ảo, lớp con sẽ override)
        HandleShooting();    // Xử lý bắn
        HandleReload();      // Xử lý nạp đạn
    }

    protected abstract void RotateGun(); // Bắt buộc lớp con phải định nghĩa cách xoay

    void HandleShooting()
    {
        // Chuột trái giữ, còn đạn, hết thời gian hồi, không đang nạp
        if (Input.GetMouseButton(0) && currentAmmo > 0 && Time.time >= nextShotTime && !isReloading)
        {
            nextShotTime = Time.time + fireRate; // Đặt thời điểm bắn tiếp theo
            Instantiate(bulletPrefab, firePos.position, firePos.rotation); // Tạo viên đạn
            currentAmmo--;
            if (currentAmmo <= 0) StartReload(); // Tự động nạp khi hết đạn
        }
    }

    void HandleReload()
    {
        // Bấm phím R để nạp đạn thủ công
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize && !isReloading)
        {
            StartReload();
        }

        // Nếu đang nạp, đếm ngược thời gian
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                currentAmmo = magazineSize; // Nạp đầy
                isReloading = false;
            }
        }
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;
    }
}