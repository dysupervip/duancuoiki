using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 25f;          // Tốc độ bay
    [SerializeField] private float timeDestroy = 0.5f;       // Thời gian tự hủy
    [SerializeField] private float damage = 10f;             // Sát thương mặc định (sẽ bị ghi đè bởi súng)
    [SerializeField] GameObject bloodPrefabs;                // Hiệu ứng máu

    /// <summary>
    /// Được gọi từ WeaponBase để truyền sát thương thực tế (có tính nâng cấp).
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    void Start()
    {
        Destroy(gameObject, timeDestroy);   // Tự hủy sau thời gian quy định
    }

    void Update()
    {
        MoveBullet();
    }

    void MoveBullet()
    {
        // Di chuyển đạn theo hướng phải của nó (giả sử đạn xoay theo hướng bắn)
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);   // Gây sát thương (đã được gán từ súng)
                // Tạo hiệu ứng máu
                GameObject blood = Instantiate(bloodPrefabs, transform.position, Quaternion.identity);
                Destroy(blood, 1f);
            }
            Destroy(gameObject); // Hủy đạn sau khi trúng
        }
    }
}