using UnityEngine;

public class Pet : MonoBehaviour
{
    [Header("Cấu hình Pet")]
    [SerializeField] private float moveSpeed = 3f;            // Tốc độ di chuyển
    [SerializeField] private float attackDamage = 5f;         // Sát thương mỗi lần chạm
    [SerializeField] private float attackCooldown = 0.5f;     // Thời gian hồi giữa các lần gây sát thương (giây)
    [SerializeField] private float lifetime = 10f;            // Thời gian tồn tại tối đa (giây)

    private Transform targetEnemy;       // Enemy gần nhất
    private float nextAttackTime;        // Thời điểm có thể gây sát thương tiếp
    private float spawnTime;             // Thời điểm được tạo ra

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnTime = Time.time;           // Ghi nhận lúc xuất hiện
        nextAttackTime = 0f;
    }

    void Update()
    {
        // Tự hủy nếu đã vượt quá thời gian sống
        if (Time.time - spawnTime >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        FindClosestEnemy();         // Tìm enemy gần nhất
        MoveTowardsTarget();        // Di chuyển đến enemy
        TryAttackTarget();          // Gây sát thương (thực hiện qua va chạm Trigger)
    }

    // Tìm enemy có tag "Enemy" gần nhất
    void FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = enemy.transform;
            }
        }
        targetEnemy = closest; // Có thể là null nếu không còn enemy nào
    }

    // Di chuyển về phía enemy mục tiêu
    void MoveTowardsTarget()
    {
        if (targetEnemy == null)
        {
            rb.linearVelocity = Vector2.zero; // Đứng yên nếu không có enemy
            return;
        }

        Vector2 direction = (targetEnemy.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // Lật mặt pet theo hướng di chuyển
        if (direction.x < 0)
            spriteRenderer.flipX = true;
        else if (direction.x > 0)
            spriteRenderer.flipX = false;
    }

    // Gây sát thương khi chạm enemy (dùng Trigger)
    void TryAttackTarget()
    {
        // Hàm này không cần code, vì ta dùng OnTriggerStay2D để gây sát thương.
        // Nhưng nếu muốn kiểm tra thêm logic gì trước khi gây sát thương, có thể thêm vào đây.
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && Time.time >= nextAttackTime)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                nextAttackTime = Time.time + attackCooldown; // Đặt thời gian hồi
            }
        }
    }

    // (Tùy chọn) Nếu muốn pet gây sát thương cả khi va chạm vật lý (không trigger), thêm OnCollisionStay2D tương tự
}