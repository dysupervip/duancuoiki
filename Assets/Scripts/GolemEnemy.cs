using UnityEngine;
using System.Collections;

public class GlEnemy : Enemy
{
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackHitDelay = 0.25f;

    private Animator anim;
    private bool touchingPlayer;
    private bool isAttacking;
    private bool isDying;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (isDying || player == null || isAttacking) return;

        if (touchingPlayer)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            base.Update(); // Enemy tự MoveToPlayer + FlipEnemy
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (anim != null)
            anim.SetTrigger("Attack");

        yield return new WaitForSeconds(attackHitDelay);

        if (touchingPlayer && player != null && !isDying)
            player.TakeDamage(enterDamage);

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    protected override void Die()
    {
        if (isDying) return;

        isDying = true;
        StopAllCoroutines();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        HideHpBar();

        if (anim != null)
            anim.SetTrigger("Die");

        Invoke(nameof(FinishDie), 0.5f);
    }

    public void FinishDie()
    {
        base.Die();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            touchingPlayer = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            touchingPlayer = false;
    }
}