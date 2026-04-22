using UnityEngine;

public class BasicEnemy : Enemy
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            player.TakeDamage();
        }
    }
}
