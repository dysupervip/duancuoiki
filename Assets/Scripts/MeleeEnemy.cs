using UnityEngine;
using System.Collections;

public class MeleeEnemy : Enemy
{
    [Header("Cận chiến")]
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.2f;

    private float nextAttackTime = 0f;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        // Mặc định animation Run được chạy (do Animator đặt Run là Default State)
    }

    protected override void Update()
    {
        base.Update(); // Gọi MoveToTarget, FlipEnemy

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

        yield return new WaitForSeconds(0.3f); // Chờ đến frame gây sát thương

        if (currentTarget != null)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist <= attackRange + 0.3f)
            {
                Player player = currentTarget.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(attackDamage);
                }
            }
        }

        yield return new WaitForSeconds(0.5f); // Chờ animation kết thúc
        // Tự động quay về animation Run (Animator sẽ tự chuyển vì Attack -> Run có Has Exit Time)
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