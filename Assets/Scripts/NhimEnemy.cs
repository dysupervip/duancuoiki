using UnityEngine;
using System.Collections;

public class RollingEnemy : Enemy
{
    [Header("Roll Attack")]
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Roll Movement")]
    [SerializeField] private float rollWindup = 0.15f;     // chờ animation chuyển sang cuộn
    [SerializeField] private float rollSpeed = 7f;
    [SerializeField] private float rollDuration = 0.45f;

    [Header("Roll Damage")]
    [SerializeField] private float rollDamage = 25f;
    [SerializeField] private float hitRadius = 0.7f;       // vùng gây damage quanh con nhím

    private Animator anim;
    private bool isRolling;
    private bool isDying;
    private bool hasHitPlayer;
    private float nextAttackTime;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (isDying || player == null || isRolling) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(RollAttack());
        }
        else
        {
            MoveToPlayer();
        }
    }

    private IEnumerator RollAttack()
    {
        isRolling = true;
        hasHitPlayer = false;
        nextAttackTime = Time.time + attackCooldown;

        if (anim != null)
            anim.SetTrigger("Attack");

        // Chờ một chút cho animation bắt đầu chuyển sang cuộn
        yield return new WaitForSeconds(rollWindup);

        Vector2 rollDirection = (player.transform.position - transform.position).normalized;

        float timer = 0f;

        while (timer < rollDuration)
        {
            transform.position += (Vector3)rollDirection * rollSpeed * Time.deltaTime;

            TryHitPlayer();

            timer += Time.deltaTime;
            yield return null;
        }

        isRolling = false;
    }

    private void TryHitPlayer()
    {
        if (hasHitPlayer || player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= hitRadius)
        {
            player.TakeDamage(rollDamage);
            hasHitPlayer = true;

            Debug.Log("Nhím lăn trúng Player, damage = " + rollDamage);
        }
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

        FinishDie();
    }

    public void FinishDie()
    {
        base.Die();
    }
}