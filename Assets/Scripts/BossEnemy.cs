using UnityEngine;

public class BossEnemy : Enemy // Kế thừa từ Enemy.cs của bạn
{
    [Header("Phase Sprites")]
    [SerializeField] private Sprite phase1Sprite; // Ảnh phase 1
    [SerializeField] private Sprite phase2Sprite; // Ảnh phase 2
    [SerializeField] private Sprite phase3Sprite; // Ảnh phase 3

    [Header("Attacks")]
    [SerializeField] private GameObject bulletPrefabs;   // Prefab đạn
    [SerializeField] private Transform firePoint;        // Điểm bắn
    [SerializeField] private float speedDanThuong = 20f; // Tốc độ đạn thường
    [SerializeField] private float speddDanVongTron = 10f; // Tốc độ đạn vòng tròn
    [SerializeField] private float hpValue = 100f;       // Lượng máu hồi phục (nếu có skill)
    [SerializeField] private GameObject miniEnemy;       // Prefab quái con
    [SerializeField] private float skillCooldown = 2f;   // Thời gian hồi chiêu
    [SerializeField] private GameObject usbPrefabs;      // Prefab chìa khóa (USB) rơi ra khi chết

    private int currentBossPhase = 1;           // Phase hiện tại (1,2,3)
    private SpriteRenderer bossSprite;          // Để thay đổi ảnh
    private float nextSkillTime;                // Thời điểm dùng skill tiếp theo
    private float hpPerPhase;

    protected override void Start()
    {
        base.Start();

        bossSprite = GetComponent<SpriteRenderer>();

        hpPerPhase = maxHp / 3f;   // chia 3 thanh
        currentHp = hpPerPhase;    // bắt đầu thanh 1

        if (bossSprite != null && phase1Sprite != null)
            bossSprite.sprite = phase1Sprite;
    }

    protected override void Update()
    {
        base.Update(); // Gọi Update của Enemy (di chuyển)
        // Nếu đã đến lúc dùng skill
        if (Time.time >= nextSkillTime)
        {
            UseSkillsByPhase();               // Chọn skill theo phase
            nextSkillTime = Time.time + skillCooldown; // Đặt thời điểm dùng skill tiếp
        }
    }

    void UseSkillsByPhase()
    {
        switch (currentBossPhase)
        {
            case 1:
                BanDanThuong(); // Phase 1 chỉ bắn thường
                break;
            case 2:
                // Phase 2: 50% bắn thường, 50% bắn vòng tròn
                if (Random.value < 0.5f)
                    BanDanThuong();
                else
                    BandanVongTron();
                break;
            case 3:
                ChonSkillNgauNhien(); // Phase 3: dùng skill ngẫu nhiên (như code cũ)
                break;
        }
    }

    // ===== COPY TOÀN BỘ HÀM KỸ NĂNG CỦA BẠN VÀO ĐÂY =====
    private void BanDanThuong()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = player.transform.position - firePoint.position;
            GameObject bullet = Instantiate(bulletPrefabs, firePoint.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(directionToPlayer * speedDanThuong);
        }
    }

    private void BandanVongTron()
    {
        const int bulletCount = 12;
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 bulletDirection = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0);
            GameObject bullet = Instantiate(bulletPrefabs, transform.position, Quaternion.identity);
            EnemyBullet enemyBullet = bullet.AddComponent<EnemyBullet>();
            enemyBullet.SetMovementDirection(bulletDirection * speddDanVongTron);
        }
    }

    private void HoiMau(float hpAmount)
    {
        currentHp = Mathf.Min(currentHp + hpAmount, maxHp);
        UpdateHpBar();
    }

    private void SinhMiniEnemy()
    {
        Instantiate(miniEnemy, transform.position, Quaternion.identity);
    }

    private void Dichchuyen()
    {
        if (player != null)
            transform.position = player.transform.position;
    }

    private void ChonSkillNgauNhien()
    {
        int randomSkill = Random.Range(0, 5);
        switch (randomSkill)
        {
            case 0: BanDanThuong(); break;
            case 1: BandanVongTron(); break;
            case 2: HoiMau(hpValue); break;
            case 3: SinhMiniEnemy(); break;
            case 4: Dichchuyen(); break;
        }
    }
    // =====================================================

    // Ghi đè Die để rơi chìa khóa
    protected override void Die()
    {
        // Tạo chìa khóa (USB) tại vị trí boss
        Instantiate(usbPrefabs, transform.position, Quaternion.identity);
        base.Die(); // Gọi Die của Enemy (báo WaveManager, hủy đối tượng)
    }
    public override void TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();

        if (currentHp <= 0)
        {
            if (currentBossPhase < 3)
            {
                NextPhase();
            }
            else
            {
                Die();
            }
        }
    }
    void NextPhase()
    {
        currentBossPhase++;

        currentHp = hpPerPhase; // hồi đầy cây máu mới
        UpdateHpBar();
        ThanhTienTrinhUI.Instance.UpdateBossHP(currentHp, hpPerPhase);

        if (currentBossPhase == 2)
        {
            bossSprite.sprite = phase2Sprite;
            enemyMoveSpeed += 1f;
            skillCooldown = 1.5f;
        }
        else if (currentBossPhase == 3)
        {
            bossSprite.sprite = phase3Sprite;
            enemyMoveSpeed += 2f;
            skillCooldown = 1f;
        }
    }
}