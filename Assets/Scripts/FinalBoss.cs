using UnityEngine;
using System.Collections;

public class FinalBoss : Enemy
{
    [Header("Spell 1 - Beam từ trên trời")]
    [SerializeField] private GameObject beamStraightPrefab;
    [SerializeField] private GameObject beamDiagonalPrefab;
    [SerializeField] private float spell1Cooldown = 3f;
    [SerializeField] private float spell1Range = 10f;
    [SerializeField] private float beamHeight = 6f;

    [Header("Spell 2 - Quả cầu năng lượng")]
    [SerializeField] private GameObject beamSpell2Prefab;
    [SerializeField] private Transform orbSpawnPoint;
    [SerializeField] private float spell2Cooldown = 5f;
    [SerializeField] private float spell2Range = 8f;
    [SerializeField] private int spell2BeamCount = 4;
    [SerializeField] private float spell2RotationDuration = 1f;
    [SerializeField] private float spell2RotationAmount = 90f;

    [Header("Spell 3 - Phun lửa")]
    [SerializeField] private GameObject flamePrefab;
    [SerializeField] private float spell3Cooldown = 5f;
    [SerializeField] private float spell3Range = 4f;

    [Header("Phase HP")]
    [SerializeField] private int phase1HP = 500;
    [SerializeField] private int phase2HP = 600;
    [SerializeField] private int phase3HP = 700;

    [Header("Reward")]
    [SerializeField] private GameObject usbPrefab;

    private int currentPhase = 1;
    private float nextSpell1Time;
    private float nextSpell2Time;
    private float nextSpell3Time;
    private bool isCasting = false;
    private Animator animator;


    protected override void Start()
    {
        maxHp = phase1HP;

        base.Start();

        animator = GetComponent<Animator>();

        currentHp = maxHp;

        UpdateHpBar();

        nextSpell1Time = 0f;
        nextSpell2Time = 0f;
        nextSpell3Time = 0f;
    }

    protected override void Update()
    {
        if (!isCasting)
        {
            base.Update();   // Di chuyển (gọi FlipEnemy đã ghi đè)
        }

        if (isDead) return;

        if (!isCasting)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);

            if (Time.time >= nextSpell1Time && currentPhase >= 1 && dist <= spell1Range)
            {
                StartCoroutine(PerformSpell1());
                return;
            }

            if (Time.time >= nextSpell2Time && currentPhase >= 2 && dist <= spell2Range)
            {
                StartCoroutine(PerformSpell2());
                return;
            }

