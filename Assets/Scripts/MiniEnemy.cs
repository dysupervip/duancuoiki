using UnityEngine;
using System.Collections;

public class MiniBossEnemy : Enemy
{
    [Header("Tham chiếu")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerTransform;

    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 3f;

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

    // Tên các Trigger trong Animator (đặt đúng với tên bạn đã tạo)
    private const string TRIGGER_LEFT_PUNCH = "LeftPunch";   // Melee1
    private const string TRIGGER_RIGHT_PUNCH = "RightPunch"; // Melee2
    private const string TRIGGER_KICK = "Kick";             // Melee3
    private const string TRIGGER_THROW = "ThrowGift";       // Throw
    private const string TRIGGER_JUMP = "LandingPunch";     // Jump
    private const string TRIGGER_HURT = "Damage";           // Hurt
    private const string TRIGGER_DIE = "Die";               // Die
    private const string BOOL_ISMOVING = "isMoving";        // Bool điều khiển Run/Idle

   protected override void Start()
    {
        if (playerTransform == null)
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null) playerTransform = player.transform;
        }
        if (animator == null) animator = GetComponent<Animator>();
        currentHp = maxHp;
        UpdateHpBar();
        
        // Reset scale về đúng chuẩn trước khi lật mặt
        transform.localScale = Vector3.one;
        FlipEnemy(); // Sau đó mới lật mặt theo hướng player
    }

    protected override void Update()
    {
        if (isDead || playerTransform == null || isUsingSkill) return;

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
        else
        {
            // Đứng yên, tắt animation chạy
            if (animator != null) animator.SetBool(BOOL_ISMOVING, false);
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        FlipEnemy();
        if (animator != null) animator.SetBool(BOOL_ISMOVING, true);
    }

    IEnumerator PerformMelee()
    {
        isUsingSkill = true;
        nextMeleeTime = Time.time + meleeCooldown;
        if (animator != null) animator.SetBool(BOOL_ISMOVING, false);

        int animIndex = Random.Range(0, 3);
        string triggerName = animIndex == 0 ? TRIGGER_LEFT_PUNCH : (animIndex == 1 ? TRIGGER_RIGHT_PUNCH : TRIGGER_KICK);
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
        if (animator != null) animator.SetBool(BOOL_ISMOVING, false);

        animator.SetTrigger(TRIGGER_THROW);
        yield return new WaitForSeconds(0.3f);

        if (giftBoxPrefab != null && throwPoint != null && playerTransform != null)
        {
            GameObject giftBoxObj = Instantiate(giftBoxPrefab, throwPoint.position, Quaternion.identity);
            GiftBox giftBox = giftBoxObj.GetComponent<GiftBox>();
            if (giftBox == null) giftBox = giftBoxObj.AddComponent<GiftBox>();

            Vector3 direction = (playerTransform.position - throwPoint.position).normalized;
            giftBox.Initialize(giftBoxDamage, giftBoxSpeed, direction);
        }

        yield return new WaitForSeconds(0.7f);
        isUsingSkill = false;
    }

    IEnumerator PerformJump()
    {
        isUsingSkill = true;
        nextJumpTime = Time.time + jumpCooldown;
        if (animator != null) animator.SetBool(BOOL_ISMOVING, false);

        animator.SetTrigger(TRIGGER_JUMP);

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
        else if (animator != null)
        {
            animator.SetTrigger(TRIGGER_HURT);
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

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

        if (animator != null)
        {
            animator.SetTrigger(TRIGGER_DIE);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;
        enabled = false;
    }

    private void FlipEnemy()
    {
        if (playerTransform == null) return;
        Vector3 scale = transform.localScale;
        scale.x = playerTransform.position.x < transform.position.x ? -1 : 1;
        transform.localScale = scale;
    }
}