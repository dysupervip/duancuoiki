using UnityEngine;

public class RangedBossEnemy : Enemy
{
    [Header("Ranged Attack")]
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Animator anim;
    private float nextAttackTime;
    private bool isDeadBoss;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (isDeadBoss || player == null) return;

        FlipEnemy();

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance > attackRange)
        {
            MoveToPlayer();
        }
        else
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (anim != null)
            anim.SetTrigger("Attack");
    }

    // Gọi bằng Animation Event ở frame bắn
    public void ShootProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("Thiếu Projectile Prefab / FirePoint / Player");
            return;
        }

        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        BossProjectile projectile = bullet.GetComponent<BossProjectile>();
        if (projectile != null)
        {
            Vector2 dir = player.transform.position - firePoint.position;
            projectile.SetDirection(dir);
        }
    }

    protected override void Die()
    {
        if (isDeadBoss) return;

        isDeadBoss = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        HideHpBar();

        if (anim != null)
            anim.SetTrigger("Die");

        Invoke(nameof(FinishDie), 0.5f);
    }

    public void FinishDie()
    {
        base.Die();
    }
}