            if (Time.time >= nextSpell3Time && currentPhase >= 3 && dist <= spell3Range)
            {
                StartCoroutine(PerformSpell3());
                return;
            }
        }
    }

    // Ghi đè FlipEnemy: mặt mặc định hướng TRÁI
    protected new void FlipEnemy()
    {
         if (player != null)
        {

            if (player.transform.position.x > transform.position.x)
                transform.localScale = new Vector3(-1, 1, 1);
            else
                transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void CheckPhase()
    {
        // Phase 1 -> 2
        if (currentPhase == 1 && currentHp <= 0)
        {
            currentPhase = 2;

            maxHp = phase2HP;
            currentHp = maxHp;

            UpdateHpBar();
            ThanhTienTrinhUI.Instance?.ResetBar();

            Debug.Log("PHASE 2");
        }

        // Phase 2 -> 3
        else if (currentPhase == 2 && currentHp <= 0)
        {
            currentPhase = 3;

            maxHp = phase3HP;
            currentHp = maxHp;

            UpdateHpBar();
            ThanhTienTrinhUI.Instance?.ResetBar();
            Debug.Log("PHASE 3");
        }
    }

    // ===== SPELL 1 =====
    IEnumerator PerformSpell1()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossBeam();
        isCasting = true;
        nextSpell1Time = Time.time + spell1Cooldown;

        FlipEnemy();                   // Đảm bảo hướng mặt trước khi animation
        animator?.SetTrigger("Spell1");

        // KHÔNG gọi CastSpell1 ở đây !!
        // Beam sẽ được tạo bởi Animation Event

        yield return new WaitForSeconds(2f); // Tổng thời gian animation (bạn chỉnh theo thực tế)
        FlipEnemy();
        isCasting = false;
    }

    // Hàm này được gọi từ Animation Event
    public void SpawnBeamAtSavedPosition()
    {
        if (player == null) return;

        // Lưu vị trí Player ngay lúc này
        Vector3 savedPos = player.transform.position;
        CastSpell1At(savedPos);
    }

    void CastSpell1At(Vector3 targetPos)
    {
        GameObject selectedPrefab = Random.Range(0, 2) == 0 ? beamStraightPrefab : beamDiagonalPrefab;
        if (selectedPrefab == null) return;

        Vector3 spawnPos = targetPos + Vector3.up * beamHeight;
        GameObject beam = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

        Transform footTransform = beam.transform.Find("Foot");
        if (footTransform != null)
        {
            Vector3 offset = beam.transform.position - footTransform.position;
            beam.transform.position = targetPos + offset;
        }
        else
        {
            beam.transform.position = targetPos;   // fallback
        }
    }

    // ===== SPELL 2 =====
    IEnumerator PerformSpell2()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossOrb();
        isCasting = true;
        nextSpell2Time = Time.time + spell2Cooldown;
        FlipEnemy();
        animator?.SetTrigger("Spell2");
        SpawnOrbSpell();
        yield return new WaitForSeconds(2f);
        FlipEnemy();
        isCasting = false;
    }

    void SpawnOrbSpell()
    {
        if (beamSpell2Prefab == null || orbSpawnPoint == null) return;

        GameObject orbContainer = new GameObject("OrbContainer");
        orbContainer.transform.position = orbSpawnPoint.position;
        orbContainer.transform.rotation = Quaternion.identity;

        float angleStep = 360f / spell2BeamCount;
        for (int i = 0; i < spell2BeamCount; i++)
        {
            float angle = i * angleStep;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Instantiate(beamSpell2Prefab, orbContainer.transform.position, rot, orbContainer.transform);
        }

        StartCoroutine(RotateOrbContainer(orbContainer));
    }

    IEnumerator RotateOrbContainer(GameObject container)
    {
        float startAngle = container.transform.eulerAngles.z;
        float targetAngle = startAngle + spell2RotationAmount;
        float elapsed = 0f;

        while (elapsed < spell2RotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spell2RotationDuration;
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            container.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }
        Destroy(container);
    }

    // ===== SPELL 3 =====
    IEnumerator PerformSpell3()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBossFire();
        isCasting = true;
        nextSpell3Time = Time.time + spell3Cooldown;
        FlipEnemy();
        animator?.SetTrigger("Spell3");
        CastSpell3();
        yield return new WaitForSeconds(1f);
        FlipEnemy();
        isCasting = false;
    }

    void CastSpell3()
    {
        if (flamePrefab == null || player == null) return;
        Vector3 dir = (player.transform.position - transform.position).normalized;
        Vector3 spawnPos = transform.position + dir * 1.5f;
        GameObject flame = Instantiate(flamePrefab, spawnPos, Quaternion.identity);
        FlameBehaviour fb = flame.GetComponent<FlameBehaviour>();
        if (fb != null) fb.SetDirection(dir);
    }

    

    public override void TakeDamage(float damage)
    {
        Debug.Log("Boss tru mau");

        if (isDead) return;

        Debug.Log("HP truoc: " + currentHp);

        currentHp -= damage;

        Debug.Log("HP sau: " + currentHp);

        currentHp = Mathf.Max(currentHp, 0);

        UpdateHpBar();

        if (currentHp <= 0)
        {
            if (currentPhase < 3)
            {
                CheckPhase();
            }
            else
            {
                Die();
            }
        }
        else
        {
            animator?.SetTrigger("Hurt");
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        animator?.SetTrigger("Die");
        Collider2D col = GetComponent<Collider2D>();
        if (usbPrefab != null)
        {
            Instantiate(usbPrefab, transform.position, Quaternion.identity);
        }
        if (col) col.enabled = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;
        enabled = false;
        Destroy(gameObject, 3f);
    }
}