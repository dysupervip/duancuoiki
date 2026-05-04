using UnityEngine;
using System.Collections;

public class MiniBossEnemy : Enemy
{
    [Header("Tham chiếu")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerTransform;

    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Đánh thường")]
    [SerializeField] private float meleeDamage = 20f;
    [SerializeField] private float meleeRange = 2.5f;
    [SerializeField] private float meleeCooldown = 1f;

    [Header("Nhảy đập")]
    [SerializeField] private float jumpDamage = 25f;
    [SerializeField] private float jumpRange = 6f;
    [SerializeField] private float jumpDistance = 7f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 0.6f;       // Thời gian bay (giây) – đã giảm
    [SerializeField] private float impactRadius = 2f;
    [SerializeField] private float jumpCooldown = 5f;

    [Header("Ném hộp quà")]
    [SerializeField] private GameObject giftBoxPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float giftBoxSpeed = 10f;
    [SerializeField] private float giftBoxDamage = 15f;
    [SerializeField] private float throwRange = 10f;
    [SerializeField] private float throwCooldown = 4f;

    [Header("Hồi chiêu chung")]
    [SerializeField] private float specialSkillCooldown = 2f;

    private enum State { Moving, Attacking, Jumping, Throwing, Dead }
    private State currentState = State.Moving;

    private Rigidbody2D rb;

    private float nextMeleeTime = 0f;
    private float nextJumpTime = 0f;
    private float nextThrowTime = 0f;
    private float nextSpecialSkillTime = 0f;

    private bool isHurtPlaying = false;   // Đang chạy animation Hurt

    // Tên trigger – phải khớp hoàn toàn với Animator
    private const string TRIG_LEFT = "LeftPunch";
    private const string TRIG_RIGHT = "RightPunch";
    private const string TRIG_KICK = "Kick";
    private const string TRIG_JUMP = "LandingPunch";
    private const string TRIG_THROW = "ThrowGift";
    private const string TRIG_HURT = "Damage";
    private const string TRIG_DIE = "Die";
    private const string BOOL_MOVING = "isMoving";

    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.bodyType = RigidbodyType2D.Kinematic;

        if (playerTransform == null)
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null) playerTransform = player.transform;
            else Debug.LogError("[MiniBoss] Không tìm thấy Player!");
        }
        if (animator == null) animator = GetComponent<Animator>();

        currentHp = maxHp;
        UpdateHpBar();
        transform.localScale = Vector3.one;
        FlipEnemy();

        currentState = State.Moving;
        if (animator) animator.SetBool(BOOL_MOVING, true);
    }

    protected override void Update()
    {
        if (isDead || playerTransform == null) return;

        // Không cho phép dùng skill khi đang tấn công, nhảy, ném.
        // Khi đang Hurt (isHurtPlaying = true), boss vẫn có thể di chuyển và sẽ tự động ngắt Hurt nếu muốn dùng skill.
        if (currentState == State.Attacking || currentState == State.Jumping || currentState == State.Throwing)
            return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= meleeRange)
        {
            // Dừng chạy, đứng yên và đánh thường
            if (animator) animator.SetBool(BOOL_MOVING, false);
            FlipEnemy();

            if (Time.time >= nextMeleeTime)
            {
                // Ngắt Hurt nếu đang chạy (giúp animation skill được ưu tiên)
                if (isHurtPlaying) isHurtPlaying = false;
                StartCoroutine(DoMelee());
            }
        }
        else
        {
            // Chạy về phía Player
            if (currentState != State.Moving)
            {
                currentState = State.Moving;
                if (animator) animator.SetBool(BOOL_MOVING, true);
            }
            MoveTowardsPlayer();

            // Chỉ dùng skill đặc biệt khi hết cooldown chung
            if (Time.time >= nextSpecialSkillTime)
            {
                if (distance <= jumpRange && Time.time >= nextJumpTime)
                {
                    if (isHurtPlaying) isHurtPlaying = false;
                    StartCoroutine(DoJump());
                }
                else if (distance <= throwRange && Time.time >= nextThrowTime)
                {
                    if (isHurtPlaying) isHurtPlaying = false;
                    StartCoroutine(DoThrow());
                }
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        Vector2 newPos = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        if (rb) rb.MovePosition(newPos);
        else transform.position = newPos;
        FlipEnemy();
    }

    // ===== ĐÁNH THƯỜNG =====
    IEnumerator DoMelee()
    {
        currentState = State.Attacking;
        nextMeleeTime = Time.time + meleeCooldown;
        if (animator) animator.SetBool(BOOL_MOVING, false);

        int r = Random.Range(0, 3);
        string trig = r == 0 ? TRIG_LEFT : (r == 1 ? TRIG_RIGHT : TRIG_KICK);
        animator.SetTrigger(trig);

        yield return new WaitForSeconds(0.4f);

        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist <= meleeRange + 0.5f)
            {
                Player p = playerTransform.GetComponent<Player>();
                if (p) p.TakeDamage(meleeDamage);
            }
        }

        yield return new WaitForSeconds(0.6f);
        currentState = State.Moving;
        if (animator) animator.SetBool(BOOL_MOVING, true);
    }

    // ===== NHẢY ĐẬP =====
    IEnumerator DoJump()
    {
        currentState = State.Jumping;
        // Đặt cooldown ngay từ đầu để tránh gọi lại liên tục
        nextJumpTime = Time.time + jumpCooldown;
        nextSpecialSkillTime = Time.time + specialSkillCooldown;
        if (animator) animator.SetBool(BOOL_MOVING, false);

        animator.SetTrigger(TRIG_JUMP);

        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTransform.position;
        Vector2 jumpDir = (targetPos - startPos).normalized;
        float actualDist = Vector2.Distance(startPos, targetPos);
        if (actualDist > jumpDistance)
            targetPos = startPos + (Vector3)(jumpDir * jumpDistance);

        // Tạm thời vô hiệu Rigidbody2D nếu có để tránh xung đột
        if (rb) rb.simulated = false;

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            Vector2 pos = Vector2.Lerp(startPos, targetPos, t);
            pos.y += jumpHeight * Mathf.Sin(t * Mathf.PI);
            transform.position = pos;
            yield return null;
        }

        if (rb) rb.simulated = true;

        // Sát thương vùng khi đáp
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D hit in hits)
        {
            Player p = hit.GetComponent<Player>();
            if (p) p.TakeDamage(jumpDamage);
        }

        yield return new WaitForSeconds(0.5f);
        currentState = State.Moving;
        if (animator) animator.SetBool(BOOL_MOVING, true);
    }

    // ===== NÉM HỘP QUÀ =====
    IEnumerator DoThrow()
    {
        currentState = State.Throwing;
        nextThrowTime = Time.time + throwCooldown;
        nextSpecialSkillTime = Time.time + specialSkillCooldown;
        if (animator) animator.SetBool(BOOL_MOVING, false);

        animator.SetTrigger(TRIG_THROW);
        yield return new WaitForSeconds(0.4f);

        if (giftBoxPrefab && throwPoint && playerTransform)
        {
            GameObject obj = Instantiate(giftBoxPrefab, throwPoint.position, Quaternion.identity);
            GiftBox gb = obj.GetComponent<GiftBox>();
            if (!gb) gb = obj.AddComponent<GiftBox>();
            Vector3 dir = (playerTransform.position - throwPoint.position).normalized;
            gb.Initialize(giftBoxDamage, giftBoxSpeed, dir);
        }

        yield return new WaitForSeconds(0.6f);
        currentState = State.Moving;
        if (animator) animator.SetBool(BOOL_MOVING, true);
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
        else if (animator && !isHurtPlaying)   // Chỉ gọi Hurt nếu chưa có Hurt đang chạy
        {
            StartCoroutine(PlayHurt());
        }
    }

    IEnumerator PlayHurt()
    {
        isHurtPlaying = true;
        // KHÔNG đổi currentState -> boss vẫn di chuyển được
        animator.SetTrigger(TRIG_HURT);

        // Chờ animation Hurt hoàn tất (tối thiểu 0.5s, có thể điều chỉnh)
        yield return new WaitForSeconds(0.5f);

        if (currentState == State.Dead) yield break;

        isHurtPlaying = false;   // Cho phép các hành động khác tiếp tục
    }

    // ===== CHẾT =====
    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        currentState = State.Dead;

        Player player = FindAnyObjectByType<Player>();
        if (player) player.AddXP(xpReward);
        if (dropChip && chipPrefab)
            Instantiate(chipPrefab, transform.position, Quaternion.identity);

        if (WaveManager.Instance)
        {
            WaveManager.Instance.HandleEnemyDeath();
            WaveManager.Instance.TryDropOil(transform.position);
        }

        if (animator) animator.SetTrigger(TRIG_DIE);

        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        if (rb) rb.simulated = false;
        enabled = false;   // Ngừng Update hoàn toàn
    }

    private void FlipEnemy()
    {
        if (!playerTransform) return;
        Vector3 s = transform.localScale;
        s.x = playerTransform.position.x < transform.position.x ? -1 : 1;
        transform.localScale = s;
    }
}