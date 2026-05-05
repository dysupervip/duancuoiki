using UnityEngine;
using System.Collections;

public class RangedEnemy : Enemy
{
    [Header("Đánh xa")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float attackCooldown = 2f;

    private float nextAttackTime = 0f;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        if (currentTarget != null)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);

            if (dist <= attackRange && Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformAttack());
            }
        }
    }

    IEnumerator PerformAttack()
    {
        nextAttackTime = Time.time + attackCooldown;
        animator?.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f); // Chờ animation bắn

        if (currentTarget != null && bulletPrefab != null && firePoint != null)
        {
            Vector3 dir = (currentTarget.position - firePoint.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
            if (eb != null)
            {
                eb.SetMovementDirection(dir * bulletSpeed);
            }
            else
            {
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = dir * bulletSpeed;
            }
        }

        yield return new WaitForSeconds(0.5f);
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