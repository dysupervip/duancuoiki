using UnityEngine;

public class FireBall : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;       // Tốc độ bay
    [SerializeField] private float damage = 15f;         // Sát thương
    [SerializeField] private float lifeTime = 3f;        // Thời gian tự hủy

    private Vector2 moveDirection;

    /// <summary>
    /// Gọi từ RangedEnemy để thiết lập hướng bay.
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        // Xoay sprite theo hướng bay (nếu có sprite)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime); // Tự hủy sau thời gian
    }

    private void Update()
    {
        // Di chuyển đạn mỗi frame
        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Tìm Player trong đối tượng va chạm hoặc cha của nó
        Player target = collision.GetComponent<Player>();
        if (target == null)
            target = collision.GetComponentInParent<Player>();

        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Hủy nếu chạm tường hoặc mặt đất (tag tùy chỉnh)
        if (collision.CompareTag("Wall") || collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}