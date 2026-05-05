using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float damage = 15f;
    [SerializeField] private float lifeTime = 3f;

    private Vector2 moveDirection;

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player target = collision.GetComponent<Player>();
        if (target == null)
            target = collision.GetComponentInParent<Player>();

        if (target != null)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("Wall") || collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}