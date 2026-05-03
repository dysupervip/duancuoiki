using UnityEngine;

public class GiftBox : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float lifetime = 5f;

    private Vector2 moveDirection;

    public void Initialize(float dmg, float speed, Vector2 direction)
    {
        damage = dmg;
        moveSpeed = speed;
        moveDirection = direction.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}