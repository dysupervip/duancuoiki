using UnityEngine;
using System.Collections;

public class FinalBoss : Enemy
{
    [Header("Spell 1 - Beam từ trên trời")]
    [SerializeField] private GameObject beamPrefab;        // Prefab BeamSpell1 (có Animator)
    [SerializeField] private float spell1Cooldown = 3f;
    [SerializeField] private Transform beamSpawnPoint;     // Vị trí trên đầu boss để bắt đầu bay? Thực ra beam sẽ xuất hiện trên trời, nên ta có thể spawn từ vị trí trên cao.
    // Ta sẽ tự tính vị trí spawn beam: trên đầu player một khoảng.

    [Header("Spell 2 - Quả cầu năng lượng")]
    [SerializeField] private GameObject orbPrefab;         // Prefab quả cầu (có script xử lý 4 tia)
    [SerializeField] private float spell2Cooldown = 5f;
    [Header("Skill 2")]
    public GameObject orbSpellPrefab;
    public Transform orbSpawnPoint;   // Kéo OrbSpawnPoint vào đây

    [Header("Spell 3 - Phun lửa")]
    [SerializeField] private GameObject flamePrefab;       // Prefab lửa
    [SerializeField] private float spell3Cooldown = 5f;
    [SerializeField] private float horizontalThreshold = 1f; // Sai số trục Y để coi là cùng trục ngang

    [Header("Phase HP")]
    [SerializeField] private int phase1HP = 500;
    [SerializeField] private int phase2HP = 600;
    [SerializeField] private int phase3HP = 700;


    private int currentPhase = 1;
    private float nextSpell1Time;
    private float nextSpell2Time;
    private float nextSpell3Time;

    protected override void Start()
    {
        base.Start();
        maxHp = phase3HP; // Tổng HP toàn bộ các phase cộng lại? Hay mỗi phase có HP riêng? Bạn nói "HP của boss là 500 (phase 1), 600 (phase 2), 700 (phase 3)". Thông thường tổng HP sẽ là 500+600+700 = 1800, nhưng boss sẽ chuyển phase khi HP giảm đến mức nhất định. Tôi sẽ thiết lập maxHp = 1800 và các mốc phase.
        // Giả sử phase 1 bắt đầu, HP hiện tại = maxHp.
        currentHp = maxHp;
        UpdateHpBar();
        // Đặt thời gian cho các spell
        nextSpell1Time = Time.time + spell1Cooldown;
    }

    protected override void Update()
    {
        base.Update(); // Gọi MoveToPlayer từ Enemy, boss sẽ di chuyển về phía player? Bạn không yêu cầu di chuyển, có thể giữ nguyên.
        CheckPhase();
        HandleSpells();
    }

    void CheckPhase()
    {
        float hpPercent = (float)currentHp / maxHp;
        // Phase 1: 100% -> 33%? Tôi chia theo HP tổng: phase3HP=700, tổng 1800.
        // Bạn có thể định nghĩa rõ hơn. Tạm thời:
        if (currentHp > phase2HP + phase3HP) // > 1300 => phase 1
            currentPhase = 1;
        else if (currentHp > phase3HP) // > 700 => phase 2
            currentPhase = 2;
        else
            currentPhase = 3;
    }

    void HandleSpells()
    {
        if (currentPhase >= 1 && Time.time >= nextSpell1Time)
        {
            CastSpell1();
            nextSpell1Time = Time.time + spell1Cooldown;
        }
        if (currentPhase >= 2 && Time.time >= nextSpell2Time)
        {
            CastSpell2();
            nextSpell2Time = Time.time + spell2Cooldown;
        }
        if (currentPhase >= 3 && Time.time >= nextSpell3Time && IsPlayerOnSameHorizontal())
        {
            CastSpell3();
            nextSpell3Time = Time.time + spell3Cooldown;
        }
    }

    void CastSpell1()
    {
        // Chọn ngẫu nhiên góc 0 hoặc 45
        float angle = Random.Range(0, 2) == 0 ? 0f : 45f;
        // Vị trí spawn: trên đầu player một khoảng, ví dụ (player.x, player.y + 5)
        Vector3 spawnPos = player.transform.position + Vector3.up * 5f;
        GameObject beam = Instantiate(beamPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
        // Beam sẽ tự chạy animation và tự hủy, không cần thêm gì.
    }

    void CastSpell2()
    {
        // Spawn quả cầu trên đầu boss, sau đó quả cầu sẽ tự bắn 4 tia xoay.
        // Cần script cho orb.
        Vector3 spawnPos = transform.position + Vector3.up * 2f;
        Instantiate(orbPrefab, spawnPos, Quaternion.identity);
    }
    // Hàm này sẽ được gọi từ Animation Event của boss
    public void SpawnOrbSpell()
    {
        if (orbSpellPrefab != null && orbSpawnPoint != null)
        {
            Instantiate(orbSpellPrefab, orbSpawnPoint.position, Quaternion.identity);
        }
    }

    void CastSpell3()
    {
        // Phun lửa: spawn flame từ tay boss về phía player.
        Vector3 direction = (player.transform.position - transform.position).normalized;
        // Xác định hướng tay: dựa vào hướng mặt (giả sử boss có spriteRenderer.flipX)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = transform.position + direction * 1f; // cách một chút
        GameObject flame = Instantiate(flamePrefab, spawnPos, Quaternion.Euler(0, 0, angle));
        // Flame sẽ tự hủy sau thời gian ngắn.
    }

    bool IsPlayerOnSameHorizontal()
    {
        return Mathf.Abs(player.transform.position.y - transform.position.y) < horizontalThreshold;
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        // Có thể thêm hiệu ứng khi chuyển phase
    }
} 