using UnityEngine;

public class EnergyBeam : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float range = 5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float lifetime = 0.5f;

    public void Initialize(float dmg, float rng, Vector2 direction)
    {
        damage = dmg;
        range = rng;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        Destroy(gameObject, lifetime);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, range, enemyLayer);
        if (hit.collider != null)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage);
        }
    }
}