using UnityEngine;
using System.Collections;

public class RollEnemy : Enemy
{
    [Header("Lăn (Roll)")]
    [SerializeField] private float rollDamage = 18f;
    [SerializeField] private float rollSpeed = 12f;
    [SerializeField] private float rollDuration = 0.6f;
    [SerializeField] private float rollCooldown = 3f;
    [SerializeField] private float rollTriggerRange = 4f;

    private float nextRollTime = 0f;
    private bool isRolling = false;
    private Animator animator;
    private Rigidbody2D rb;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        if (isDead || isRolling) return;

        base.Update();

        if (currentTarget != null && Time.time >= nextRollTime)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist <= rollTriggerRange)
            {
                StartCoroutine(PerformRoll());
            }
        }
    }

    IEnumerator PerformRoll()
    {
        isRolling = true;
        nextRollTime = Time.time + rollCooldown;

        animator?.SetTrigger("Roll");

        Vector2 rollDir = (currentTarget.position - transform.position).normalized;
        if (rb != null) rb.linearVelocity = rollDir * rollSpeed;

        // Bật trigger collider
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;

        yield return new WaitForSeconds(rollDuration);

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (col) col.isTrigger = false;
        isRolling = false;
        // Tự động quay về Run (Animator sẽ chuyển Roll -> Run sau Exit Time)
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isRolling) return;

        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(rollDamage);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (currentHp > 0)
            animator?.SetTrigger("Hurt");
    }

    protected override void Die()
    {
        if (isDead) return;
        animator?.SetTrigger("Die");
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        base.Die();
    }
}