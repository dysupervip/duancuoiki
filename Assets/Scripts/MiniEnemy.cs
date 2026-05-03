using UnityEngine;
using System.Collections;

public class MiniBossEnemy : Enemy
{
    [Header("Cấu hình chung")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 3f;              // Tốc độ chạy riêng

    [Header("Đánh thường")]
    [SerializeField] private float meleeDamage = 20f;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeCooldown = 1.5f;

    [Header("Nhảy đập")]
    [SerializeField] private float jumpDamage = 25f;
    [SerializeField] private float jumpRange = 5f;
    [SerializeField] private float jumpDistance = 7f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 1f;
    [SerializeField] private float impactRadius = 2f;
    [SerializeField] private float jumpCooldown = 5f;

    [Header("Ném hộp quà")]
    [SerializeField] private GameObject giftBoxPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float giftBoxSpeed = 10f;
    [SerializeField] private float giftBoxDamage = 15f;
    [SerializeField] private float throwRange = 8f;
    [SerializeField] private float throwCooldown = 4f;

    // Trạng thái
    private bool isUsingSkill = false;
    private bool isDead = false;

    // Cooldown riêng
    private float nextMeleeTime = 0f;
    private float nextJumpTime = 0f;
    private float nextThrowTime = 0f;

    // Animation triggers
    private const string ANIM_MELEE_1 = "Melee1";
    private const string ANIM_MELEE_2 = "Melee2";
    private const string ANIM_MELEE_3 = "Melee3";
    private const string ANIM_THROW = "Throw";
    private const string ANIM_JUMP = "Jump";
    private const string ANIM_HURT = "Hurt";
    private const string ANIM_DIE = "Die";

    protected override void Start()
    {
        // Không gọi base.Start() vì chúng ta tự tìm player
        if (playerTransform == null)
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null) playerTransform = player.transform;
        }
        if (animator == null) animator = GetComponent<Animator>();
        currentHp = maxHp;
        UpdateHpBar();
    }

    // Ghi đè Update, KHÔNG gọi base.Update() để tránh di chuyển mặc định từ Enemy
    protected override void Update()
    {
        if (isDead) return;
        if (playerTransform == null || isUsingSkill) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= meleeRange && Time.time >= nextMeleeTime)
        {
            StartCoroutine(PerformMelee());
        }
        else if (distance > meleeRange && distance <= jumpRange && Time.time >= nextJumpTime)
        {
            StartCoroutine(PerformJump());
        }
        else if (distance > jumpRange && distance <= throwRange && Time.time >= nextThrowTime)
        {
            StartCoroutine(PerformThrow());
        }
        else if (distance > throwRange)
        {
            MoveTowardsPlayer();
        }
    }

    // Di chuyển về phía player (không dùng coroutine để liên tục)
    void MoveTowardsPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        FlipEnemy();
    }

    // ===== KỸ NĂNG =====

    IEnumerator PerformMelee()
    {
        isUsingSkill = true;
        nextMeleeTime = Time.time + meleeCooldown;

        int animIndex = Random.Range(0, 3);
        string triggerName = animIndex == 0 ? ANIM_MELEE_1 : (animIndex == 1 ? ANIM_MELEE_2 : ANIM_MELEE_3);
        animator.SetTrigger(triggerName);

        yield return new WaitForSeconds(0.3f);

        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= meleeRange)
            {
                Player player = playerTransform.GetComponent<Player>();
                if (player != null) player.TakeDamage(meleeDamage);
            }
        }

        yield return new WaitForSeconds(0.7f);
        isUsingSkill = false;
    }

    IEnumerator PerformThrow()
    {
        isUsingSkill = true;
        nextThrowTime = Time.time + throwCooldown;

        animator.SetTrigger(ANIM_THROW);
        yield return new WaitForSeconds(0.3f);

        if (giftBoxPrefab != null && throwPoint != null && playerTransform != null)
        {
            GameObject giftBox = Instantiate(giftBoxPrefab, throwPoint.position, Quaternion.identity);
            EnemyBullet bullet = giftBox.AddComponent<EnemyBullet>();
            Vector3 direction = (playerTransform.position - throwPoint.position).normalized;
            bullet.SetMovementDirection(direction * giftBoxSpeed);
            // TODO: thêm SetDamage vào EnemyBullet nếu cần
        }

        yield return new WaitForSeconds(0.7f);
        isUsingSkill = false;
    }

    IEnumerator PerformJump()
    {
        isUsingSkill = true;
        nextJumpTime = Time.time + jumpCooldown;

        animator.SetTrigger(ANIM_JUMP);

        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTransform.position;
        Vector2 jumpDir = (targetPos - startPos).normalized;
        float actualDist = Vector2.Distance(startPos, targetPos);
        if (actualDist > jumpDistance)
        {
            targetPos = startPos + (Vector3)(jumpDir * jumpDistance);
        }

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
            currentPos.y += jumpHeight * Mathf.Sin(t * Mathf.PI);
            transform.position = currentPos;
            yield return null;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D hit in hits)
        {
            Player player = hit.GetComponent<Player>();
            if (player != null) player.TakeDamage(jumpDamage);
        }

        yield return new WaitForSeconds(0.5f);
        isUsingSkill = false;
    }

    // ===== NHẬN SÁT THƯƠNG =====
    public override void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // Kích hoạt animation nhận damage
            if (animator != null)
            {
                animator.SetTrigger(ANIM_HURT);
            }
        }
    }

    // ===== CHẾT (để lại xác) =====
    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        // Cộng XP, rơi chip,... (giữ nguyên logic từ Enemy)
        Player player = FindAnyObjectByType<Player>();
        if (player != null) player.AddXP(xpReward);

        if (dropChip && chipPrefab != null)
        {
            Instantiate(chipPrefab, transform.position, Quaternion.identity);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.HandleEnemyDeath();
            WaveManager.Instance.TryDropOil(transform.position);
        }

        // Kích hoạt animation chết
        if (animator != null)
        {
            animator.SetTrigger(ANIM_DIE);
        }

        // Vô hiệu hóa collider, script, rigidbody để xác nằm yên
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        // Ngăn không cho Update chạy tiếp
        enabled = false;

        // KHÔNG Destroy(gameObject) ngay lập tức
        // Nếu muốn hủy sau một thời gian, dùng Destroy(gameObject, 5f);
    }

    // Lật mặt
    private void FlipEnemy()
    {
        if (playerTransform == null) return;
        Vector3 scale = transform.localScale;
        scale.x = playerTransform.position.x < transform.position.x ? -1 : 1;
        transform.localScale = scale;
    }
}