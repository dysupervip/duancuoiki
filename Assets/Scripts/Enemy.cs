using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    protected bool isDead = false;
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
    protected virtual void Start()
    {
        player = FindAnyObjectByType<Player>();

        if (player != null)
            currentTarget = player.transform;

        currentHp = maxHp;
        UpdateHpBar();
    }

    void FindTarget()
    {
        // Tìm pet gần nhất
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

        // Nếu có pet -> target pet
        if (closestPet != null)
        {
            currentTarget = closestPet;
            return;
        }

        // Không có pet -> target player
        if (player != null)
            currentTarget = player.transform;
    }

    protected virtual void Update()
    {
        FindTarget();
        MoveToTarget();
    }

    protected void MoveToTarget()
    {
        if (currentTarget == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            currentTarget.position,
            enemyMoveSpeed * Time.deltaTime
        );

        FlipEnemy();
    }

    protected void FlipEnemy()
    {
        if (currentTarget != null)
            transform.localScale = new Vector3(
                currentTarget.position.x < transform.position.x ? -1 : 1,
                1, 1
            );
    }

    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(currentHp, 0);
        UpdateHpBar();
        if (currentHp <= 0) Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // Cộng XP cho Player
        Player player = FindAnyObjectByType<Player>();
        if (player != null) player.AddXP(xpReward);

        // Rơi chip nếu được đánh dấu
        if (dropChip && chipPrefab != null)
        {
            Instantiate(chipPrefab, transform.position, Quaternion.identity);
        }

        // Báo WaveManager và rơi dầu
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.HandleEnemyDeath();
            WaveManager.Instance.TryDropOil(transform.position);
        }

        Destroy(gameObject);
    }

    protected void UpdateHpBar() { if (hpBar) hpBar.fillAmount = currentHp / maxHp; }

    public float GetCurrentHP() => currentHp;
    public float GetMaxHP() => maxHp;
    public void HideHpBar() { if (hpBar) hpBar.transform.parent.gameObject.SetActive(false); }
}