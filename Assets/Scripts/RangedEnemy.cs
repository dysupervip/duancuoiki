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
    private bool isAttacking = false;   // Đang thực hiện tấn công thì đứng im
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (isDead) return;

        // Chỉ di chuyển khi không đang tấn công
        if (!isAttacking)
        {
            base.Update(); // Gọi MoveToTarget, FlipEnemy
        }

        if (currentTarget != null && Time.time >= nextAttackTime)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist <= attackRange)
            {
                StartCoroutine(PerformAttack());
            }
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;                  // Chặn di chuyển
        nextAttackTime = Time.time + attackCooldown;

        animator?.SetTrigger("Attack");

        yield return new WaitForSeconds(0.4f); // Chờ đến lúc bắn

        if (currentTarget != null && bulletPrefab != null && firePoint != null)
        {
            Vector3 dir = (currentTarget.position - firePoint.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            FireBall fireBall = bullet.GetComponent<FireBall>();
            if (fireBall != null)
            {
                fireBall.SetDirection(dir);
            }
        }

        yield return new WaitForSeconds(0.5f); // Chờ animation kết thúc

        isAttacking = false;                 // Cho phép di chuyển lại
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
    base.Die(); // Gọi Die mới của Enemy (sẽ tự delay hủy)
}

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        base.Die();
    }
}