using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] protected AudioClip shootSound;
    [SerializeField] protected AudioClip reloadSound;
    protected AudioSource audioSource;

    [Header("Base stats")]
    public float baseDamage = 10f;
    public float baseFireRate = 0.5f;
    public float baseReloadTime = 1.5f;
    public int baseMagazine = 12;
    // Đã xóa baseTotalAmmo và totalAmmo

    [Header("References")]
    [SerializeField] protected Transform firePos;
    [SerializeField] protected GameObject bulletPrefab;

    // Chỉ số thực tế
    protected float damage;
    protected float fireRate;
    protected float reloadTime;
    protected int magazineSize;

    protected int currentAmmo;          // Đạn hiện tại trong băng
    protected float nextShotTime;
    protected bool isReloading;
    public bool IsReloading => isReloading;
    protected float reloadTimer;

    public float ReloadProgress => isReloading ? (1f - reloadTimer / reloadTime) : 0f;

    public System.Action OnReloadStarted;
    public System.Action OnReloadFinished;

    [HideInInspector] public bool isClone = false;

    [Header("UI Info")]
    public Sprite weaponIcon;
    [TextArea]
    public string weaponDescription;
    public string weaponName;

    protected virtual void Start()
    {
        UpdateStatsFromPlayer();
        currentAmmo = magazineSize; // Nạp đầy băng ban đầu
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetCurrentWeapon(this);
        audioSource = GetComponent<AudioSource>();
    }

    public void UpdateStatsFromPlayer()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            damage = baseDamage + player.damageLevel * 10f;
            fireRate = baseFireRate;
            reloadTime = Mathf.Max(0.1f, baseReloadTime - player.reloadLevel * 0.3f);
            magazineSize = baseMagazine + player.magazineLevel * 5;
        }
        else
        {
            damage = baseDamage;
            fireRate = baseFireRate;
            reloadTime = baseReloadTime;
            magazineSize = baseMagazine;
        }
    }

    protected virtual void Update()
    {
        if (isClone) return;
        RotateGun();
        HandleReload();
        HandleShooting();
    }

    protected abstract void RotateGun();

    void HandleShooting()
    {
        if (Input.GetMouseButton(0) && currentAmmo > 0 && Time.time >= nextShotTime && !isReloading)
        {
            nextShotTime = Time.time + fireRate;
            Instantiate(bulletPrefab, firePos.position, firePos.rotation);
            if (shootSound != null)
                audioSource.PlayOneShot(shootSound);

            // Xử lý tiêu hao đạn (chỉ khi chưa thăng hoa)
            Player player = FindAnyObjectByType<Player>();
            if (player == null || !player.IsAscended)
            {
                currentAmmo--;
            }

            // Tự động nạp khi hết đạn trong băng (không cần kiểm tra totalAmmo)
            if (currentAmmo <= 0 && !isReloading)
            {
                StartReload();
            }
        }
    }

    void HandleReload()
    {
        // Nạp thủ công (chuột phải) hoặc tự động đều được
        if (Input.GetMouseButtonDown(1) && currentAmmo < magazineSize && !isReloading)
        {
            StartReload();
        }

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0)
            {
                currentAmmo = magazineSize; // Nạp đầy băng, không giới hạn đạn dự trữ
                isReloading = false;
                OnReloadFinished?.Invoke();
            }
        }
    }

    void StartReload()
    {
        // Không cần kiểm tra totalAmmo, luôn có thể nạp
        isReloading = true;
        reloadTimer = reloadTime;
        OnReloadStarted?.Invoke();
    }
}