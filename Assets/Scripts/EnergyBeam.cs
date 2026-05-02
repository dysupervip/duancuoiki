using UnityEngine;

public class EnergyBeam : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float range = 5f;
    [SerializeField] private float lifetime = 0.3f;       // Thời gian tồn tại
    [SerializeField] private LayerMask enemyLayer;         // Vẫn giữ để kiểm tra (có thể không cần)

    private bool hasDealtDamage = false;                  // Chỉ gây sát thương 1 lần

    public void Initialize(float dmg, float rng, Vector2 direction)
    {
        damage = dmg;
        range = rng;

        // 1. Xoay beam theo hướng bắn
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2. Điều chỉnh kích thước collider (nếu có) để khớp với tầm bắn
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(range, col.size.y);   // Dài theo hướng bắn
            col.offset = new Vector2(range / 2f, 0);     // Đặt pivot ở đầu beam
        }

        // 3. Tự hủy sau thời gian lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra tag Enemy để đảm bảo
        if (collision.CompareTag("Enemy") && !hasDealtDamage)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hasDealtDamage = true;   // Mỗi beam chỉ gây sát thương 1 lần
                Debug.Log($"Beam trúng {enemy.name}, gây {damage} sát thương");
            }
        }
    }
}