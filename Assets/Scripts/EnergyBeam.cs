using UnityEngine;
using System.Collections.Generic;   // Cần cho HashSet

public class EnergyBeam : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float range = 5f;
    [SerializeField] private float lifetime = 0.5f;       // Tăng lên để beam tồn tại đủ lâu
    [SerializeField] private LayerMask enemyLayer;         // (không dùng trực tiếp, giữ lại tùy chọn)

    // Tập hợp các enemy đã bị beam này gây sát thương
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    public void Initialize(float dmg, float rng, Vector2 direction)
    {
        damage = dmg;
        range = rng;

        // Xoay beam theo hướng bắn
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Điều chỉnh collider dài ra theo hướng bắn
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(range, col.size.y);   // Dài theo hướng bắn
            col.offset = new Vector2(range / 2f, 0);     // Đặt pivot ở đầu beam
        }
        else
        {
            Debug.LogError("[EnergyBeam] Không tìm thấy BoxCollider2D trên prefab Beam! Hãy thêm component.");
        }

        Destroy(gameObject, lifetime);
    }

    // Dùng OnTriggerStay2D để liên tục kiểm tra enemy trong vùng beam
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy == null) return;

        // Nếu enemy này chưa bị beam này sát thương
        if (!hitEnemies.Contains(enemy))
        {
            enemy.TakeDamage(damage);
            hitEnemies.Add(enemy);
            Debug.Log($"[EnergyBeam] Gây {damage} sát thương cho {enemy.name}");
        }
    }
}