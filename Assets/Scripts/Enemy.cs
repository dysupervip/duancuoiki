using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    protected bool isDead = false;
    public bool IsDead => isDead;
    [SerializeField] protected float enemyMoveSpeed = 1f;
    protected Player player;
    [SerializeField] protected float maxHp = 50f;
    protected float currentHp;
    [SerializeField] private Image hpBar;
    [SerializeField] protected float enterDamage = 10f;
    [SerializeField] protected float stayDamage = 1f;
    [SerializeField] protected int xpReward = 10;
    [SerializeField] protected bool dropChip = false;
    [SerializeField] protected GameObject chipPrefab;
    protected Transform currentTarget;

    protected Animator animator;  // Thêm Animator

    protected virtual void Start()
    {
        player = FindAnyObjectByType<Player>();
        if (player != null) currentTarget = player.transform;
        animator = GetComponent<Animator>();  // Lấy Animator
        currentHp = maxHp;
        UpdateHpBar();
    }

    void FindTarget()
    {
        GameObject[] pets = GameObject.FindGameObjectsWithTag("Pet");
        float closestDistance = Mathf.Infinity;
        Transform closestPet = null;
        foreach (GameObject pet in pets)
        {
            float dist = Vector2.Distance(transform.position, pet.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPet = pet.transform;
            }
        }
        if (closestPet != null)
        {
            currentTarget = closestPet;
            return;
        }
        if (player != null) currentTarget = player.transform;
    }

    protected virtual void Update()
    {
        if (isDead) return;
        FindTarget();
        MoveToTarget();
    }

    protected void MoveToTarget()
    {
        if (currentTarget == null) return;
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, enemyMoveSpeed * Time.deltaTime);
        FlipEnemy();
    }

    protected void FlipEnemy()
    {
        if (currentTarget != null)
            transform.localScale = new Vector3(currentTarget.position.x < transform.position.x ? -1 : 1, 1, 1);
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // Kích hoạt animation Die nếu có Animator và trigger "Die"
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Cộng XP, rơi đồ, báo WaveManager...
        Player player = FindAnyObjectByType<Player>();
        if (player != null) player.AddXP(xpReward);
        if (dropChip && chipPrefab != null) Instantiate(chipPrefab, transform.position, Quaternion.identity);
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.HandleEnemyDeath();
            WaveManager.Instance.TryDropOil(transform.position);
        }

        // Hủy đối tượng sau 0.8 giây (thời gian đủ để animation Die chạy)
        Destroy(gameObject, 0.8f);

        // Vô hiệu hóa collider và script để tránh tương tác thêm
        Collider2D col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        enabled = false; // Dừng Update
    }

    protected void UpdateHpBar()
    {
        if (hpBar) hpBar.fillAmount = currentHp / maxHp;
    }

    public float GetCurrentHP() => currentHp;
    public float GetMaxHP() => maxHp;
    public void HideHpBar()
    {
        if (hpBar) hpBar.transform.parent.gameObject.SetActive(false);
    }
}