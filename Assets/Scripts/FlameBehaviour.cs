using UnityEngine;

public class FlameBehaviour : MonoBehaviour
{
    [SerializeField] private float damage = 15f;
    [SerializeField] private float speed = 8f;            // Tốc độ bay
    [SerializeField] private float lifetime = 2f;          // Thời gian tồn tại
    [SerializeField] private LayerMask playerLayer;

    private bool hasDealtDamage = false;
    private Vector2 moveDirection;

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasDealtDamage && ((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hasDealtDamage = true;
            }
        }
    }
}