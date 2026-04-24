using UnityEngine;

public class HealEnemy : Enemy
{
    [SerializeField] private float healValue = 20f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player != null)
            {
                player.TakeDamage(enterDamage);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (player!=null)
            {
                player.TakeDamage(stayDamage);
            }
        }
    }
    protected override void Die()
    {
        HealPlayer();
        base.Die();
    }
    private void HealPlayer()
    {
        if(player != null)
        {
            player.Heal(healValue);
        }
    }
}
