using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{
    protected bool isDead;
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
    protected virtual void Start()
    {
        player = FindAnyObjectByType<Player>();
        currentHp = maxHp;
        UpdateHpBar();
    }

    protected virtual void Update() => MoveToPlayer();

    protected void MoveToPlayer()
    {
        if (player != null)
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, enemyMoveSpeed * Time.deltaTime);
        FlipEnemy();
    }

    protected void FlipEnemy()
    {
        if (player != null)
        {
            float sizeX = Mathf.Abs(transform.localScale.x);
            float sizeY = Mathf.Abs(transform.localScale.y);

            transform.localScale = new Vector3(
                player.transform.position.x < transform.position.x ? -sizeX : sizeX,
                sizeY,
                transform.localScale.z
            );
        }
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