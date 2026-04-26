using UnityEngine;

public class ExplosionEnemy : Enemy
{
    [SerializeField] private GameObject explosionPrefabs;

    private void CreateExplosion()
    {
        if (explosionPrefabs != null)
        {
            Instantiate(explosionPrefabs, transform.position, Quaternion.identity);
        }
    }

    protected override void Die()
    {
        CreateExplosion();
        base.Die();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Gây sát thương cho Player (tùy chọn, dùng enterDamage có sẵn)
            Player player = collision.GetComponent<Player>();
            if (player != null)
                player.TakeDamage(enterDamage);

            Die(); // Gọi Die() để tạo vụ nổ, báo WaveManager và hủy đối tượng
        }
    }
}