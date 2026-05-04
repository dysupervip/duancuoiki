using UnityEngine;
using System.Collections;

public class FinalBoss : Enemy
{
    [Header("Spell 1 - Beam từ trên trời")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float spell1Cooldown = 3f;
    [SerializeField] private float beamHeight = 6f; // Khoảng cách từ chân tia đến điểm spawn (đỉnh)

    [Header("Spell 2 - Quả cầu năng lượng (4 tia xoay)")]
    [SerializeField] private GameObject orbSpellPrefab;
    [SerializeField] private Transform orbSpawnPoint;
    [SerializeField] private float spell2Cooldown = 5f;

    [Header("Spell 3 - Phun lửa")]
    [SerializeField] private GameObject flamePrefab;
    [SerializeField] private float spell3Cooldown = 5f;
    [SerializeField] private float horizontalThreshold = 1.5f;

    [Header("Phase HP")]
    [SerializeField] private int phase1HP = 500;
    [SerializeField] private int phase2HP = 600;
    [SerializeField] private int phase3HP = 700;

    // Nội bộ
    private int currentPhase = 1;
    private float nextSpell1Time;
    private float nextSpell2Time;
    private float nextSpell3Time;
    private bool spell1Ready = false;
    private bool spell2Ready = false;
    private bool spell3Ready = false;
    private bool isUsingSkill = false;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("[FinalBoss] Không tìm thấy Animator!");
        maxHp = phase1HP + phase2HP + phase3HP; // Tổng 1800
        currentHp = maxHp;
        UpdateHpBar();

        nextSpell1Time = Time.time + spell1Cooldown;
        nextSpell2Time = Time.time + spell2Cooldown;
        nextSpell3Time = Time.time + spell3Cooldown;
    }

    protected override void Update()
    {
        base.Update(); // Giữ lại để boss di chuyển (MoveToPlayer)

        if (isDead) return;

        CheckPhase();
        HandleCooldowns();

        // Thực hiện spell nếu có sẵn và không đang dùng skill
        if (!isUsingSkill)
        {
            if (spell1Ready)
            {
                StartCoroutine(PerformSpell1());
            }
            else if (spell2Ready)
            {
                StartCoroutine(PerformSpell2());
            }
            else if (spell3Ready)
            {
                StartCoroutine(PerformSpell3());
            }
        }
    }

    // ===== QUẢN LÝ PHASE =====
    void CheckPhase()
    {
        if (currentHp > phase2HP + phase3HP) // > 1300
            currentPhase = 1;
        else if (currentHp > phase3HP) // > 700
            currentPhase = 2;
        else
            currentPhase = 3;
    }

    // ===== QUẢN LÝ COOLDOWN =====
    void HandleCooldowns()
    {
        if (Time.time >= nextSpell1Time && !spell1Ready && currentPhase >= 1)
            spell1Ready = true;
        if (Time.time >= nextSpell2Time && !spell2Ready && currentPhase >= 2)
            spell2Ready = true;
        if (Time.time >= nextSpell3Time && !spell3Ready && currentPhase >= 3 && IsPlayerOnSameHorizontal())
            spell3Ready = true;
    }

    // ===== THỰC HIỆN SPELL (COROUTINE) =====
    IEnumerator PerformSpell1()
    {
        isUsingSkill = true;
        spell1Ready = false;
        nextSpell1Time = Time.time + spell1Cooldown;

        animator.SetTrigger("Spell1");
        // Animation sẽ gọi CastSpell1() qua Animation Event
        yield return new WaitForSeconds(1.5f); // Chờ animation kết thúc (có thể thay bằng Animation Event)
        isUsingSkill = false;
    }

    IEnumerator PerformSpell2()
    {
        isUsingSkill = true;
        spell2Ready = false;
        nextSpell2Time = Time.time + spell2Cooldown;
        animator.SetTrigger("Spell2");
        // Animation sẽ gọi SpawnOrbSpell() qua Animation Event
        yield return new WaitForSeconds(2f);
        isUsingSkill = false;
    }

    IEnumerator PerformSpell3()
    {
        isUsingSkill = true;
        spell3Ready = false;
        nextSpell3Time = Time.time + spell3Cooldown;
        animator.SetTrigger("Spell3");
        // Animation sẽ gọi CastSpell3() qua Animation Event
        yield return new WaitForSeconds(1f);
        isUsingSkill = false;
    }

    // ===== HÀM GỌI TỪ ANIMATION EVENT =====
    public void CastSpell1()
    {
        if (beamPrefab == null || player == null) return;

        // Lấy vị trí player ngay lúc này
        Vector3 targetPos = player.transform.position;
        // Spawn beam ở trên cao, ngay phía trên player
        Vector3 spawnPos = targetPos + Vector3.up * beamHeight;
        // Chọn thẳng hoặc xiên 45°
        float angle = Random.Range(0, 2) == 0 ? 0f : 45f;
        GameObject beam = Instantiate(beamPrefab, spawnPos, Quaternion.Euler(0, 0, angle));

        // Điều chỉnh cho chân tia khớp với targetPos
        Transform footTransform = beam.transform.Find("Foot");
        if (footTransform != null)
        {
            Vector3 offset = beam.transform.position - footTransform.position;
            beam.transform.position = targetPos + offset;
        }
        else
        {
            beam.transform.position = targetPos;
        }
    }

    public void SpawnOrbSpell()
    {
        if (orbSpellPrefab && orbSpawnPoint)
            Instantiate(orbSpellPrefab, orbSpawnPoint.position, Quaternion.identity);
    }

    public void CastSpell3()
    {
        if (flamePrefab == null || player == null) return;
        Vector3 dir = (player.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector3 spawnPos = transform.position + dir * 1.5f;
        Instantiate(flamePrefab, spawnPos, Quaternion.Euler(0, 0, angle));
    }

    // ===== KIỂM TRA CÙNG TRỤC NGANG =====
    bool IsPlayerOnSameHorizontal()
    {
        if (player == null) return false;
        return Mathf.Abs(player.transform.position.y - transform.position.y) < horizontalThreshold;
    }

    // ===== KẾ THỪA TAKE DAMAGE =====
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        // Có thể thêm hiệu ứng khi chuyển phase (vd: rung màn hình, flash đỏ)
    }

    // ===== CHẾT =====
    protected override void Die()
    {
        base.Die();
        // Nếu bạn muốn boss rơi USB key, hãy thêm Instantiate(usbPrefab, transform.position, Quaternion.identity);
        // Tạm thời dùng logic có sẵn trong Enemy.cs
    }
}