using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);

            if (clipInfo.Length > 0)
            {
                float duration = clipInfo[0].clip.length;

                Destroy(gameObject, duration);
            }
        }
        else
        {
            Destroy(gameObject, 1f);
        }
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
        }

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    public void DestroyExplosion()
    {
        Destroy(gameObject);
    }
}