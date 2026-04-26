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

    protected override void Start()
    {
        base.Start(); // Gọi Start của Enemy (tìm Player, set máu...)
        bossSprite = GetComponent<SpriteRenderer>();
        if (bossSprite != null && phase1Sprite != null)
            bossSprite.sprite = phase1Sprite;   // Gán ảnh phase 1
    }

    protected override void Update()
    {
        base.Update(); // Gọi Update của Enemy (di chuyển)
        CheckPhaseTransition(); // Kiểm tra chuyển phase

        // Nếu đã đến lúc dùng skill
        if (Time.time >= nextSkillTime)
        {
            UseSkillsByPhase();               // Chọn skill theo phase
            nextSkillTime = Time.time + skillCooldown; // Đặt thời điểm dùng skill tiếp
        }
    }

    void CheckPhaseTransition()
    {
        float hpPercent = currentHp / maxHp; // % máu hiện tại

        // Chuyển từ phase 1 sang 2 khi máu <= 66%
        if (currentBossPhase == 1 && hpPercent <= 0.66f)
        {
            currentBossPhase = 2;
            if (phase2Sprite != null) bossSprite.sprite = phase2Sprite; // Đổi ảnh
            Debug.Log("Boss chuyển phase 2");
        }
        // Chuyển từ phase 2 sang 3 khi máu <= 33%
        else if (currentBossPhase == 2 && hpPercent <= 0.33f)
        {
            currentBossPhase = 3;
            if (phase3Sprite != null) bossSprite.sprite = phase3Sprite;
            Debug.Log("Boss chuyển phase 3");
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
}