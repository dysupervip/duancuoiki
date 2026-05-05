using UnityEngine;

public class Pet : MonoBehaviour
{
    [Header("Cấu hình Pet")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float lifetime = 10f;

    private Transform targetEnemy;
    private float nextAttackTime;
    private float spawnTime;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnTime = Time.time;
        nextAttackTime = 0f;

        if (rb == null)
        {
            Debug.LogError("[Pet] Không tìm thấy Rigidbody2D! Pet cần Rigidbody2D để di chuyển.");
        }
    }

    void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        FindClosestEnemy();
        MoveTowardsTarget();
    }

    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            targetEnemy = null;
            Debug.Log("[Pet] Không có enemy nào.");
            return;
        }

        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = enemy.transform;
            }
        }
        targetEnemy = closest;

        if (targetEnemy != null)
            Debug.Log($"[Pet] Tìm thấy enemy gần nhất: {targetEnemy.name}, khoảng cách: {closestDistance}");
        else
            Debug.Log("[Pet] Không tìm thấy enemy hợp lệ.");
    }

    void MoveTowardsTarget()
    {
        if (targetEnemy == null)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (targetEnemy.position - transform.position).normalized;
        if (rb != null)
            rb.linearVelocity = direction * moveSpeed;

        if (direction.x < 0) spriteRenderer.flipX = true;
        else if (direction.x > 0) spriteRenderer.flipX = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (this == null || gameObject == null) return;

        if (collision.CompareTag("Enemy") && Time.time >= nextAttackTime)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.TakeDamage(attackDamage);
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }
}